using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class PathPartValueProvider : BaseValueProvider
{
	public PathPartValueProvider()
		: base(ConditionType.PathPart, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		EnableInputArguments(required: true, mayReturnMultipleValues: false, 2);
	}

	public override bool UpdateContext(IContext context)
	{
		bool result = !IsReady;
		MarkReady(ready: true);
		return result;
	}

	public override bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryValidateInput(input, out error))
		{
			return false;
		}
		PathFragment result;
		int result2;
		if (input.PositionalArgs.Length == 0)
		{
			error = "The " + Name + " token requires a path argument.";
		}
		else if (input.PositionalArgs.Length == 1)
		{
			error = $"The {Name} token requires a fragment type (one of {string.Join(", ", Enum.GetNames(typeof(PathFragment)))}).";
		}
		else if (!Enum.TryParse<PathFragment>(input.PositionalArgs[1], ignoreCase: true, out result) && !int.TryParse(input.PositionalArgs[1], out result2))
		{
			error = $"Invalid path fragment type '{input.PositionalArgs[1]}'; expected a numeric index, or one of {string.Join(", ", Enum.GetNames(typeof(PathFragment)))}.";
		}
		return error == null;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		if (!TryGetPart(input.PositionalArgs[0], input.PositionalArgs[1], out string part, out string error))
		{
			throw new InvalidOperationException(error);
		}
		return (part != null) ? InvariantSets.FromValue(part) : InvariantSets.Empty;
	}

	private bool TryGetPart(string? path, string? rawPart, out string? part, [NotNullWhen(false)] out string? error)
	{
		error = null;
		if (string.IsNullOrWhiteSpace(path))
		{
			part = null;
			return true;
		}
		if (Enum.TryParse<PathFragment>(rawPart, ignoreCase: true, out var result))
		{
			if (1 == 0)
			{
			}
			string text = result switch
			{
				PathFragment.DirectoryPath => Path.GetDirectoryName(path), 
				PathFragment.FileName => Path.GetFileName(path), 
				PathFragment.FilenameWithoutExtension => Path.GetFileNameWithoutExtension(path), 
				_ => null, 
			};
			if (1 == 0)
			{
			}
			part = text;
			if (part == null)
			{
				error = $"Invalid path fragment type '{rawPart}'; expected a numeric index, or one of {string.Join(", ", Enum.GetNames(typeof(PathFragment)))}.";
			}
			return part != null;
		}
		if (int.TryParse(rawPart, out var result2))
		{
			string[] segments = PathUtilities.GetSegments(rawPart, (int?)null);
			if (Math.Abs(result2) >= segments.Length)
			{
				part = null;
				return true;
			}
			part = ((result2 >= 0) ? segments[result2] : segments[segments.Length + result2]);
			return true;
		}
		part = null;
		error = $"Invalid path fragment type '{rawPart}'; expected a numeric index, or one of {string.Join(", ", Enum.GetNames(typeof(PathFragment)))}.";
		return false;
	}
}
