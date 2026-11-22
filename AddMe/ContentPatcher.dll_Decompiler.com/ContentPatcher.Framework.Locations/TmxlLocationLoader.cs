using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace ContentPatcher.Framework.Locations;

internal class TmxlLocationLoader
{
	private record SaveData(SaveLocation[] Locations);

	public record SaveLocation(string Name, string Objects);

	private readonly IMonitor Monitor;

	private readonly Lazy<IDictionary<string, string>> SerializedLocations;

	private readonly Lazy<XmlSerializer> LocationSerializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(GameLocation), new Type[23]
	{
		typeof(Tool),
		typeof(Duggy),
		typeof(Ghost),
		typeof(GreenSlime),
		typeof(RockCrab),
		typeof(ShadowGuy),
		typeof(Child),
		typeof(Pet),
		typeof(Dog),
		typeof(Cat),
		typeof(Horse),
		typeof(SquidKid),
		typeof(Grub),
		typeof(Fly),
		typeof(DustSpirit),
		typeof(Bug),
		typeof(BigSlime),
		typeof(BreakableContainer),
		typeof(MetalHead),
		typeof(ShadowGirl),
		typeof(Monster),
		typeof(JunimoHarvester),
		typeof(TerrainFeature)
	}));

	public TmxlLocationLoader(IMonitor monitor)
	{
		Monitor = monitor;
		SerializedLocations = new Lazy<IDictionary<string, string>>(GetSerializedLocations);
	}

	public bool TryGetLocation(string name, [NotNullWhen(true)] out GameLocation? location)
	{
		if (SerializedLocations.Value.TryGetValue(name, out string value) && TryDeserialize(name, value, out location))
		{
			return true;
		}
		location = null;
		return false;
	}

	private bool TryDeserialize(string name, string xml, [NotNullWhen(true)] out GameLocation? location)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected Obj, but got Unknown
		try
		{
			using StringReader input = new StringReader(xml);
			using XmlReader xmlReader = XmlReader.Create(input, new XmlReaderSettings
			{
				ConformanceLevel = ConformanceLevel.Auto
			});
			location = (GameLocation)LocationSerializer.Value.Deserialize(xmlReader);
			return location != null;
		}
		catch (Exception ex)
		{
			Monitor.Log("Couldn't parse the '" + name + "' location data from TMXL Map Toolkit. The location may not be migrated correctly.", (LogLevel)3);
			Monitor.Log(ex.ToString(), (LogLevel)0);
			location = null;
			return false;
		}
	}

	private Dictionary<string, string> GetSerializedLocations()
	{
		try
		{
			if (((Dictionary<string, string>)(object)SaveGame.loaded.CustomData).TryGetValue("smapi/mod-data/platonymous.tmxloader/locations", out string value) && !string.IsNullOrWhiteSpace(value))
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				SaveData saveData = JsonConvert.DeserializeObject<SaveData>(value);
				if ((object)saveData != null)
				{
					SaveLocation[] locations = saveData.Locations;
					foreach (SaveLocation saveLocation in locations)
					{
						dictionary[saveLocation.Name] = saveLocation.Objects;
					}
				}
				return dictionary;
			}
		}
		catch (Exception ex)
		{
			Monitor.Log("Couldn't parse location data from TMXL Map Toolkit; if a content pack is migrating TMXL locations to Content Patcher, they may not be migrated correctly.", (LogLevel)3);
			Monitor.Log(ex.ToString(), (LogLevel)0);
		}
		return new Dictionary<string, string>();
	}
}
