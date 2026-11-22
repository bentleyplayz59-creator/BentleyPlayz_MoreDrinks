using System;

namespace ContentPatcher.Framework.ConfigModels;

internal class CustomLocationConfig
{
	public string? Name { get; }

	public string? FromMapFile { get; }

	public string?[] MigrateLegacyNames { get; }

	public CustomLocationConfig(string? name, string? fromMapFile, string[]? migrateLegacyNames)
	{
		Name = name;
		FromMapFile = fromMapFile;
		MigrateLegacyNames = migrateLegacyNames ?? Array.Empty<string>();
	}
}
