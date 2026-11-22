using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal interface IRuntimeMigration : IMigration
{
	IAssetName? RedirectTarget(IAssetName assetName, IPatch patch);

	bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error) where T : notnull;

	bool TryApplyEditPatch<T>(IPatch patch, IAssetData asset, out string? error) where T : notnull;
}
