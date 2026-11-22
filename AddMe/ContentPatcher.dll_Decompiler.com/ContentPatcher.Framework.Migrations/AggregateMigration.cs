using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens.Json;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class AggregateMigration : IRuntimeMigration, IMigration
{
	private readonly HashSet<string> ValidVersions;

	private readonly string LatestVersion;

	private readonly IMigration[] Migrations;

	private readonly IRuntimeMigration[] RuntimeMigrations;

	public ISemanticVersion Version { get; }

	public string[] MigrationWarnings { get; }

	public AggregateMigration(ISemanticVersion version, IMigration[] migrations)
	{
		Version = version;
		HashSet<string> hashSet = new HashSet<string>();
		foreach (string item in migrations.Select((IMigration p) => p.Version.ToString()))
		{
			hashSet.Add(item);
		}
		ValidVersions = hashSet;
		LatestVersion = migrations.Last().Version.ToString();
		Migrations = migrations.Where((IMigration m) => m.Version.IsNewerThan(version)).ToArray();
		RuntimeMigrations = Migrations.OfType<IRuntimeMigration>().ToArray();
		MigrationWarnings = Migrations.SelectMany((IMigration p) => p.MigrationWarnings).Distinct().ToArray();
	}

	public bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		if (!ValidVersions.Contains(((object)new SemanticVersion(content.Format.MajorVersion, content.Format.MinorVersion, 0, (string)null, (string)null)).ToString()))
		{
			string latestVersion = LatestVersion;
			error = (content.Format.IsNewerThan(latestVersion) ? $"unsupported format version {content.Format}, you may need to update Content Patcher" : $"unsupported format version {content.Format} (expected {latestVersion})");
			return false;
		}
		IMigration[] migrations = Migrations;
		foreach (IMigration migration in migrations)
		{
			if (!migration.TryMigrateMainContent(content, out error))
			{
				return false;
			}
		}
		error = null;
		return true;
	}

	public bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
	{
		IMigration[] migrations = Migrations;
		foreach (IMigration migration in migrations)
		{
			if (!migration.TryMigrate(ref patches, out error))
			{
				return false;
			}
		}
		error = null;
		return true;
	}

	public bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		IMigration[] migrations = Migrations;
		foreach (IMigration migration in migrations)
		{
			if (!migration.TryMigrate(ref lexToken, out error))
			{
				return false;
			}
		}
		error = null;
		return true;
	}

	public bool TryMigrate(IManagedTokenString tokenStr, [NotNullWhen(false)] out string? error)
	{
		IMigration[] migrations = Migrations;
		foreach (IMigration migration in migrations)
		{
			if (!migration.TryMigrate(tokenStr, out error))
			{
				return false;
			}
		}
		error = null;
		return true;
	}

	public bool TryMigrate(TokenizableJToken tokenStructure, [NotNullWhen(false)] out string? error)
	{
		IMigration[] migrations = Migrations;
		foreach (IMigration migration in migrations)
		{
			if (!migration.TryMigrate(tokenStructure, out error))
			{
				return false;
			}
		}
		error = null;
		return true;
	}

	public bool TryMigrate(Condition condition, [NotNullWhen(false)] out string? error)
	{
		IMigration[] migrations = Migrations;
		foreach (IMigration migration in migrations)
		{
			if (!migration.TryMigrate(condition, out error))
			{
				return false;
			}
		}
		error = null;
		return true;
	}

	public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
	{
		IAssetName val = null;
		IRuntimeMigration[] runtimeMigrations = RuntimeMigrations;
		foreach (IRuntimeMigration runtimeMigration in runtimeMigrations)
		{
			val = runtimeMigration.RedirectTarget(val ?? assetName, patch) ?? val;
		}
		return val;
	}

	public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error) where T : notnull
	{
		bool result = false;
		error = null;
		IRuntimeMigration[] runtimeMigrations = RuntimeMigrations;
		foreach (IRuntimeMigration runtimeMigration in runtimeMigrations)
		{
			if (runtimeMigration.TryApplyLoadPatch(patch, assetName, ref asset, out error))
			{
				result = true;
			}
			if (error != null)
			{
				return false;
			}
		}
		return result;
	}

	public bool TryApplyEditPatch<T>(IPatch patch, IAssetData asset, out string? error) where T : notnull
	{
		bool result = false;
		error = null;
		IRuntimeMigration[] runtimeMigrations = RuntimeMigrations;
		foreach (IRuntimeMigration runtimeMigration in runtimeMigrations)
		{
			if (runtimeMigration.TryApplyEditPatch<T>(patch, asset, out error))
			{
				result = true;
			}
			if (error != null)
			{
				return false;
			}
		}
		return result;
	}
}
