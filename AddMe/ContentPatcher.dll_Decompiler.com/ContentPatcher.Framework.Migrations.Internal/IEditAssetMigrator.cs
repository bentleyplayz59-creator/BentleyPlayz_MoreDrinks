using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations.Internal;

internal interface IEditAssetMigrator
{
	bool AppliesTo(IAssetName assetName);

	IAssetName? RedirectTarget(IAssetName assetName, IPatch patch);

	bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error);

	bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error);
}
