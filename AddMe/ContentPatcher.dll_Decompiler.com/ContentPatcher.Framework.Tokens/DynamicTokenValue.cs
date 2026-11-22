using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens;

internal class DynamicTokenValue : IContextual, IContextualInfo
{
	private readonly AggregateContextual Contextuals;

	public ManagedManualToken ParentToken { get; }

	public string Name => ParentToken.ValueProvider.Name;

	public ITokenString Value { get; }

	public Condition[] Conditions { get; }

	public bool IsMutable => Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady;

	public DynamicTokenValue(ManagedManualToken parentToken, IManagedTokenString value, Condition[] conditions)
	{
		ParentToken = parentToken;
		Value = value;
		Conditions = conditions;
		Contextuals = new AggregateContextual().Add(value).Add(Conditions);
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
