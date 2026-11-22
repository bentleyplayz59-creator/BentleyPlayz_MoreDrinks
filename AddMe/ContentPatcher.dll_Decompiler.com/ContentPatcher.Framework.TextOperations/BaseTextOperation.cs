using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.TextOperations;

internal abstract class BaseTextOperation : ITextOperation, IContextual, IContextualInfo
{
	protected readonly AggregateContextual Contextuals;

	public TextOperationType Operation { get; }

	public ITokenString[] Target { get; }

	public bool IsMutable => Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady;

	public bool UpdateContext(IContext context)
	{
		return Contextuals.UpdateContext(context);
	}

	public IInvariantSet GetTokensUsed()
	{
		return Contextuals.GetTokensUsed();
	}

	public IContextualState GetDiagnosticState()
	{
		return Contextuals.GetDiagnosticState();
	}

	public TextOperationTargetRoot? GetTargetRoot()
	{
		if (Target.Length == 0)
		{
			return null;
		}
		if (!Enum.TryParse<TextOperationTargetRoot>(Target[0].Value, ignoreCase: true, out var result))
		{
			return null;
		}
		return result;
	}

	[return: NotNullIfNotNull("text")]
	public abstract string? Apply(string? text);

	protected BaseTextOperation(TextOperationType operation, ICollection<IManagedTokenString> target)
	{
		Operation = operation;
		Target = ((IEnumerable<ITokenString>)target).ToArray();
		Contextuals = new AggregateContextual().Add(target);
	}
}
