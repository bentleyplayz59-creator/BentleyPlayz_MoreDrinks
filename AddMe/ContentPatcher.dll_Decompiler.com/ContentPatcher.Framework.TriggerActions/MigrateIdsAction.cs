using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Netcode;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;

namespace ContentPatcher.Framework.TriggerActions;

internal class MigrateIdsAction
{
	private readonly Dictionary<string, string[]> JsonAssetsTypes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
	{
		["big-craftables"] = new string[1] { "(BC)" },
		["clothing"] = new string[2] { "(P)", "(S)" },
		["hats"] = new string[1] { "(H)" },
		["objects"] = new string[1] { "(O)" },
		["weapons"] = new string[1] { "(W)" }
	};

	public bool Handle(string[] args, TriggerActionContext context, [NotNullWhen(false)] out string? error)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (context.Data == null)
		{
			error = "this action must be run via Data/TriggerActions";
			return false;
		}
		if (!context.Data.HostOnly || !string.Equals(context.Data.Trigger?.Trim(), "DayStarted"))
		{
			error = "this action must be run with `\"HostOnly\": true` and `\"Trigger: \"DayStarted\"`";
			return false;
		}
		MigrateIdType migrateIdType = default;
		if (!ArgUtility.TryGetEnum<MigrateIdType>(args, 1, ref migrateIdType, ref error, "MigrateIdType type"))
		{
			return false;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string text = default;
		string value = default;
		for (int i = 2; i < args.Length; i += 2)
		{
			if (!ArgUtility.TryGet(args, i, ref text, ref error, false, "string oldId"))
			{
				return false;
			}
			if (!ArgUtility.TryGet(args, i + 1, ref value, ref error, false, "string newId"))
			{
				if (!ArgUtility.HasIndex<string>(args, i + 1))
				{
					error = $"index {i} with old ID \"{text}\" doesn't have a corresponding new ID at index {i + 1}";
				}
				return false;
			}
			dictionary[text] = value;
		}
		Farmer[] players = Game1.getAllFarmers().ToArray();
		switch (migrateIdType)
		{
		case MigrateIdType.Buildings:
			return TryMigrateBuildings(dictionary, out error);
		case MigrateIdType.CookingRecipes:
			return TryMigrateCookingRecipeIds(players, dictionary, out error);
		case MigrateIdType.CraftingRecipes:
			return TryMigrateCraftingRecipeIds(players, dictionary, out error);
		case MigrateIdType.Events:
			return TryMigrateEventIds(players, dictionary, out error);
		case MigrateIdType.FarmAnimals:
			return TryMigrateFarmAnimals(dictionary, out error);
		case MigrateIdType.Items:
			return TryMigrateItemIds(dictionary, out error);
		case MigrateIdType.Mail:
			return TryMigrateMailIds(players, dictionary, out error);
		case MigrateIdType.Songs:
			return TryMigrateSongIds(players, dictionary, out error);
		default:
			error = $"required index 1 has unknown ID type '{migrateIdType}'";
			return false;
		}
	}

	private bool TryMigrateBuildings(Dictionary<string, string> mapIds, out string? error)
	{
		CollectionExtensions.RemoveWhere<string, string>((IDictionary<string, string>)mapIds, (Func<KeyValuePair<string, string>, bool>)((KeyValuePair<string, string> pair) => Game1.buildingData.ContainsKey(pair.Key)));
		foreach (string value4 in mapIds.Values)
		{
			if (!Game1.buildingData.ContainsKey(value4))
			{
				error = "the new building type \"" + value4 + "\" doesn't match an existing building";
				return false;
			}
		}
		Utility.ForEachBuilding((Func<Building, bool>)((Building building) =>
		{
			string value = ((NetFieldBase<string, NetString>)(object)building.buildingType).Value;
			if (value != null && mapIds.TryGetValue(value, out string value2) && Game1.buildingData.TryGetValue(value2, out var value3))
			{
				((NetFieldBase<string, NetString>)(object)building.buildingType).Value = value2;
				if (building.GetIndoors() != null && value3.IndoorMap == null)
				{
					((NetFieldBase<GameLocation, NetRef<GameLocation>>)(object)building.indoors).Value = null;
					((NetFieldBase<string, NetString>)(object)building.nonInstancedIndoorsName).Value = null;
				}
			}
			return true;
		}), true);
		error = null;
		return true;
	}

	private bool TryMigrateCookingRecipeIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		foreach (Farmer player in players)
		{
			KeyValuePair<string, int>[] array = ((IEnumerable<KeyValuePair<string, int>>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.cookingRecipes).Pairs).ToArray();
			string key;
			int value;
			foreach (KeyValuePair<string, int> keyValuePair in array)
			{
				keyValuePair.Deconstruct(out key, out value);
				string text = key;
				int num = value;
				if (mapIds.TryGetValue(text, out string value2))
				{
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.cookingRecipes).Remove(text);
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.cookingRecipes).TryAdd(value2, num);
				}
			}
			KeyValuePair<string, int>[] array2 = ((IEnumerable<KeyValuePair<string, int>>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.craftingRecipes).Pairs).ToArray();
			foreach (KeyValuePair<string, int> keyValuePair in array2)
			{
				keyValuePair.Deconstruct(out key, out value);
				string text2 = key;
				int num2 = value;
				if (mapIds.TryGetValue(text2, out string value3))
				{
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.craftingRecipes).Remove(text2);
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.craftingRecipes).TryAdd(value3, num2);
				}
			}
		}
		error = null;
		return true;
	}

	private bool TryMigrateCraftingRecipeIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Farmer player in players)
		{
			KeyValuePair<string, int>[] array = ((IEnumerable<KeyValuePair<string, int>>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.craftingRecipes).Pairs).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				var (text2, num2) = array[i];
				if (mapIds.TryGetValue(text2, out string value))
				{
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.craftingRecipes).Remove(text2);
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.craftingRecipes).TryAdd(value, num2);
				}
			}
		}
		error = null;
		return true;
	}

	private bool TryMigrateEventIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
	{
		foreach (Farmer player in players)
		{
			string[] array = ((IEnumerable<string>)player.eventsSeen).ToArray();
			foreach (string text in array)
			{
				if (mapIds.TryGetValue(text, out string value))
				{
					((NetHashSet<string>)(object)player.eventsSeen).Remove(text);
					((NetHashSet<string>)(object)player.eventsSeen).Add(value);
				}
			}
		}
		error = null;
		return true;
	}

	private bool TryMigrateFarmAnimals(IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
	{
		CollectionExtensions.RemoveWhere<string, string>(mapIds, (Func<KeyValuePair<string, string>, bool>)((KeyValuePair<string, string> pair) => Game1.farmAnimalData.ContainsKey(pair.Key)));
		foreach (string value2 in mapIds.Values)
		{
			if (!Game1.farmAnimalData.ContainsKey(value2))
			{
				error = "the new farm animal type \"" + value2 + "\" doesn't match an existing animal";
				return false;
			}
		}
		Utility.ForEachLocation((Func<GameLocation, bool>)((GameLocation location) =>
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			Enumerator<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>> enumerator2 = ((NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>)(object)location.animals).Values.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					FarmAnimal current2 = enumerator2.Current;
					if (((NetFieldBase<string, NetString>)(object)current2.type).Value != null && mapIds.TryGetValue(((NetFieldBase<string, NetString>)(object)current2.type).Value, out string value))
					{
						((NetFieldBase<string, NetString>)(object)current2.type).Value = value;
						current2.ReloadTextureIfNeeded(true);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
			return true;
		}), true, false);
		error = null;
		return true;
	}

	private bool TryMigrateItemIds(IDictionary<string, string> mapRawIds, [NotNullWhen(false)] out string? error)
	{
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, ItemMetadata> mapQualifiedIds = new Dictionary<string, ItemMetadata>();
		Dictionary<string, ItemMetadata> dictionary = new Dictionary<string, ItemMetadata>();
		Lazy<Dictionary<string, Dictionary<string, string>>> jsonAssetsMap = new Lazy<Dictionary<string, Dictionary<string, string>>>(LoadJsonAssetsMap);
		string key2;
		foreach (KeyValuePair<string, string> mapRawId in mapRawIds)
		{
			mapRawId.Deconstruct(out var key, out key2);
			string text = key;
			string text2 = key2;
			ItemMetadata val = ItemRegistry.ResolveMetadata(text2);
			if (val == null)
			{
				error = "the new item ID \"" + text2 + "\" doesn't match an existing item";
				return false;
			}
			if (TryReadJsonAssetsId(text, jsonAssetsMap, out string[] oldIds, out error))
			{
				string[] array = oldIds;
				foreach (string text3 in array)
				{
					mapQualifiedIds[text3] = val;
					if (val.TypeIdentifier == "(O)")
					{
						string key3 = ItemRegistry.ManuallyQualifyItemId(text3, "", true);
						dictionary[key3] = val;
					}
				}
				continue;
			}
			if (error != null)
			{
				return false;
			}
			if (!ItemRegistry.IsQualifiedItemId(text))
			{
				error = $"the old item ID \"{text}\" must be a qualified item ID (like {"(O)"}{text}) or a Json Assets ID in the form \"JsonAssets:<type>:<name>\"";
				return false;
			}
			mapQualifiedIds[text] = val;
			if (val.TypeIdentifier == "(O)")
			{
				string key4 = ItemRegistry.ManuallyQualifyItemId(text, "", true);
				dictionary[key4] = val;
			}
		}
		Utility.ForEachItem((Func<Item, bool>)((Item item) =>
		{
			if (mapQualifiedIds.TryGetValue(item.QualifiedItemId, out ItemMetadata value10))
			{
				if (item.ParentSheetIndex == 0 || (int.TryParse(item.ItemId, out var result) && item.ParentSheetIndex == result))
				{
					item.ParentSheetIndex = value10.GetParsedData()?.SpriteIndex ?? item.ParentSheetIndex;
				}
				item.ItemId = value10.LocalItemId;
				Object val2 = (Object)(object)((item is Object) ? item : null);
				if (val2 != null)
				{
					val2.reloadSprite();
				}
			}
			return true;
		}));
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			KeyValuePair<string, int[]>[] array2 = ((IEnumerable<KeyValuePair<string, int[]>>)(object)((NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>)(object)allFarmer.archaeologyFound).Pairs).ToArray();
			int[] value;
			foreach (KeyValuePair<string, int[]> keyValuePair in array2)
			{
				keyValuePair.Deconstruct(out key2, out value);
				string text4 = key2;
				int[] array3 = value;
				if (dictionary.TryGetValue(text4, out var value2))
				{
					((NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>)(object)allFarmer.archaeologyFound).Remove(text4);
					((NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>)(object)allFarmer.archaeologyFound).TryAdd(value2.LocalItemId, array3);
				}
			}
			KeyValuePair<string, int[]>[] array4 = ((IEnumerable<KeyValuePair<string, int[]>>)(object)((NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>)(object)allFarmer.fishCaught).Pairs).ToArray();
			foreach (KeyValuePair<string, int[]> keyValuePair in array4)
			{
				keyValuePair.Deconstruct(out key2, out value);
				string text5 = key2;
				int[] array5 = value;
				if (mapQualifiedIds.TryGetValue(text5, out ItemMetadata value3))
				{
					((NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>)(object)allFarmer.fishCaught).Remove(text5);
					((NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>)(object)allFarmer.fishCaught).TryAdd(value3.QualifiedItemId, array5);
				}
			}
			int value4;
			foreach (SerializableDictionary<string, int> value11 in ((Dictionary<string, SerializableDictionary<string, int>>)(object)allFarmer.giftedItems).Values)
			{
				KeyValuePair<string, int>[] array6 = ((IEnumerable<KeyValuePair<string, int>>)value11).ToArray();
				foreach (KeyValuePair<string, int> keyValuePair2 in array6)
				{
					keyValuePair2.Deconstruct(out key2, out value4);
					string text6 = key2;
					int value5 = value4;
					if (dictionary.TryGetValue(text6, out var value6))
					{
						value11.Remove(text6);
						((Dictionary<string, int>)(object)value11).TryAdd(value6.LocalItemId, value5);
					}
				}
			}
			KeyValuePair<string, int>[] array7 = ((IEnumerable<KeyValuePair<string, int>>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.mineralsFound).Pairs).ToArray();
			foreach (KeyValuePair<string, int> keyValuePair2 in array7)
			{
				keyValuePair2.Deconstruct(out key2, out value4);
				string text7 = key2;
				int num5 = value4;
				if (dictionary.TryGetValue(text7, out var value7))
				{
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.mineralsFound).Remove(text7);
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.mineralsFound).TryAdd(value7.LocalItemId, num5);
				}
			}
			KeyValuePair<string, int>[] array8 = ((IEnumerable<KeyValuePair<string, int>>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.basicShipped).Pairs).ToArray();
			foreach (KeyValuePair<string, int> keyValuePair2 in array8)
			{
				keyValuePair2.Deconstruct(out key2, out value4);
				string text8 = key2;
				int num7 = value4;
				if (dictionary.TryGetValue(text8, out var value8))
				{
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.basicShipped).Remove(text8);
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.basicShipped).TryAdd(value8.LocalItemId, num7);
				}
			}
			KeyValuePair<string, int>[] array9 = ((IEnumerable<KeyValuePair<string, int>>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.tailoredItems).Pairs).ToArray();
			foreach (KeyValuePair<string, int> keyValuePair2 in array9)
			{
				keyValuePair2.Deconstruct(out key2, out value4);
				string text9 = key2;
				int num9 = value4;
				Item itemFromStandardTextDescription = Utility.getItemFromStandardTextDescription(text9, Game1.player, ' ');
				if (itemFromStandardTextDescription != null && mapQualifiedIds.TryGetValue(itemFromStandardTextDescription.QualifiedItemId, out ItemMetadata value9))
				{
					string standardDescriptionFromItem = Utility.getStandardDescriptionFromItem(value9.TypeIdentifier, value9.LocalItemId, false, false, 1, ' ');
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.tailoredItems).Remove(text9);
					((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)allFarmer.tailoredItems).TryAdd(standardDescriptionFromItem, num9);
				}
			}
		}
		error = null;
		return true;
	}

	private bool TryMigrateMailIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
	{
		foreach (Farmer player in players)
		{
			string[] array = ((IEnumerable<string>)player.mailReceived).ToArray();
			foreach (string text in array)
			{
				if (mapIds.TryGetValue(text, out string value))
				{
					((NetHashSet<string>)(object)player.mailReceived).Remove(text);
					((NetHashSet<string>)(object)player.mailReceived).Add(value);
				}
			}
			for (int j = 0; j < ((NetList<string, NetString>)(object)player.mailbox).Count; j++)
			{
				if (mapIds.TryGetValue(((NetList<string, NetString>)(object)player.mailbox)[j], out string value2))
				{
					((NetList<string, NetString>)(object)player.mailbox).RemoveAt(j);
					((NetList<string, NetString>)(object)player.mailbox).Insert(j, value2);
				}
			}
			string[] array2 = ((IEnumerable<string>)player.mailForTomorrow).ToArray();
			foreach (string text2 in array2)
			{
				if (mapIds.TryGetValue(text2, out string value3))
				{
					((NetHashSet<string>)(object)player.mailForTomorrow).Remove(text2);
					((NetHashSet<string>)(object)player.mailForTomorrow).Add(value3);
				}
			}
		}
		error = null;
		return true;
	}

	private bool TryMigrateSongIds(IEnumerable<Farmer> players, IDictionary<string, string> mapIds, [NotNullWhen(false)] out string? error)
	{
		foreach (Farmer player in players)
		{
			string[] array = player.songsHeard.ToArray();
			foreach (string text in array)
			{
				if (mapIds.TryGetValue(text, out string value))
				{
					player.songsHeard.Remove(text);
					player.songsHeard.Add(value);
				}
			}
		}
		error = null;
		return true;
	}

	private Dictionary<string, Dictionary<string, string>> LoadJsonAssetsMap()
	{
		Dictionary<string, Dictionary<string, string>> dictionary = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
		foreach (string key in JsonAssetsTypes.Keys)
		{
			Dictionary<string, string> collection = null;
			try
			{
				string path = Path.Combine(Constants.CurrentSavePath, "JsonAssets", "ids-" + key + ".json");
				if (File.Exists(path))
				{
					string text = File.ReadAllText(path);
					collection = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
				}
			}
			catch
			{
			}
			dictionary[key] = collection.ToNonNullCaseInsensitive();
		}
		return dictionary;
	}

	private bool TryReadJsonAssetsId(string rawId, Lazy<Dictionary<string, Dictionary<string, string>>> jsonAssetsMap, out string[] oldIds, out string? error)
	{
		if ((!(rawId?.StartsWith("JsonAssets", StringComparison.OrdinalIgnoreCase))) ?? true)
		{
			oldIds = Array.Empty<string>();
			error = null;
			return false;
		}
		string[] array = rawId.Split(':', 3, StringSplitOptions.TrimEntries);
		if (array.Length != 3)
		{
			oldIds = Array.Empty<string>();
			error = $"the old item ID \"{rawId}\" is not a valid Json Assets item specifier. It must have the form \"JsonAssets:<type>:<name>\", where the type is one of [{string.Join(", ", JsonAssetsTypes.Keys)}].";
			return false;
		}
		string text = array[1];
		string key = array[2];
		if (!JsonAssetsTypes.TryGetValue(text, out string[] value))
		{
			oldIds = Array.Empty<string>();
			error = $"the old item ID \"{rawId}\" has invalid Json Assets type '{text}'. This must be one of [{string.Join(", ", JsonAssetsTypes.Keys)}].";
			return false;
		}
		oldIds = ((jsonAssetsMap.Value.TryGetValue(text, out Dictionary<string, string> value2) && value2.TryGetValue(key, out var newId)) ? value.Select((string prefix) => prefix + newId).ToArray() : Array.Empty<string>());
		error = null;
		return true;
	}
}
