using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels;

internal class PatchMoveEntryConfig
{
	public string? ID { get; }

	public string? BeforeId { get; }

	public string? AfterId { get; }

	public string? ToPosition { get; }

	[JsonConstructor]
	public PatchMoveEntryConfig(string? id, string? beforeId, string? afterId, string? toPosition)
	{
		ID = id;
		BeforeId = beforeId;
		AfterId = afterId;
		ToPosition = toPosition;
	}

	public PatchMoveEntryConfig(PatchMoveEntryConfig other)
		: this(other.ID, other.BeforeId, other.AfterId, other.ToPosition)
	{
	}
}
