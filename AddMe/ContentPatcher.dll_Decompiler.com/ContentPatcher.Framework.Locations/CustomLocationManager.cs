using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.ConfigModels;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Locations;
using xTile;

namespace ContentPatcher.Framework.Locations;

internal class CustomLocationManager
{
	private static readonly Regex ValidNamePattern = new Regex("^[a-z0-9_]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private readonly List<CustomLocationData> CustomLocations = new List<CustomLocationData>();

	private readonly Dictionary<IAssetName, CustomLocationData> CustomLocationsByMapPath = new Dictionary<IAssetName, CustomLocationData>();

	private readonly IMonitor Monitor;

	private readonly IGameContentHelper ContentHelper;

	public CustomLocationManager(IMonitor monitor, IGameContentHelper contentHelper)
	{
		Monitor = monitor;
		ContentHelper = contentHelper;
	}

	public bool TryAddCustomLocationConfig(CustomLocationConfig? config, IContentPack contentPack, [NotNullWhen(false)] out string? error)
	{
		if (!TryParseCustomLocationConfig(config, contentPack, out CustomLocationData parsed, out error))
		{
			return false;
		}
		CustomLocations.Add(parsed);
		if (parsed.IsEnabled)
		{
			CustomLocationsByMapPath[ContentHelper.ParseAssetName(parsed.PublicMapPath)] = parsed;
		}
		return true;
	}

	public void EnforceUniqueNames()
	{
		var source = from location in CustomLocations
			from name in new string[1] { location.Name }.Concat(location.MigrateLegacyNames)
			group location by name into locationGroup
			select new
			{
				Name = locationGroup.Key,
				Locations = locationGroup.ToArray()
			} into @group
			where @group.Locations.Length > 1
			select @group;
		foreach (var item in source.OrderByHuman(p => p.Name))
		{
			string[] array = item.Locations.Select((CustomLocationData p) => p.ModName).OrderByHuman().Distinct()
				.ToArray();
			string text = ((array.Length > 1) ? $"The '{item.Name}' location was added by multiple content packs ('{string.Join("', '", array)}')." : $"The '{item.Name}' location was added multiple times in the '{array[0]}' content pack.");
			text += " This location won't be added to the game.";
			Monitor.Log(text, (LogLevel)4);
			CustomLocationData[] locations = item.Locations;
			foreach (CustomLocationData customLocationData in locations)
			{
				customLocationData.Disable(text);
			}
		}
		KeyValuePair<IAssetName, CustomLocationData>[] array2 = CustomLocationsByMapPath.Where((KeyValuePair<IAssetName, CustomLocationData> p) => !p.Value.IsEnabled).ToArray();
		foreach (KeyValuePair<IAssetName, CustomLocationData> keyValuePair in array2)
		{
			CustomLocationsByMapPath.Remove(keyValuePair.Key);
		}
	}

	public void AddTmxlLocations(IList<GameLocation>? saveLocations, IList<GameLocation> gameLocations)
	{
		int? num = saveLocations?.Count;
		if (!num.HasValue || num.GetValueOrDefault() <= 0)
		{
			return;
		}
		Lazy<IDictionary<string, GameLocation>> lazy = new Lazy<IDictionary<string, GameLocation>>(() => MapByName(saveLocations));
		Lazy<IDictionary<string, GameLocation>> lazy2 = new Lazy<IDictionary<string, GameLocation>>(() => MapByName(gameLocations));
		TmxlLocationLoader tmxlLocationLoader = new TmxlLocationLoader(Monitor);
		foreach (CustomLocationData customLocation in CustomLocations)
		{
			if (!customLocation.IsEnabled || !customLocation.HasLegacyNames)
			{
				continue;
			}
			List<string> list = new List<string>();
			string[] migrateLegacyNames = customLocation.MigrateLegacyNames;
			foreach (string text in migrateLegacyNames)
			{
				if (lazy2.Value.ContainsKey(text))
				{
					Monitor.Log($"'{customLocation.ModName}' defines a legacy location name '{text}' which matches an existing location and will be ignored.", (LogLevel)3);
				}
				else
				{
					list.Add(text);
				}
			}
			if (lazy.Value.ContainsKey(customLocation.Name))
			{
				continue;
			}
			foreach (string item in list)
			{
				if (!tmxlLocationLoader.TryGetLocation(item, out GameLocation location))
				{
					continue;
				}
				Monitor.Log($"'{customLocation.ModName}' recovered TMXL Map Toolkit location '{item}' which will be mapped to new location '{customLocation.Name}'.", (LogLevel)2);
				saveLocations.Add(location);
				((NetFieldBase<string, NetString>)(object)location.name).Value = customLocation.Name;
				lazy.Value[customLocation.Name] = location;
				break;
			}
		}
	}

	public void OnScreenInitialized()
	{
		if (CustomLocations.Count > 0)
		{
			ContentHelper.InvalidateCache("Data/Locations");
		}
	}

	public bool OnAssetRequested(AssetRequestedEventArgs e)
	{
		IAssetName nameWithoutLocale = e.NameWithoutLocale;
		CustomLocationData location;
		if (e.NameWithoutLocale.IsEquivalentTo("Data/Locations", false))
		{
			e.Edit((Action<IAssetData>)((IAssetData rawData) =>
			{
				//IL_0079: Unknown result type (might be due to invalid IL or missing references)
				//IL_007e: Unknown result type (might be due to invalid IL or missing references)
				//IL_007f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0084: Unknown result type (might be due to invalid IL or missing references)
				//IL_0095: Expected Obj, but got Unknown
				//IL_0096: Expected Obj, but got Unknown
				IDictionary<string, LocationData> data = ((IAssetData<IDictionary<string, LocationData>>)(object)rawData.AsDictionary<string, LocationData>()).Data;
				foreach (CustomLocationData customLocation in CustomLocations)
				{
					if (customLocation.IsEnabled)
					{
						if (data.ContainsKey(customLocation.Name))
						{
							Monitor.Log(customLocation.ModName + " failed to add location '" + customLocation.Name + "' because it already exists in Data/Locations.", (LogLevel)4);
						}
						else
						{
							LocationData val = new LocationData
							{
								CreateOnLoad = new CreateLocationData
								{
									MapPath = customLocation.PublicMapPath
								}
							};
							if (customLocation.HasLegacyNames)
							{
								val.FormerLocationNames = customLocation.MigrateLegacyNames.ToList();
							}
							data[customLocation.Name] = val;
						}
					}
				}
			}), (AssetEditPriority)(-1000), (string)null);
		}
		else if (CustomLocationsByMapPath.TryGetValue(nameWithoutLocale, out location))
		{
			if (!typeof(Map).IsAssignableFrom(e.DataType))
			{
				throw new InvalidOperationException($"Unexpected attempt to load asset '{nameWithoutLocale}' as a {e.DataType} asset instead of {typeof(Map)}.");
			}
			e.LoadFrom((Func<object>)(() => location.ContentPack.ModContent.Load<Map>(location.FromMapFile)), (AssetLoadPriority)int.MaxValue, location.ContentPack.Manifest.UniqueID);
			return true;
		}
		return false;
	}

	public IEnumerable<CustomLocationData> GetCustomLocationData()
	{
		return CustomLocations;
	}

	private bool TryParseCustomLocationConfig(CustomLocationConfig? config, IContentPack contentPack, [NotNullWhen(true)] out CustomLocationData? parsed, [NotNullWhen(false)] out string? error)
	{
		string text = config?.Name?.Trim();
		string text2 = config?.FromMapFile?.Trim();
		string[] migrateLegacyNames = config?.MigrateLegacyNames.Select((string p) => p?.Trim()).Where((string p) => !string.IsNullOrWhiteSpace(p)).ToArray() ?? Array.Empty<string>();
		parsed = null;
		if (string.IsNullOrWhiteSpace(text))
		{
			return Fail("the Name field is required.", out error);
		}
		if (!ValidNamePattern.IsMatch(text))
		{
			return Fail("the Name field can only contain alphanumeric or underscore characters.", out error);
		}
		if (!text.StartsWith(contentPack.Manifest.UniqueID + "_") && !text.StartsWith("Custom_"))
		{
			return Fail($"the {"Name"} field must be prefixed with the mod ID (like '{contentPack.Manifest.UniqueID}_') or 'Custom_' to avoid conflicting with current or future vanilla locations.", out error);
		}
		if (string.IsNullOrWhiteSpace(text2))
		{
			return Fail("the FromMapFile field is required.", out error);
		}
		if (!contentPack.HasFile(text2))
		{
			return Fail($"the {"FromMapFile"} field specifies a file '{text2}' which doesn't exist in the content pack.", out error);
		}
		parsed = new CustomLocationData(text, text2, migrateLegacyNames, contentPack);
		error = null;
		return true;
		static bool Fail(string reason, out string setError)
		{
			setError = reason;
			return false;
		}
	}

	private IDictionary<string, GameLocation> MapByName(IEnumerable<GameLocation> locations)
	{
		Dictionary<string, GameLocation> dictionary = new Dictionary<string, GameLocation>();
		foreach (GameLocation location in locations)
		{
			dictionary[location.NameOrUniqueName] = location;
		}
		return dictionary;
	}
}
