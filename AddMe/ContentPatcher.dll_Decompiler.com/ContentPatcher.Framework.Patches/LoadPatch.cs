using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Migrations;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace ContentPatcher.Framework.Patches;

internal class LoadPatch : Patch
{
	public LoadPatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IManagedTokenString? assetLocale, IManagedTokenString localAsset, AssetLoadPriority priority, UpdateRate updateRate, InvariantDictionary<IManagedTokenString>? inheritedLocalTokens, InvariantDictionary<IManagedTokenString>? localTokens, IEnumerable<Condition> conditions, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, Func<string, IAssetName> parseAssetName)
		: base(indexPath, path, PatchType.Load, assetName, assetLocale, (int)priority, updateRate, inheritedLocalTokens, localTokens, conditions, contentPack, migrator, parentPatch, parseAssetName, localAsset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected I4, but got Unknown
	}

	public override T Load<T>(IAssetName assetName)
	{
		return ContentPack.ModContent.Load<T>(FromAsset);
	}

	public override IEnumerable<string> GetChangeLabels()
	{
		return new _003C_003Ez__ReadOnlySingleElementList<string>("replaced asset");
	}
}
