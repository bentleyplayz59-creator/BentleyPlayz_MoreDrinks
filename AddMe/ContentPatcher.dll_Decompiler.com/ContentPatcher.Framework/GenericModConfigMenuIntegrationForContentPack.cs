using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

internal class GenericModConfigMenuIntegrationForContentPack : IGenericModConfigMenuIntegrationFor<InvariantDictionary<ConfigField>>
{
	private readonly IContentPack ContentPack;

	private readonly InvariantDictionary<ConfigField> Config;

	private readonly Func<string, IInvariantSet> ParseCommaDelimitedField;

	public GenericModConfigMenuIntegrationForContentPack(IContentPack contentPack, Func<string, IInvariantSet> parseCommaDelimitedField, InvariantDictionary<ConfigField> config)
	{
		ContentPack = contentPack;
		Config = config;
		ParseCommaDelimitedField = parseCommaDelimitedField;
	}

	public void Register(GenericModConfigMenuIntegration<InvariantDictionary<ConfigField>> menu, IMonitor monitor)
	{
		menu.Register();
		InvariantDictionary<InvariantDictionary<ConfigField>> invariantDictionary = new InvariantDictionary<InvariantDictionary<ConfigField>> { [""] = new InvariantDictionary<ConfigField>() };
		string key;
		ConfigField value;
		foreach (KeyValuePair<string, ConfigField> item in Config)
		{
			item.Deconstruct(out key, out value);
			string key2 = key;
			ConfigField configField = value;
			string key3 = configField.Section?.Trim() ?? "";
			if (!invariantDictionary.TryGetValue(key3, out var value2))
			{
				value2 = (invariantDictionary[key3] = new InvariantDictionary<ConfigField>());
			}
			value2[key2] = configField;
		}
		foreach (KeyValuePair<string, InvariantDictionary<ConfigField>> item2 in invariantDictionary)
		{
			item2.Deconstruct(out key, out var value3);
			string text = key;
			InvariantDictionary<ConfigField> invariantDictionary3 = value3;
			if (!invariantDictionary3.Any())
			{
				continue;
			}
			if (text != "")
			{
				AddSection(menu, text);
			}
			foreach (KeyValuePair<string, ConfigField> item3 in invariantDictionary3)
			{
				item3.Deconstruct(out key, out value);
				string name = key;
				ConfigField field = value;
				AddField(menu, name, field);
			}
		}
	}

	private void AddField(GenericModConfigMenuIntegration<InvariantDictionary<ConfigField>> menu, string name, ConfigField field)
	{
		if (!field.AllowValues.Any())
		{
			menu.AddTextbox(GetName, GetDescription, (InvariantDictionary<ConfigField> _) => string.Join(", ", field.Value), (InvariantDictionary<ConfigField> _, string newValue) =>
			{
				IInvariantSet invariantSet = ParseCommaDelimitedField(newValue);
				field.SetValue((field.AllowMultiple || invariantSet.Count <= 1) ? invariantSet : InvariantSets.FromValue(invariantSet.First()));
			});
			return;
		}
		if (field.AllowMultiple)
		{
			foreach (string value in field.AllowValues)
			{
				menu.AddCheckbox(() => GetName() + ": " + GetValueText(value), GetDescription, (InvariantDictionary<ConfigField> _) => field.Value.Contains(value), (InvariantDictionary<ConfigField> _, bool selected) =>
				{
					field.SetValue(selected ? field.Value.GetWith(value) : field.Value.GetWithout(value));
					if (!field.AllowBlank && !field.Value.Any())
					{
						field.SetValue(field.DefaultValues);
					}
				});
			}
			return;
		}
		if (!field.AllowBlank && field.IsBoolean())
		{
			menu.AddCheckbox(GetName, GetDescription, (InvariantDictionary<ConfigField> _) => field.Value.Contains(true.ToString()), (InvariantDictionary<ConfigField> _, bool selected) =>
			{
				field.SetValue(InvariantSets.FromValue(selected));
			});
			return;
		}
		if (!field.AllowBlank && field.IsNumericRange(out var min, out var max))
		{
			if (!int.TryParse(field.DefaultValues.FirstOrDefault(), out var defaultValue))
			{
				defaultValue = min;
			}
			menu.AddNumberField(GetName, GetDescription, (InvariantDictionary<ConfigField> _) => int.TryParse(field.Value.FirstOrDefault(), out var result) ? result : defaultValue, (InvariantDictionary<ConfigField> _, int val) =>
			{
				field.SetValue(InvariantSets.FromValue(val.ToString(CultureInfo.InvariantCulture)));
			}, min, max);
			return;
		}
		List<string> list = field.AllowValues.ToList();
		if (field.AllowBlank)
		{
			list.Insert(0, "");
		}
		menu.AddDropdown(GetName, GetDescription, (InvariantDictionary<ConfigField> _) => field.Value.FirstOrDefault() ?? "", (InvariantDictionary<ConfigField> _, string newValue) =>
		{
			field.SetValue(InvariantSets.FromValue(newValue));
		}, list.ToArray(), GetValueText);
		string GetDescription()
		{
			return TryTranslate("config." + name + ".description", field.Description);
		}
		string GetName()
		{
			return TryTranslate("config." + name + ".name", name);
		}
		string GetValueText(string text)
		{
			return TryTranslate("config." + name + ".values." + text, text);
		}
	}

	private void AddSection(GenericModConfigMenuIntegration<InvariantDictionary<ConfigField>> menu, string name)
	{
		menu.AddSectionTitle(() => TryTranslate("config.section." + name + ".name", name), () => TryTranslate("config.section." + name + ".description", null));
	}

	private string TryTranslate(string key, string? fallback)
	{
		string text = Translation.op_Implicit(ContentPack.Translation.Get(key).UsePlaceholder(false));
		return string.IsNullOrWhiteSpace(text) ? (fallback ?? "") : text;
	}
}
