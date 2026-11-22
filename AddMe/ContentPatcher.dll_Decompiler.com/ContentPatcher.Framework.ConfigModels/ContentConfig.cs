using System;
using System.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.ConfigModels;

internal class ContentConfig
{
	public ISemanticVersion? Format { get; }

	public DynamicTokenConfig[] DynamicTokens { get; }

	public InvariantDictionary<string?> AliasTokenNames { get; }

	public CustomLocationConfig?[] CustomLocations { get; }

	public PatchConfig[] Changes { get; private set; }

	public InvariantDictionary<ConfigSchemaFieldConfig?> ConfigSchema { get; }

	public ContentConfig(ISemanticVersion? format, DynamicTokenConfig?[]? dynamicTokens, InvariantDictionary<string?>? aliasTokenNames, CustomLocationConfig?[]? customLocations, PatchConfig?[]? changes, InvariantDictionary<ConfigSchemaFieldConfig?>? configSchema)
	{
		Format = format;
		DynamicTokens = dynamicTokens?.WhereNotNull().ToArray() ?? Array.Empty<DynamicTokenConfig>();
		AliasTokenNames = aliasTokenNames ?? new InvariantDictionary<string>();
		CustomLocations = customLocations ?? Array.Empty<CustomLocationConfig>();
		Changes = changes?.WhereNotNull().ToArray() ?? Array.Empty<PatchConfig>();
		ConfigSchema = configSchema ?? new InvariantDictionary<ConfigSchemaFieldConfig>();
	}
}
