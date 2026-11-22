using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class FirstValidFileValueProvider : BaseValueProvider
{
	private readonly Func<string, bool> RelativePathExists;

	public FirstValidFileValueProvider(Func<string, bool> relativePathExists)
		: base(ConditionType.FirstValidFile, mayReturnMultipleValuesForRoot: false)
	{
		RelativePathExists = relativePathExists;
		EnableInputArguments(required: true, mayReturnMultipleValues: false, null);
	}

	public override bool UpdateContext(IContext context)
	{
		bool result = !IsReady;
		MarkReady(ready: true);
		return result;
	}

	public override bool HasBoundedValues(IInputArguments input, out IInvariantSet allowedValues)
	{
		allowedValues = InvariantSets.From(input.PositionalArgs);
		return true;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return InvariantSets.From(input.PositionalArgs.Where(GetPathExists).Take(1));
	}

	private bool GetPathExists(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}
		path = PathUtilities.NormalizePath(path);
		if (Path.IsPathRooted(path))
		{
			return false;
		}
		if (!PathUtilities.IsSafeRelativePath(path))
		{
			throw new InvalidOperationException("The " + Name + " token requires a relative path and cannot contain directory climbing (../).");
		}
		return RelativePathExists(path);
	}
}
