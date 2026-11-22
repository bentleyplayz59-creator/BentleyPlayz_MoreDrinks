using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations;

internal class RemoveDelimitedTextOperation : BaseDelimitedTextOperation
{
	public RemoveDelimitedTextOperation(ICollection<IManagedTokenString> target, IManagedTokenString search, string delimiter, TextOperationReplaceMode replaceMode)
		: base(TextOperationType.RemoveDelimited, target, search, delimiter, replaceMode)
	{
	}

	protected override void ApplyToValue(IReadOnlyList<string> values, Lazy<IList<string>> mutableValues, int index)
	{
		mutableValues.Value.RemoveAt(index);
	}
}
