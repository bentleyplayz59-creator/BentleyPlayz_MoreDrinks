using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Patches;

internal class EditMapPatchProperty : IContextual, IContextualInfo
{
	private readonly AggregateContextual Contextuals;

	public ITokenString Key { get; }

	public ITokenString? Value { get; }

	public bool IsMutable => Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady;

	public EditMapPatchProperty(IManagedTokenString key, IManagedTokenString? value)
	{
		Key = key;
		Value = value;
		Contextuals = new AggregateContextual().Add(key).Add(value);
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
