using System;
using System.Collections.Generic;
using System.IO;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class AbsoluteFilePathValueProvider : BaseValueProvider
{
	private readonly string BaseDirPath;

	public AbsoluteFilePathValueProvider(string baseDirPath)
		: base(ConditionType.AbsoluteFilePath, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		BaseDirPath = baseDirPath;
		EnableInputArguments(required: true, mayReturnMultipleValues: false, null);
	}

	public override bool UpdateContext(IContext context)
	{
		bool result = !IsReady;
		MarkReady(ready: true);
		return result;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		string absolutePath = GetAbsolutePath(input.GetPositionalSegment());
		return (absolutePath != null) ? InvariantSets.FromValue(absolutePath) : InvariantSets.Empty;
	}

	private string? GetAbsolutePath(string? path)
	{
		path = PathUtilities.NormalizePath(path);
		if (string.IsNullOrWhiteSpace(path))
		{
			return path;
		}
		if (Path.IsPathRooted(path))
		{
			throw new InvalidOperationException("The " + Name + " token requires a relative path.");
		}
		if (!PathUtilities.IsSafeRelativePath(path))
		{
			throw new InvalidOperationException("The " + Name + " token requires a relative path and cannot contain directory climbing (../).");
		}
		return Path.GetFullPath(Path.Combine(BaseDirPath, path));
	}
}
