using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Patches;

internal class EditDataPatchMoveRecord : IContextual, IContextualInfo
{
	private readonly AggregateContextual Contextuals;

	public ITokenString ID { get; }

	public ITokenString? BeforeId { get; }

	public ITokenString? AfterId { get; }

	public MoveEntryPosition ToPosition { get; }

	public bool IsMutable => Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady;

	public EditDataPatchMoveRecord(IManagedTokenString id, IManagedTokenString? beforeId, IManagedTokenString? afterId, MoveEntryPosition toPosition)
	{
		ID = id;
		BeforeId = beforeId;
		AfterId = afterId;
		ToPosition = toPosition;
		Contextuals = new AggregateContextual().Add(id).Add(beforeId).Add(afterId);
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
