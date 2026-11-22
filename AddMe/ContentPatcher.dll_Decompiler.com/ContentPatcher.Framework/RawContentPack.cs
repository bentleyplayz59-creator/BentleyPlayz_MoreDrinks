using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

internal class RawContentPack
{
	private readonly Func<ContentConfig, IMigration[]> GetMigrations;

	private ContentConfig? ContentImpl;

	private IRuntimeMigration? MigratorImpl;

	public IContentPack ContentPack { get; }

	public LogPathBuilder LogPath { get; }

	public ContentConfig Content => ContentImpl ?? throw new InvalidOperationException("Must call RawContentPack.TryReloadContent before accessing the Content field.");

	public IRuntimeMigration Migrator => MigratorImpl ?? throw new InvalidOperationException("Must call RawContentPack.TryReloadContent before accessing the Migrator field.");

	public int Index { get; }

	public IManifest Manifest => ContentPack.Manifest;

	public RawContentPack(IContentPack contentPack, int index, Func<ContentConfig, IMigration[]> getMigrations)
	{
		ContentPack = contentPack;
		Index = index;
		GetMigrations = getMigrations;
		LogPath = new LogPathBuilder(contentPack.Manifest.Name);
	}

	public RawContentPack(RawContentPack copyFrom)
		: this(copyFrom.ContentPack, copyFrom.Index, copyFrom.GetMigrations)
	{
		ContentImpl = copyFrom.Content;
		MigratorImpl = copyFrom.Migrator;
	}

	public bool TryReloadContent([NotNullWhen(false)] out string? error)
	{
		ContentConfig contentConfig = ContentPack.ReadJsonFile<ContentConfig>("content.json");
		if (contentConfig == null)
		{
			error = "content pack has no content.json file";
			return false;
		}
		if (contentConfig.Format == null)
		{
			error = "content pack doesn't specify the required Format field.";
			return false;
		}
		if (!contentConfig.Changes.Any() && !contentConfig.CustomLocations.Any())
		{
			error = "content pack must specify the Changes or CustomLocations fields.";
			return false;
		}
		IRuntimeMigration runtimeMigration = new AggregateMigration(contentConfig.Format, GetMigrations(contentConfig));
		if (!runtimeMigration.TryMigrateMainContent(contentConfig, out error))
		{
			return false;
		}
		ContentImpl = contentConfig;
		MigratorImpl = runtimeMigration;
		error = null;
		return true;
	}
}
