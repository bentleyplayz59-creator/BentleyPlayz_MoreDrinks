using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Patches;

internal class EditDataPatchField : IContextual, IContextualInfo
{
	private readonly AggregateContextual Contextuals;

	public ITokenString EntryKey { get; }

	public ITokenString FieldKey { get; }

	public TokenizableJToken? Value { get; }

	public bool IsMutable => Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady;

	public EditDataPatchField(IManagedTokenString entryKey, IManagedTokenString fieldKey, TokenizableJToken? value)
	{
		EntryKey = entryKey;
		FieldKey = fieldKey;
		Value = value;
		Contextuals = new AggregateContextual().Add(entryKey).Add(fieldKey).Add(value);
	}

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
}
