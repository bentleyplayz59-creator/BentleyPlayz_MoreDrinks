using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Migrations;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches;

internal interface IPatch : IContextual, IContextualInfo
{
	int[] IndexPath { get; }

	LogPathBuilder Path { get; }

	PatchType Type { get; }

	IPatch? ParentPatch { get; }

	IContentPack ContentPack { get; }

	IRuntimeMigration Migrator { get; }

	string? FromAsset { get; }

	ITokenString? RawFromAsset { get; }

	IAssetName? TargetAsset { get; }

	string? TargetLocale { get; }

	IAssetName? TargetAssetBeforeRedirection { get; }

	ITokenString? RawTargetAsset { get; }

	int Priority { get; }

	UpdateRate UpdateRate { get; }

	Condition[] Conditions { get; }

	bool IsApplied { get; set; }

	int LastChangedTick { get; }

	bool FromAssetExists();

	T Load<T>(IAssetName assetName) where T : notnull;

	void Edit<T>(IAssetData asset) where T : notnull;

	IEnumerable<string> GetChangeLabels();
}
