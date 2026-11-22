using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

internal class ConfigFileHandler
{
	private readonly string Filename;

	private readonly Func<string?, IInvariantSet> ParseCommaDelimitedField;

	private readonly Action<IContentPack, string, string> LogWarning;

	public ConfigFileHandler(string filename, Func<string?, IInvariantSet> parseCommandDelimitedField, Action<IContentPack, string, string> logWarning)
	{
		Filename = filename;
		ParseCommaDelimitedField = parseCommandDelimitedField;
		LogWarning = logWarning;
	}

	public InvariantDictionary<ConfigField> Read(IContentPack contentPack, InvariantDictionary<ConfigSchemaFieldConfig?>? rawSchema, ISemanticVersion formatVersion)
	{
		InvariantDictionary<ConfigField> invariantDictionary = LoadConfigSchema(rawSchema, (string field, string reason) =>
		{
			LogWarning(contentPack, "ConfigSchema field '" + field + "'", reason);
		}, formatVersion);
		LoadConfigValues(contentPack, invariantDictionary, (string field, string reason) =>
		{
			LogWarning(contentPack, Filename + " > " + field, reason);
		});
		return invariantDictionary;
	}

	public void Save(IContentPack contentPack, InvariantDictionary<ConfigField> config, IModHelper modHelper)
	{
		if (config.Any())
		{
			InvariantDictionary<string> invariantDictionary = new InvariantDictionary<string>(config.ToDictionary((KeyValuePair<string, ConfigField> p) => p.Key, (KeyValuePair<string, ConfigField> p) => string.Join(", ", p.Value.Value)));
			contentPack.WriteJsonFile<InvariantDictionary<string>>(Filename, invariantDictionary);
		}
		else
		{
			FileInfo fileInfo = new FileInfo(Path.Combine(new string[1] { contentPack.GetFullPath(Filename) }));
			if (fileInfo.Exists)
			{
				fileInfo.Delete();
			}
		}
	}

	private InvariantDictionary<ConfigField> LoadConfigSchema(InvariantDictionary<ConfigSchemaFieldConfig?>? rawSchema, Action<string, string> logWarning, ISemanticVersion formatVersion)
	{
		InvariantDictionary<ConfigField> invariantDictionary = new InvariantDictionary<ConfigField>();
		if (rawSchema == null || !rawSchema.Any())
		{
			return invariantDictionary;
		}
		foreach (string key in rawSchema.Keys)
		{
			ConfigSchemaFieldConfig configSchemaFieldConfig = rawSchema[key];
			if (configSchemaFieldConfig == null)
			{
				continue;
			}
			if (string.IsNullOrWhiteSpace(key))
			{
				logWarning(key, "the config field name can't be empty.");
				continue;
			}
			if (key.Contains(":") || key.Contains("|"))
			{
				logWarning(key, $"the name '{key}' can't have input arguments ({":"} or {"|"} character).");
				continue;
			}
			if (Enum.TryParse<ConditionType>(key, ignoreCase: true, out var _))
			{
				logWarning(key, "can't use " + key + " as a config field, because it's a reserved condition key.");
				continue;
			}
			IInvariantSet invariantSet = ParseCommaDelimitedField(configSchemaFieldConfig.AllowValues);
			IInvariantSet invariantSet2 = ParseCommaDelimitedField(configSchemaFieldConfig.Default);
			if (formatVersion.IsOlderThan("1.7"))
			{
				if (!invariantSet.Any())
				{
					logWarning(key, "no AllowValues specified (and format version is less than 1.7).");
					continue;
				}
				if (!invariantSet2.Any() && !configSchemaFieldConfig.AllowBlank)
				{
					invariantSet2 = InvariantSets.FromValue(invariantSet.First());
				}
			}
			if (!configSchemaFieldConfig.AllowBlank && !invariantSet2.Any())
			{
				logWarning(key, "if AllowBlank is false, you must specify Default.");
				continue;
			}
			if (invariantSet.Any() && invariantSet2.Any())
			{
				IInvariantSet without = invariantSet2.GetWithout(invariantSet);
				if (without.Any())
				{
					logWarning(key, $"default values '{string.Join(", ", without)}' are not allowed according to {"AllowValues"}.");
					continue;
				}
			}
			if (!configSchemaFieldConfig.AllowMultiple && invariantSet2.Count > 1)
			{
				logWarning(key, "can't have multiple default values because AllowMultiple is false.");
			}
			else
			{
				invariantDictionary[key] = new ConfigField(invariantSet, invariantSet2, InvariantSets.Empty, configSchemaFieldConfig.AllowBlank, configSchemaFieldConfig.AllowMultiple, configSchemaFieldConfig.Description, configSchemaFieldConfig.Section);
			}
		}
		return invariantDictionary;
	}

	private void LoadConfigValues(IContentPack contentPack, InvariantDictionary<ConfigField> config, Action<string, string> logWarning)
	{
		if (!config.Any())
		{
			return;
		}
		InvariantDictionary<IInvariantSet> invariantDictionary = new InvariantDictionary<IInvariantSet>(from _003C_003Eh__TransparentIdentifier0 in (contentPack.ReadJsonFile<InvariantDictionary<string>>(Filename) ?? new InvariantDictionary<string>()).Select((KeyValuePair<string, string> entry) =>
			{
				KeyValuePair<string, string> keyValuePair = entry;
				return new
				{
					entry = entry,
					key = keyValuePair.Key.Trim()
				};
			})
			let value = ParseCommaDelimitedField(entry.Value)
			select new KeyValuePair<string, IInvariantSet>(key, value));
		string[] array = invariantDictionary.Keys.ExceptIgnoreCase(config.Keys).ToArray();
		foreach (string text in array)
		{
			logWarning(text, "no such field supported by this content pack.");
			invariantDictionary.Remove(text);
		}
		foreach (string key in config.Keys)
		{
			ConfigField configField = config[key];
			if (!invariantDictionary.TryGetValue(key, out var value) || (!configField.AllowBlank && !value.Any()))
			{
				invariantDictionary[key] = configField.DefaultValues;
			}
		}
		foreach (string key2 in config.Keys)
		{
			ConfigField configField2 = config[key2];
			if (configField2 != null && !configField2.AllowMultiple)
			{
				IInvariantSet value2 = configField2.Value;
				if (value2 != null && value2.Count > 1)
				{
					logWarning(key2, "field only allows a single value.");
					configField2.SetValue(configField2.DefaultValues);
					continue;
				}
			}
			if (configField2.AllowValues.Any())
			{
				IInvariantSet without = configField2.Value.GetWithout(configField2.AllowValues);
				if (without.Any())
				{
					logWarning(key2, $"found invalid values ({string.Join(", ", without)}), expected: {string.Join(", ", configField2.AllowValues)}.");
					configField2.SetValue(configField2.DefaultValues);
					continue;
				}
			}
			configField2.SetValue(invariantDictionary[key2]);
		}
	}
}
