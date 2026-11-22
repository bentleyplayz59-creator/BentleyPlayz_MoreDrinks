using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal abstract class BaseRuntimeMigration : BaseMigration, IRuntimeMigration, IMigration
{
	protected IEditAssetMigrator[] RuntimeEditDataMigrators = Array.Empty<IEditAssetMigrator>();

	public virtual IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
	{
		IEditAssetMigrator[] runtimeEditDataMigrators = RuntimeEditDataMigrators;
		foreach (IEditAssetMigrator editAssetMigrator in runtimeEditDataMigrators)
		{
			if (editAssetMigrator.AppliesTo(assetName))
			{
				IAssetName val = editAssetMigrator.RedirectTarget(assetName, patch);
				if (val != null)
				{
					return val;
				}
			}
		}
		return null;
	}

	public virtual bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error) where T : notnull
	{
		IEditAssetMigrator[] runtimeEditDataMigrators = RuntimeEditDataMigrators;
		foreach (IEditAssetMigrator editAssetMigrator in runtimeEditDataMigrators)
		{
			if (editAssetMigrator.AppliesTo(patch.TargetAssetBeforeRedirection ?? assetName))
			{
				if (editAssetMigrator.TryApplyLoadPatch(patch, assetName, ref asset, out error))
				{
					return true;
				}
				if (error != null)
				{
					return false;
				}
			}
		}
		error = null;
		return false;
	}

	public virtual bool TryApplyEditPatch<T>(IPatch patch, IAssetData asset, out string? error) where T : notnull
	{
		if (patch is EditDataPatch patch2)
		{
			IEditAssetMigrator[] runtimeEditDataMigrators = RuntimeEditDataMigrators;
			foreach (IEditAssetMigrator editAssetMigrator in runtimeEditDataMigrators)
			{
				bool hasLoggedWarning;
				if (editAssetMigrator.AppliesTo(patch.TargetAssetBeforeRedirection ?? ((IAssetInfo)asset).Name))
				{
					hasLoggedWarning = false;
					if (editAssetMigrator.TryApplyEditPatch<T>(patch2, asset, OnWarning, out error))
					{
						return true;
					}
					if (error != null)
					{
						return false;
					}
				}
				void OnWarning(string warning, IMonitor monitor)
				{
					if (!hasLoggedWarning)
					{
						monitor.Log($"Data patch \"{patch.Path}\" reported warnings when applying runtime migration {Version}. (For the mod author: see https://smapi.io/cp-migrate to avoid runtime migrations.)", (LogLevel)3);
					}
					hasLoggedWarning = true;
				}
			}
		}
		error = null;
		return false;
	}

	protected BaseRuntimeMigration(ISemanticVersion version)
		: base(version)
	{
	}
}
