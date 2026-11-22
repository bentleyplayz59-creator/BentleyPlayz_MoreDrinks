using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations;

internal interface ITextOperation : IContextual, IContextualInfo
{
	TextOperationType Operation { get; }

	ITokenString[] Target { get; }

	TextOperationTargetRoot? GetTargetRoot();

	[return: NotNullIfNotNull("text")]
	string? Apply(string? text);
}
