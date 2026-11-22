using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

internal class IndexedPatchValues
{
	public IAssetName? Target { get; set; }

	public IInvariantSet RawTokens { get; set; } = InvariantSet.Empty;

	public IInvariantSet ResolvedTokens { get; set; } = InvariantSet.Empty;
}
