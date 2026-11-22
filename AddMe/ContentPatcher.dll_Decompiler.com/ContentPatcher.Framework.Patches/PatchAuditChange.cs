using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches;

internal class PatchAuditChange
{
	public IPatch Patch { get; }

	public bool WasReady { get; }

	public string? WasFromAsset { get; }

	public IAssetName? WasTargetAsset { get; }

	public bool WillInvalidate { get; }

	public PatchAuditChange(IPatch patch, bool wasReady, string? wasFromAsset, IAssetName? wasTargetAsset, bool willInvalidate)
	{
		Patch = patch;
		WasReady = wasReady;
		WasFromAsset = wasFromAsset;
		WasTargetAsset = wasTargetAsset;
		WillInvalidate = willInvalidate;
	}
}
