using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations;

internal class ReplaceDelimitedTextOperation : BaseDelimitedTextOperation
{
	public ITokenString ReplaceWith { get; }

	public ReplaceDelimitedTextOperation(ICollection<IManagedTokenString> target, IManagedTokenString search, IManagedTokenString replaceWith, string delimiter, TextOperationReplaceMode replaceMode)
		: base(TextOperationType.ReplaceDelimited, target, search, delimiter, replaceMode)
	{
		ReplaceWith = replaceWith;
		Contextuals.Add(replaceWith);
	}

	protected override void ApplyToValue(IReadOnlyList<string> values, Lazy<IList<string>> mutableValues, int index)
	{
		mutableValues.Value[index] = ReplaceWith.Value ?? "";
	}
}
