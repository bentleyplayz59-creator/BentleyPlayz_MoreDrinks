using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations.Internal;
using ContentPatcher.Framework.Patches;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Content;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Weapons;
using StardewValley.TokenizableStrings;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_2_0 : BaseRuntimeMigration
{
	private class BigCraftableInformationMigrator : IEditAssetMigrator
	{
		private const string OldAssetName = "Data/BigCraftablesInformation";

		private const string NewAssetName = "Data/BigCraftables";

		private readonly VanillaAssetFactory<Dictionary<string, BigCraftableData>> OriginalData = new VanillaAssetFactory<Dictionary<string, BigCraftableData>>((Func<LocalizedContentManager, Dictionary<string, BigCraftableData>>)DataLoader.BigCraftables);

		private readonly HashSet<string> NumericIdsAddedIn16 = new HashSet<string> { "221", "262", "263" };

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/BigCraftablesInformation", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected Obj, but got Unknown
			return (IAssetName?)new AssetName("Data/BigCraftables", (string)null, (Nullable)null);
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, BigCraftableData> freshCopy = OriginalData.GetFreshCopy();
			Dictionary<string, string> oldFormat = GetOldFormat(freshCopy);
			Dictionary<string, string> dictionary = patch.Load<Dictionary<string, string>>(GetOldAssetName(assetName));
			MergeIntoNewFormat(freshCopy, dictionary, oldFormat);
			asset = (T)(object)freshCopy;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, BigCraftableData> data = asset.GetData<Dictionary<string, BigCraftableData>>();
			Dictionary<string, string> oldFormat = GetOldFormat(data);
			Dictionary<string, string> fromBackup = new Dictionary<string, string>(oldFormat);
			patch.Edit<Dictionary<string, string>>((IAssetData)(object)new FakeAssetData(asset, GetOldAssetName(((IAssetInfo)asset).Name), oldFormat), onWarning);
			MergeIntoNewFormat(data, oldFormat, fromBackup);
			error = null;
			return true;
		}

		private IAssetName GetOldAssetName(IAssetName newName)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected Obj, but got Unknown
			return (IAssetName)new AssetName("Data/BigCraftablesInformation", newName.LocaleCode, newName.LanguageCode);
		}

		private Dictionary<string, string> GetOldFormat(IDictionary<string, BigCraftableData> from)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = new string[10];
			foreach (var (key, val2) in from)
			{
				array[0] = val2.Name;
				array[1] = val2.Price.ToString();
				array[2] = (-300).ToString();
				array[3] = "Crafting -9";
				array[4] = TokenParser.ParseText(val2.Description, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				array[5] = val2.CanBePlacedOutdoors.ToString().ToLowerInvariant();
				array[6] = val2.CanBePlacedIndoors.ToString().ToLowerInvariant();
				array[7] = val2.Fragility.ToString();
				array[8] = val2.IsLamp.ToString().ToLowerInvariant();
				array[9] = TokenParser.ParseText(val2.DisplayName, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				dictionary[key] = string.Join('/', array);
			}
			return dictionary;
		}

		private void MergeIntoNewFormat(IDictionary<string, BigCraftableData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected Obj, but got Unknown
			foreach (string key2 in asset.Keys)
			{
				if (!from.ContainsKey(key2) && int.TryParse(key2, out var _) && !NumericIdsAddedIn16.Contains(key2))
				{
					asset.Remove(key2);
				}
			}
			foreach (var (key, text3) in from)
			{
				bool flag = false;
				if (!asset.TryGetValue(key, out BigCraftableData value))
				{
					flag = true;
					value = new BigCraftableData();
				}
				string[] array = null;
				if (fromBackup != null)
				{
					if (fromBackup.TryGetValue(key, out string value2) && value2 == text3)
					{
						continue;
					}
					array = value2?.Split('/');
				}
				string[] array2 = text3.Split('/');
				value.Name = ArgUtility.Get(array2, 0, value.Name, false);
				value.Price = ArgUtility.GetInt(array2, 1, value.Price);
				value.Description = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 4, (string)null, true), ArgUtility.Get(array, 4, (string)null, true), value.Description);
				value.CanBePlacedOutdoors = ArgUtility.GetBool(array2, 5, value.CanBePlacedOutdoors);
				value.CanBePlacedIndoors = ArgUtility.GetBool(array2, 6, value.CanBePlacedIndoors);
				value.Fragility = ArgUtility.GetInt(array2, 7, value.Fragility);
				value.IsLamp = ArgUtility.GetBool(array2, 8, value.IsLamp);
				value.DisplayName = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 9, (string)null, true), ArgUtility.Get(array, 9, (string)null, true), value.DisplayName);
				if (flag)
				{
					asset[key] = value;
				}
			}
		}
	}

	private class BlueprintsMigrator : IEditAssetMigrator
	{
		private const string OldAssetName = "Data/Blueprints";

		private const string NewAssetName = "Data/Buildings";

		private readonly HashSet<string> BuildingIdsAddedIn16 = new HashSet<string> { "Cabin", "Pet Bowl", "Farmhouse" };

		private readonly VanillaAssetFactory<Dictionary<string, BuildingData>> OriginalData = new VanillaAssetFactory<Dictionary<string, BuildingData>>((Func<LocalizedContentManager, Dictionary<string, BuildingData>>)DataLoader.Buildings);

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/Blueprints", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected Obj, but got Unknown
			return (IAssetName?)new AssetName("Data/Buildings", (string)null, (Nullable)null);
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, BuildingData> freshCopy = OriginalData.GetFreshCopy();
			Dictionary<string, string> oldFormat = GetOldFormat(freshCopy);
			Dictionary<string, string> dictionary = patch.Load<Dictionary<string, string>>(GetOldAssetName(assetName));
			MergeIntoNewFormat(freshCopy, dictionary, oldFormat);
			asset = (T)(object)freshCopy;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, BuildingData> data = asset.GetData<Dictionary<string, BuildingData>>();
			Dictionary<string, string> oldFormat = GetOldFormat(data);
			Dictionary<string, string> fromBackup = new Dictionary<string, string>(oldFormat);
			patch.Edit<Dictionary<string, string>>((IAssetData)(object)new FakeAssetData(asset, GetOldAssetName(((IAssetInfo)asset).Name), oldFormat), onWarning);
			MergeIntoNewFormat(data, oldFormat, fromBackup);
			error = null;
			return true;
		}

		private IAssetName GetOldAssetName(IAssetName newName)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected Obj, but got Unknown
			return (IAssetName)new AssetName("Data/Blueprints", newName.LocaleCode, newName.LanguageCode);
		}

		private Dictionary<string, string> GetOldFormat(IDictionary<string, BuildingData> from)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = new string[19];
			foreach (var (key, val2) in from)
			{
				array[0] = GetOldItemsRequiredField(val2);
				array[1] = val2.Size.X.ToString();
				array[2] = val2.Size.Y.ToString();
				array[3] = val2.HumanDoor.X.ToString();
				array[4] = val2.HumanDoor.Y.ToString();
				array[5] = val2.AnimalDoor.X.ToString();
				array[6] = val2.AnimalDoor.Y.ToString();
				array[7] = val2.IndoorMap;
				array[8] = TokenParser.ParseText(val2.Name, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				array[9] = TokenParser.ParseText(val2.Description, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				array[10] = "Buildings";
				array[11] = "none";
				array[12] = "0";
				array[13] = "0";
				array[14] = val2.MaxOccupants.ToString();
				array[15] = "null";
				array[16] = "Farm";
				array[17] = val2.BuildCost.ToString();
				array[18] = val2.MagicalConstruction.ToString().ToLowerInvariant();
				dictionary[key] = string.Join('/', array);
			}
			return dictionary;
		}

		private void MergeIntoNewFormat(IDictionary<string, BuildingData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Expected Obj, but got Unknown
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			foreach (string key in asset.Keys)
			{
				if (!from.ContainsKey(key) && !BuildingIdsAddedIn16.Contains(key))
				{
					asset.Remove(key);
				}
			}
			foreach (var (text3, text4) in from)
			{
				bool flag = false;
				if (!asset.TryGetValue(text3, out BuildingData value))
				{
					flag = true;
					value = new BuildingData
					{
						Name = text3,
						Description = "...",
						Texture = "Buildings\\" + text3
					};
				}
				string[] array = null;
				if (fromBackup != null)
				{
					if (fromBackup.TryGetValue(text3, out string value2) && value2 == text4)
					{
						continue;
					}
					array = value2?.Split('/');
				}
				string[] array2 = text4.Split('/');
				string text5 = ArgUtility.Get(array2, 0, (string)null, true);
				if (text5 != ArgUtility.Get(array, 0, (string)null, true))
				{
					MergeItemsRequiredFieldIntoNewFormat(value, text5);
				}
				value.Size = new Point(ArgUtility.GetInt(array2, 1, value.Size.X), ArgUtility.GetInt(array2, 2, value.Size.Y));
				value.HumanDoor = new Point(ArgUtility.GetInt(array2, 3, value.HumanDoor.X), ArgUtility.GetInt(array2, 4, value.HumanDoor.Y));
				value.AnimalDoor = new Rectangle(ArgUtility.GetInt(array2, 5, value.AnimalDoor.X), ArgUtility.GetInt(array2, 6, value.AnimalDoor.Y), 1, 1);
				value.IndoorMap = ArgUtility.Get(array2, 7, value.IndoorMap, false);
				if (string.IsNullOrWhiteSpace(value.IndoorMap) || value.IndoorMap == "null")
				{
					value.IndoorMap = null;
				}
				value.Name = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 8, (string)null, true), ArgUtility.Get(array, 8, (string)null, true), value.Name);
				value.Description = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 9, (string)null, true), ArgUtility.Get(array, 9, (string)null, true), value.Description);
				value.MaxOccupants = ArgUtility.GetInt(array2, 14, value.MaxOccupants);
				value.BuildCost = ArgUtility.GetInt(array2, 17, value.BuildCost);
				value.MagicalConstruction = ArgUtility.GetBool(array2, 18, value.MagicalConstruction);
				if (flag)
				{
					asset[text3] = value;
				}
			}
		}

		private string GetOldItemsRequiredField(BuildingData data)
		{
			int? num = data.BuildMaterials?.Count;
			if (!num.HasValue || num.GetValueOrDefault() <= 0)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (BuildingMaterial buildMaterial in data.BuildMaterials)
			{
				stringBuilder.Append(RuntimeMigrationHelper.ParseObjectId(buildMaterial.ItemId) ?? buildMaterial.ItemId).Append(' ').Append(buildMaterial.Amount)
					.Append(' ');
			}
			return stringBuilder.ToString(0, stringBuilder.Length - 1);
		}

		private void MergeItemsRequiredFieldIntoNewFormat(BuildingData data, string field)
		{
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Expected Obj, but got Unknown
			string[] array = field.Split(' ');
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			for (int i = 0; i < array.Length - 1; i += 2)
			{
				string text = ArgUtility.Get(array, i, (string)null, false);
				int value = ArgUtility.GetInt(array, i + 1, 1);
				if (text != null)
				{
					dictionary[text] = value;
				}
			}
			List<BuildingMaterial> buildMaterials = data.BuildMaterials;
			if (buildMaterials != null && buildMaterials.Count > 0)
			{
				for (int j = 0; j < data.BuildMaterials.Count; j++)
				{
					BuildingMaterial val = data.BuildMaterials[j];
					string key = RuntimeMigrationHelper.ParseObjectId(val.ItemId) ?? ItemRegistry.QualifyItemId(val.ItemId);
					if (!dictionary.TryGetValue(key, out var value2))
					{
						data.BuildMaterials.RemoveAt(j);
						j--;
					}
					else
					{
						val.Amount = value2;
						dictionary.Remove(key);
					}
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			if (data.BuildMaterials == null)
			{
				data.BuildMaterials = new List<BuildingMaterial>();
			}
			foreach (var (text3, amount) in dictionary)
			{
				string itemId = ItemRegistry.ManuallyQualifyItemId(text3, "(O)", false);
				data.BuildMaterials.Add(new BuildingMaterial
				{
					ItemId = itemId,
					Amount = amount
				});
			}
		}
	}

	private class BootsMigrator : IEditAssetMigrator
	{
		private const string AssetName = "Data/Boots";

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/Boots", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			return null;
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, string> dictionary = patch.Load<Dictionary<string, string>>(assetName);
			MigrateData(dictionary);
			asset = (T)(object)dictionary;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, string> asset2 = (Dictionary<string, string>)((IAssetData<object>)(object)asset).Data;
			patch.Edit<Dictionary<string, string>>(asset, onWarning);
			MigrateData(asset2);
			error = null;
			return true;
		}

		private void MigrateData(IDictionary<string, string> asset)
		{
			foreach (var (key, text3) in asset)
			{
				int num = RuntimeMigrationHelper.CountFields(text3);
				if (num == 6)
				{
					string text4 = text3.Substring(0, text3.IndexOf('/'));
					asset[key] = text3 + "/" + text4;
				}
			}
		}
	}

	private class CropsMigrator : IEditAssetMigrator
	{
		private const string AssetName = "Data/Crops";

		private readonly VanillaAssetFactory<Dictionary<string, CropData>> OriginalData = new VanillaAssetFactory<Dictionary<string, CropData>>((Func<LocalizedContentManager, Dictionary<string, CropData>>)DataLoader.Crops);

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/Crops", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			return null;
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, CropData> freshCopy = OriginalData.GetFreshCopy();
			Dictionary<string, string> oldFormat = GetOldFormat(freshCopy);
			Dictionary<string, string> dictionary = patch.Load<Dictionary<string, string>>(assetName);
			MergeIntoNewFormat(freshCopy, dictionary, oldFormat);
			asset = (T)(object)freshCopy;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, CropData> data = asset.GetData<Dictionary<string, CropData>>();
			Dictionary<string, string> oldFormat = GetOldFormat(data);
			Dictionary<string, string> fromBackup = new Dictionary<string, string>(oldFormat);
			patch.Edit<Dictionary<string, string>>((IAssetData)(object)new FakeAssetData(asset, ((IAssetInfo)asset).Name, oldFormat), onWarning);
			MergeIntoNewFormat(data, oldFormat, fromBackup);
			error = null;
			return true;
		}

		private Dictionary<string, string> GetOldFormat(IDictionary<string, CropData> from)
		{
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Expected I4, but got Unknown
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = new string[9];
			foreach (var (key, val2) in from)
			{
				array[0] = string.Join(' ', val2.DaysInPhase);
				array[1] = GetOldSeasonsField(val2);
				array[2] = val2.SpriteIndex.ToString();
				array[3] = val2.HarvestItemId;
				array[4] = val2.RegrowDays.ToString();
				array[5] = ((int)val2.HarvestMethod).ToString();
				array[6] = GetOldExtraHarvestField(val2);
				array[7] = val2.IsRaised.ToString().ToLowerInvariant();
				array[8] = GetOldTintColorsField(val2);
				dictionary[key] = string.Join('/', array);
			}
			return dictionary;
		}

		private void MergeIntoNewFormat(IDictionary<string, CropData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
		{
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Expected Obj, but got Unknown
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			foreach (string key2 in asset.Keys)
			{
				if (!from.ContainsKey(key2))
				{
					asset.Remove(key2);
				}
			}
			foreach (var (key, text3) in from)
			{
				bool flag = false;
				if (!asset.TryGetValue(key, out CropData value))
				{
					flag = true;
					value = new CropData();
				}
				string[] array = null;
				if (fromBackup != null)
				{
					if (fromBackup.TryGetValue(key, out string value2) && value2 == text3)
					{
						continue;
					}
					array = value2?.Split('/');
				}
				string[] array2 = text3.Split('/');
				string text4 = ArgUtility.Get(array2, 0, (string)null, true);
				if (text4 != ArgUtility.Get(array, 0, (string)null, true))
				{
					MergDaysInPhaseIntoNewFormat(value, text4);
				}
				string text5 = ArgUtility.Get(array2, 1, (string)null, true);
				if (text5 != ArgUtility.Get(array, 1, (string)null, true))
				{
					MergeSeasonsIntoNewFormat(value, text5);
				}
				value.SpriteIndex = ArgUtility.GetInt(array2, 2, value.SpriteIndex);
				value.HarvestItemId = ArgUtility.Get(array2, 3, value.HarvestItemId, false);
				value.RegrowDays = ArgUtility.GetInt(array2, 4, value.RegrowDays);
				value.HarvestMethod = ArgUtility.GetEnum<HarvestMethod>(array2, 5, value.HarvestMethod);
				string text6 = ArgUtility.Get(array2, 6, (string)null, true);
				if (text6 != null && text6 != ArgUtility.Get(array, 6, (string)null, true))
				{
					MergeExtraHarvestIntoNewFormat(value, text6);
				}
				value.IsRaised = ArgUtility.GetBool(array2, 7, value.IsRaised);
				string text7 = ArgUtility.Get(array2, 8, (string)null, true);
				if (text7 != null && text7 != ArgUtility.Get(array, 8, (string)null, true))
				{
					MergeOldTintColorsFieldIntoNewFormat(value, text7);
				}
				if (flag)
				{
					asset[key] = value;
				}
			}
		}

		private string GetOldSeasonsField(CropData entry)
		{
			return (entry.Seasons != null) ? string.Join(' ', entry.Seasons.Select((Season p) => ((object)p/*cast due to constrained. prefix*/).ToString().ToLower())) : string.Empty;
		}

		private void MergeSeasonsIntoNewFormat(CropData entry, string field)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			entry.Seasons?.Clear();
			if (entry.Seasons == null)
			{
				entry.Seasons = new List<Season>();
			}
			string[] array = field.Split(' ');
			Season item = default;
			foreach (string text in array)
			{
				if (Utility.TryParseEnum<Season>(text, ref item))
				{
					entry.Seasons.Add(item);
				}
			}
		}

		private void MergDaysInPhaseIntoNewFormat(CropData entry, string field)
		{
			entry.DaysInPhase?.Clear();
			if (entry.DaysInPhase == null)
			{
				entry.DaysInPhase = new List<int>();
			}
			string[] array = field.Split(' ');
			foreach (string s in array)
			{
				int.TryParse(s, out var result);
				entry.DaysInPhase.Add(result);
			}
		}

		private string GetOldExtraHarvestField(CropData entry)
		{
			return (entry.HarvestMaxStack > entry.HarvestMinStack || entry.HarvestMaxIncreasePerFarmingLevel > 0f || entry.ExtraHarvestChance > 0.0) ? $"true {entry.HarvestMinStack} {entry.HarvestMaxStack} {entry.HarvestMaxIncreasePerFarmingLevel.ToString().TrimStart('0')} {entry.ExtraHarvestChance.ToString().TrimStart('0')}" : "false";
		}

		private void MergeExtraHarvestIntoNewFormat(CropData entry, string field)
		{
			string[] array = field.Split(' ');
			bool flag = default;
			string text = default;
			if (ArgUtility.TryGetBool(array, 0, ref flag, ref text, "bool applies"))
			{
				int harvestMinStack = default;
				int harvestMaxStack = default;
				int num = default;
				float num2 = default;
				if (flag && ArgUtility.TryGetInt(array, 1, ref harvestMinStack, ref text, "int minStack") && ArgUtility.TryGetInt(array, 2, ref harvestMaxStack, ref text, "int maxStack") && ArgUtility.TryGetInt(array, 3, ref num, ref text, "int maxIncreasePerFarmingLevel") && ArgUtility.TryGetFloat(array, 4, ref num2, ref text, "float extraChance"))
				{
					entry.HarvestMinStack = harvestMinStack;
					entry.HarvestMaxStack = harvestMaxStack;
					entry.HarvestMaxIncreasePerFarmingLevel = num;
					entry.ExtraHarvestChance = num2;
				}
				if (!flag)
				{
					entry.HarvestMaxStack = 1;
					entry.HarvestMaxIncreasePerFarmingLevel = 0f;
					entry.ExtraHarvestChance = 0.0;
				}
			}
		}

		private string GetOldTintColorsField(CropData entry)
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			int? num = entry.TintColors?.Count;
			if (!num.HasValue || num.GetValueOrDefault() <= 0)
			{
				return "false";
			}
			StringBuilder stringBuilder = new StringBuilder("true");
			foreach (string tintColor in entry.TintColors)
			{
				Nullable val = Utility.StringToColor(tintColor);
				if (val.HasValue)
				{
					StringBuilder stringBuilder2 = stringBuilder.Append(' ');
					Color value = val.Value;
					StringBuilder stringBuilder3 = stringBuilder2.Append(value.R).Append(' ');
					value = val.Value;
					StringBuilder stringBuilder4 = stringBuilder3.Append(value.G).Append(' ');
					value = val.Value;
					stringBuilder4.Append(value.B);
				}
			}
			return stringBuilder.ToString();
		}

		private void MergeOldTintColorsFieldIntoNewFormat(CropData entry, string field)
		{
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			string[] array = field.Split(' ');
			bool flag = default;
			string text = default;
			if (!ArgUtility.TryGetBool(array, 0, ref flag, ref text, "bool applies"))
			{
				return;
			}
			if (flag)
			{
				HashSet<string> hashSet = new HashSet<string>();
				for (int i = 1; i < array.Length - 3; i += 3)
				{
					hashSet.Add($"{array[i]} {array[i + 1]} {array[i + 2]}");
				}
				List<string> tintColors = entry.TintColors;
				if (tintColors != null && tintColors.Count > 0)
				{
					for (int j = 0; j < entry.TintColors.Count; j++)
					{
						Nullable val = Utility.StringToColor(entry.TintColors[j]);
						object obj;
						if (!val.HasValue)
						{
							obj = null;
						}
						else
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
							Color value = val.Value;
							defaultInterpolatedStringHandler.AppendFormatted(value.R);
							defaultInterpolatedStringHandler.AppendLiteral(" ");
							value = val.Value;
							defaultInterpolatedStringHandler.AppendFormatted(value.G);
							defaultInterpolatedStringHandler.AppendLiteral(" ");
							value = val.Value;
							defaultInterpolatedStringHandler.AppendFormatted(value.B);
							obj = defaultInterpolatedStringHandler.ToStringAndClear();
						}
						string text2 = (string)obj;
						if (text2 == null || !hashSet.Remove(text2))
						{
							entry.TintColors.RemoveAt(j);
							j--;
						}
					}
				}
				if (hashSet.Count > 0)
				{
					if (entry.TintColors == null)
					{
						entry.TintColors = new List<string>();
					}
					entry.TintColors.AddRange(hashSet);
				}
			}
			else
			{
				entry.TintColors?.Clear();
			}
		}
	}

	private class FurnitureMigrator : IEditAssetMigrator
	{
		private const string AssetName = "Data/Furniture";

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/Furniture", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			return null;
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, string> dictionary = patch.Load<Dictionary<string, string>>(assetName);
			MigrateData(dictionary);
			asset = (T)(object)dictionary;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, string> asset2 = (Dictionary<string, string>)((IAssetData<object>)(object)asset).Data;
			patch.Edit<Dictionary<string, string>>(asset, onWarning);
			MigrateData(asset2);
			error = null;
			return true;
		}

		private void MigrateData(IDictionary<string, string> asset)
		{
			foreach (var (key, text3) in asset)
			{
				int num = RuntimeMigrationHelper.CountFields(text3);
				if ((uint)(num - 6) <= 1u)
				{
					string text4 = text3.Substring(0, text3.IndexOf('/'));
					asset[key] = text3 + ((num == 6) ? ("/-1/" + text4) : ("/" + text4));
				}
			}
		}
	}

	private class LocationsMigrator : IEditAssetMigrator
	{
		private const string AssetName = "Data/Locations";

		private readonly Season[] ValidSeasons;

		private readonly Dictionary<string, IReadOnlySet<Season>?> ParseSeasonsCache;

		private readonly VanillaAssetFactory<Dictionary<string, LocationData>> OriginalData;

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/Locations", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			return null;
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, LocationData> freshCopy = OriginalData.GetFreshCopy();
			Dictionary<string, string> oldFormat = GetOldFormat(freshCopy, out HashSet<string> skippedDueToNoData);
			Dictionary<string, string> temp = patch.Load<Dictionary<string, string>>(assetName);
			MergeIntoNewFormat(freshCopy, temp, oldFormat, skippedDueToNoData, patch.ContentPack.Manifest.UniqueID);
			asset = (T)(object)freshCopy;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, LocationData> data = asset.GetData<Dictionary<string, LocationData>>();
			Dictionary<string, string> oldFormat = GetOldFormat(data, out HashSet<string> skippedDueToNoData);
			Dictionary<string, string> tempBackup = new Dictionary<string, string>(oldFormat);
			patch.Edit<Dictionary<string, string>>((IAssetData)(object)new FakeAssetData(asset, ((IAssetInfo)asset).Name, oldFormat), onWarning);
			MergeIntoNewFormat(data, oldFormat, tempBackup, skippedDueToNoData, patch.ContentPack.Manifest.UniqueID);
			error = null;
			return true;
		}

		private Dictionary<string, string> GetOldFormat(IDictionary<string, LocationData> from, out HashSet<string> skippedDueToNoData)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			skippedDueToNoData = new HashSet<string>();
			string[] array = new string[9];
			foreach (KeyValuePair<string, LocationData> item in from)
			{
				string text = item.Key;
				LocationData value = item.Value;
				if (text == "Farm_Standard")
				{
					text = "Farm";
				}
				else if (IsReservedLocationName(text))
				{
					continue;
				}
				GetForageChancesFromAsset(value.Forage, out Dictionary<string, double> springForage, out Dictionary<string, double> summerForage, out Dictionary<string, double> fallForage, out Dictionary<string, double> winterForage);
				GetFishFromNewList(value.Fish, out HashSet<FishSpawnValue> springFish, out HashSet<FishSpawnValue> summerFish, out HashSet<FishSpawnValue> fallFish, out HashSet<FishSpawnValue> winterFish);
				GetArtifactChancesFromAsset(value.ArtifactSpots, out Dictionary<string, double> artifacts);
				if (springForage.Count == 0 && summerForage.Count == 0 && fallForage.Count == 0 && winterForage.Count == 0 && springFish.Count == 0 && summerFish.Count == 0 && fallFish.Count == 0 && winterFish.Count == 0 && artifacts.Count == 0)
				{
					skippedDueToNoData.Add(text);
					continue;
				}
				array[0] = ConvertToOldArtifactOrForageString(springForage);
				array[1] = ConvertToOldArtifactOrForageString(summerForage);
				array[2] = ConvertToOldArtifactOrForageString(fallForage);
				array[3] = ConvertToOldArtifactOrForageString(winterForage);
				array[4] = ConvertToOldFishString(text, springFish);
				array[5] = ConvertToOldFishString(text, summerFish);
				array[6] = ConvertToOldFishString(text, fallFish);
				array[7] = ConvertToOldFishString(text, winterFish);
				array[8] = ConvertToOldArtifactOrForageString(artifacts);
				dictionary[text] = string.Join('/', array);
			}
			return dictionary;
		}

		private void MergeIntoNewFormat(IDictionary<string, LocationData> asset, Dictionary<string, string> temp, Dictionary<string, string>? tempBackup, IReadOnlySet<string>? skippedDueToNoData, string modId)
		{
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Expected Obj, but got Unknown
			//IL_0124: Expected Obj, but got Unknown
			foreach (string key in asset.Keys)
			{
				if (!IsReservedLocationName(key) && !temp.ContainsKey(key) && !(skippedDueToNoData?.Contains(key) ?? false))
				{
					asset.Remove(key);
				}
			}
			foreach (var (text3, text4) in temp)
			{
				string text5 = text3;
				if (text5 == "Farm")
				{
					text5 = "Farm_Standard";
				}
				else if (IsReservedLocationName(text5))
				{
					continue;
				}
				bool flag = false;
				if (!asset.TryGetValue(text5, out LocationData value))
				{
					flag = true;
					value = new LocationData
					{
						CreateOnLoad = new CreateLocationData
						{
							MapPath = "Maps\\" + text5
						}
					};
				}
				if (tempBackup == null || !tempBackup.TryGetValue(text3, out string value2) || !(value2 == text4))
				{
					string[] fromFields = text4.Split('/');
					if (!(text5 == "Farm_Standard"))
					{
						MergeForageIntoNewFormat(text5, fromFields, value, modId);
						MergeFishIntoNewFormat(text5, fromFields, value, modId);
					}
					MergeArtifactsIntoNewFormat(fromFields, value, modId);
					if (flag)
					{
						asset[text5] = value;
					}
				}
			}
		}

		private bool IsReservedLocationName(string locationName)
		{
			return locationName == "Default" || (locationName?.StartsWith("Farm_") ?? false);
		}

		private void MergeForageIntoNewFormat(string locationName, string[] fromFields, LocationData targetLocationEntry, string modId)
		{
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			Dictionary<string, double> fromSpringForage = GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 0, (string)null, true));
			Dictionary<string, double> fromSummerForage = GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 1, (string)null, true));
			Dictionary<string, double> fromFallForage = GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 2, (string)null, true));
			Dictionary<string, double> fromWinterForage = GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 3, (string)null, true));
			List<SpawnForageData> forage = targetLocationEntry.Forage;
			if (forage != null && forage.Count > 0)
			{
				for (int i = 0; i < targetLocationEntry.Forage.Count; i++)
				{
					SpawnForageData val = targetLocationEntry.Forage[i];
					string text = RuntimeMigrationHelper.ParseObjectId(((GenericSpawnItemData)val).ItemId);
					IReadOnlySet<Season> readOnlySet = ParseEffectiveSeasons(val.Season, ((GenericSpawnItemDataWithCondition)val).Condition);
					if (text == null || readOnlySet == null)
					{
						continue;
					}
					Dictionary<Season, double> dictionary = new Dictionary<Season, double>();
					double? num = null;
					bool flag = true;
					foreach (Season item in readOnlySet)
					{
						double valueOrDefault = GetListForSeason(item).GetValueOrDefault(text);
						if (valueOrDefault > 0.0)
						{
							double valueOrDefault2 = num.GetValueOrDefault();
							if (!num.HasValue)
							{
								valueOrDefault2 = valueOrDefault;
								num = valueOrDefault2;
							}
							flag = flag && num == valueOrDefault;
							dictionary[item] = valueOrDefault;
						}
					}
					if (!readOnlySet.SetEquals(dictionary.Keys))
					{
						HashSet<Season> hashSet = new HashSet<Season>(readOnlySet);
						hashSet.IntersectWith(dictionary.Keys);
						RemoveFromSeasons(text, hashSet);
						SetSeasons(locationName, targetLocationEntry.Forage, val, hashSet, out var wasRemoved);
						if (wasRemoved)
						{
							i--;
							continue;
						}
						readOnlySet = hashSet;
					}
					if (flag)
					{
						val.Chance = num.Value;
						RemoveFromSeasons(text, readOnlySet);
					}
				}
			}
			SaveForSeason((Season)0, fromSpringForage);
			SaveForSeason((Season)1, fromSummerForage);
			SaveForSeason((Season)2, fromFallForage);
			SaveForSeason((Season)3, fromWinterForage);
			Dictionary<string, double> GetListForSeason(Season season)
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Expected I4, but got Unknown
				if (1 == 0)
				{
				}
				Dictionary<string, double> result = (int)season switch
				{
					0 => fromSpringForage, 
					1 => fromSummerForage, 
					2 => fromFallForage, 
					_ => fromWinterForage, 
				};
				if (1 == 0)
				{
				}
				return result;
			}
			void RemoveFromSeasons(string objectId, IEnumerable<Season> seasons)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				foreach (Season season in seasons)
				{
					GetListForSeason(season).Remove(objectId);
				}
			}
			void SaveForSeason(Season season, Dictionary<string, double> fromForage)
			{
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0073: Unknown result type (might be due to invalid IL or missing references)
				//IL_009a: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e9: Expected Obj, but got Unknown
				if (fromForage.Count > 0)
				{
					LocationData val2 = targetLocationEntry;
					if (val2.Forage == null)
					{
						val2.Forage = new List<SpawnForageData>();
					}
					foreach (var (text3, chance) in fromForage)
					{
						string text4 = "(O)" + text3;
						targetLocationEntry.Forage.Add(new SpawnForageData
						{
							Id = $"{modId}_{season}_{text4}",
							ItemId = text4,
							Chance = chance,
							Season = season
						});
					}
				}
			}
		}

		private void MergeFishIntoNewFormat(string locationName, string[] fromFields, LocationData targetLocationEntry, string modId)
		{
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			HashSet<FishSpawnValue> fromSpringFish = GetValuesFromOldFishList(locationName, ArgUtility.Get(fromFields, 4, (string)null, true));
			HashSet<FishSpawnValue> fromSummerFish = GetValuesFromOldFishList(locationName, ArgUtility.Get(fromFields, 5, (string)null, true));
			HashSet<FishSpawnValue> fromFallFish = GetValuesFromOldFishList(locationName, ArgUtility.Get(fromFields, 6, (string)null, true));
			HashSet<FishSpawnValue> fromWinterFish = GetValuesFromOldFishList(locationName, ArgUtility.Get(fromFields, 7, (string)null, true));
			List<SpawnFishData> fish = targetLocationEntry.Fish;
			if (fish != null && fish.Count > 0)
			{
				for (int i = 0; i < targetLocationEntry.Fish.Count; i++)
				{
					SpawnFishData val = targetLocationEntry.Fish[i];
					string text = RuntimeMigrationHelper.ParseObjectId(((GenericSpawnItemData)val).ItemId);
					IReadOnlySet<Season> readOnlySet = ParseEffectiveSeasons(val.Season, ((GenericSpawnItemDataWithCondition)val).Condition);
					if (text == null || readOnlySet == null)
					{
						continue;
					}
					FishSpawnValue fishSpawnValue = new FishSpawnValue(text, val.FishAreaId);
					HashSet<Season> hashSet = new HashSet<Season>();
					foreach (Season item in readOnlySet)
					{
						if (GetListForSeason(item).Contains(fishSpawnValue))
						{
							hashSet.Add(item);
						}
					}
					if (!readOnlySet.SetEquals(hashSet))
					{
						SetSeasons(locationName, targetLocationEntry.Fish, val, hashSet, out var wasRemoved);
						if (wasRemoved)
						{
							i--;
							continue;
						}
						readOnlySet = hashSet;
					}
					RemoveFromSeasons(fishSpawnValue, readOnlySet);
				}
			}
			SaveForSeason((Season)0, fromSpringFish);
			SaveForSeason((Season)1, fromSummerFish);
			SaveForSeason((Season)2, fromFallFish);
			SaveForSeason((Season)3, fromWinterFish);
			HashSet<FishSpawnValue> GetListForSeason(Season season)
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Expected I4, but got Unknown
				if (1 == 0)
				{
				}
				HashSet<FishSpawnValue> result = (int)season switch
				{
					0 => fromSpringFish, 
					1 => fromSummerFish, 
					2 => fromFallFish, 
					_ => fromWinterFish, 
				};
				if (1 == 0)
				{
				}
				return result;
			}
			void RemoveFromSeasons(FishSpawnValue key, IEnumerable<Season> seasons)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				foreach (Season season in seasons)
				{
					GetListForSeason(season).Remove(key);
				}
			}
			void SaveForSeason(Season season, HashSet<FishSpawnValue> fromFish)
			{
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
				//IL_0104: Expected Obj, but got Unknown
				if (fromFish.Count > 0)
				{
					LocationData val2 = targetLocationEntry;
					if (val2.Fish == null)
					{
						val2.Fish = new List<SpawnFishData>();
					}
					foreach (var (text4, text5) in fromFish)
					{
						string text6 = "(O)" + text4;
						targetLocationEntry.Fish.Add(new SpawnFishData
						{
							Id = $"{modId}_{season}_{text6}{((text5 != null) ? ("_" + text5) : "")}",
							ItemId = text6,
							FishAreaId = text5,
							Season = season
						});
					}
				}
			}
		}

		private void MergeArtifactsIntoNewFormat(string[] fromFields, LocationData targetLocationEntry, string modId)
		{
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Expected Obj, but got Unknown
			Dictionary<string, double> chancesFromOldArtifactOrForageList = GetChancesFromOldArtifactOrForageList(ArgUtility.Get(fromFields, 8, (string)null, true));
			List<ArtifactSpotDropData> artifactSpots = targetLocationEntry.ArtifactSpots;
			if (artifactSpots != null && artifactSpots.Count > 0)
			{
				for (int i = 0; i < targetLocationEntry.ArtifactSpots.Count; i++)
				{
					ArtifactSpotDropData val = targetLocationEntry.ArtifactSpots[i];
					string text = RuntimeMigrationHelper.ParseObjectId(((GenericSpawnItemData)val).ItemId);
					IReadOnlySet<Season> readOnlySet = ParseEffectiveSeasons(null, ((GenericSpawnItemDataWithCondition)val).Condition);
					if (text != null && readOnlySet != null && readOnlySet.Count == 4)
					{
						if (!chancesFromOldArtifactOrForageList.TryGetValue(text, out var value))
						{
							targetLocationEntry.ArtifactSpots.RemoveAt(i);
							i--;
						}
						else
						{
							val.Chance = value;
							chancesFromOldArtifactOrForageList.Remove(text);
						}
					}
				}
			}
			if (chancesFromOldArtifactOrForageList.Count <= 0)
			{
				return;
			}
			if (targetLocationEntry.ArtifactSpots == null)
			{
				targetLocationEntry.ArtifactSpots = new List<ArtifactSpotDropData>();
			}
			foreach (var (text3, chance) in chancesFromOldArtifactOrForageList)
			{
				string text4 = "(O)" + text3;
				targetLocationEntry.ArtifactSpots.Add(new ArtifactSpotDropData
				{
					Id = modId + "_" + text4,
					ItemId = text4,
					Chance = chance
				});
			}
		}

		private void SetSeasons(string locationName, List<SpawnForageData> list, SpawnForageData entry, HashSet<Season> seasons, out bool wasRemoved)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			wasRemoved = false;
			switch (seasons.Count)
			{
			case 0:
				list.Remove(entry);
				wasRemoved = true;
				return;
			case 1:
				entry.Season = seasons.First();
				((GenericSpawnItemDataWithCondition)entry).Condition = null;
				return;
			}
			entry.Season = null;
			((GenericSpawnItemDataWithCondition)entry).Condition = ((((GenericSpawnItemDataWithCondition)entry).Condition?.Trim().StartsWith("SEASON", StringComparison.OrdinalIgnoreCase) ?? false) ? ("SEASON " + string.Join(' ', seasons)) : $"{"LOCATION_SEASON"} {locationName} {string.Join(' ', seasons)}");
		}

		private void SetSeasons(string locationName, List<SpawnFishData> list, SpawnFishData entry, HashSet<Season> seasons, out bool wasRemoved)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			wasRemoved = false;
			switch (seasons.Count)
			{
			case 0:
				list.Remove(entry);
				wasRemoved = true;
				return;
			case 1:
				entry.Season = seasons.First();
				((GenericSpawnItemDataWithCondition)entry).Condition = null;
				return;
			}
			entry.Season = null;
			((GenericSpawnItemDataWithCondition)entry).Condition = ((((GenericSpawnItemDataWithCondition)entry).Condition?.Trim().StartsWith("SEASON", StringComparison.OrdinalIgnoreCase) ?? false) ? ("SEASON " + string.Join(' ', seasons)) : $"{"LOCATION_SEASON"} {locationName} {string.Join(' ', seasons)}");
		}

		private Dictionary<string, double> GetChancesFromOldArtifactOrForageList(string rawData)
		{
			Dictionary<string, double> dictionary = new Dictionary<string, double>();
			string[] array = rawData.Split(' ');
			for (int i = 0; i < array.Length - 1; i += 2)
			{
				string key = array[i];
				if (double.TryParse(array[i + 1], out var result))
				{
					dictionary[key] = result;
				}
			}
			return dictionary;
		}

		private HashSet<FishSpawnValue> GetValuesFromOldFishList(string locationName, string rawData)
		{
			HashSet<FishSpawnValue> hashSet = new HashSet<FishSpawnValue>();
			string[] array = rawData.Split(' ');
			for (int i = 0; i < array.Length - 1; i += 2)
			{
				string objectId = array[i];
				string fishZoneId = UpgradeFishAreaId(locationName, array[i + 1]);
				hashSet.Add(new FishSpawnValue(objectId, fishZoneId));
			}
			return hashSet;
		}

		private void GetForageChancesFromAsset(List<SpawnForageData> data, out Dictionary<string, double> springForage, out Dictionary<string, double> summerForage, out Dictionary<string, double> fallForage, out Dictionary<string, double> winterForage)
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Expected I4, but got Unknown
			springForage = new Dictionary<string, double>();
			summerForage = new Dictionary<string, double>();
			fallForage = new Dictionary<string, double>();
			winterForage = new Dictionary<string, double>();
			foreach (SpawnForageData datum in data)
			{
				string text = RuntimeMigrationHelper.ParseObjectId(((GenericSpawnItemData)datum).ItemId);
				IReadOnlySet<Season> readOnlySet = ParseEffectiveSeasons(datum.Season, ((GenericSpawnItemDataWithCondition)datum).Condition);
				if (text == null || readOnlySet == null)
				{
					continue;
				}
				Season[] validSeasons = ValidSeasons;
				foreach (Season val in validSeasons)
				{
					if (readOnlySet.Contains(val))
					{
						if (1 == 0)
						{
						}
						Dictionary<string, double> dictionary = (int)val switch
						{
							0 => springForage, 
							1 => summerForage, 
							2 => fallForage, 
							_ => winterForage, 
						};
						if (1 == 0)
						{
						}
						Dictionary<string, double> dictionary2 = dictionary;
						dictionary2[text] = datum.Chance;
					}
				}
			}
		}

		private void GetArtifactChancesFromAsset(List<ArtifactSpotDropData> data, out Dictionary<string, double> artifacts)
		{
			artifacts = new Dictionary<string, double>();
			foreach (ArtifactSpotDropData datum in data)
			{
				string text = RuntimeMigrationHelper.ParseObjectId(((GenericSpawnItemData)datum).ItemId);
				IReadOnlySet<Season> readOnlySet = ParseEffectiveSeasons(null, ((GenericSpawnItemDataWithCondition)datum).Condition);
				if (text != null && readOnlySet != null && readOnlySet.Count == 4)
				{
					artifacts[text] = datum.Chance;
				}
			}
		}

		private void GetFishFromNewList(List<SpawnFishData> data, out HashSet<FishSpawnValue> springFish, out HashSet<FishSpawnValue> summerFish, out HashSet<FishSpawnValue> fallFish, out HashSet<FishSpawnValue> winterFish)
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Expected I4, but got Unknown
			springFish = new HashSet<FishSpawnValue>();
			summerFish = new HashSet<FishSpawnValue>();
			fallFish = new HashSet<FishSpawnValue>();
			winterFish = new HashSet<FishSpawnValue>();
			foreach (SpawnFishData datum in data)
			{
				if (datum.Chance < 1f)
				{
					continue;
				}
				string text = RuntimeMigrationHelper.ParseObjectId(((GenericSpawnItemData)datum).ItemId);
				IReadOnlySet<Season> readOnlySet = ParseEffectiveSeasons(null, ((GenericSpawnItemDataWithCondition)datum).Condition);
				if (text == null || readOnlySet == null)
				{
					continue;
				}
				Season[] validSeasons = ValidSeasons;
				foreach (Season val in validSeasons)
				{
					if (readOnlySet.Contains(val))
					{
						if (1 == 0)
						{
						}
						HashSet<FishSpawnValue> hashSet = (int)val switch
						{
							0 => springFish, 
							1 => summerFish, 
							2 => fallFish, 
							_ => winterFish, 
						};
						if (1 == 0)
						{
						}
						HashSet<FishSpawnValue> hashSet2 = hashSet;
						hashSet2.Add(new FishSpawnValue(text, datum.FishAreaId));
					}
				}
			}
		}

		private string ConvertToOldArtifactOrForageString(Dictionary<string, double> forage)
		{
			if (forage.Count == 0)
			{
				return "-1";
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var (value, value2) in forage)
			{
				stringBuilder.Append(value).Append(' ').Append(value2)
					.Append(' ');
			}
			return stringBuilder.ToString(0, stringBuilder.Length - 1);
		}

		private string ConvertToOldFishString(string locationName, HashSet<FishSpawnValue> fish)
		{
			if (fish.Count == 0)
			{
				return "-1";
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (var (value, id) in fish)
			{
				stringBuilder.Append(value).Append(' ').Append(DowngradeFishAreaId(locationName, id))
					.Append(' ');
			}
			return stringBuilder.ToString(0, stringBuilder.Length - 1);
		}

		private string DowngradeFishAreaId(string locationName, string? id)
		{
			if (id == null)
			{
				return "-1";
			}
			string result;
			if (!(locationName == "Forest"))
			{
				if (locationName == "IslandWest")
				{
					if (1 == 0)
					{
					}
					if (id == "Freshwater")
					{
						result = "2";
					}
					else
					{
						result = ((!(id == "Ocean")) ? id : "1");
					}
					if (1 == 0)
					{
					}
					return result;
				}
				return id;
			}
			if (1 == 0)
			{
			}
			if (id == "Lake")
			{
				result = "1";
			}
			else
			{
				result = ((!(id == "River")) ? id : "0");
			}
			if (1 == 0)
			{
			}
			return result;
		}

		private string? UpgradeFishAreaId(string locationName, string id)
		{
			if (id == "-1")
			{
				return null;
			}
			string result;
			if (!(locationName == "Forest"))
			{
				if (locationName == "IslandWest")
				{
					if (1 == 0)
					{
					}
					if (id == "1")
					{
						result = "Ocean";
					}
					else
					{
						result = ((!(id == "2")) ? id : "Freshwater");
					}
					if (1 == 0)
					{
					}
					return result;
				}
				return id;
			}
			if (1 == 0)
			{
			}
			if (id == "0")
			{
				result = "River";
			}
			else
			{
				result = ((!(id == "1")) ? id : "Lake");
			}
			if (1 == 0)
			{
			}
			return result;
		}

		private IReadOnlySet<Season>? ParseEffectiveSeasons(Nullable season, string condition)
		{
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			string key = $"{season}|{condition}";
			if (ParseSeasonsCache.TryGetValue(key, out IReadOnlySet<Season> value))
			{
				return value;
			}
			if (!TryParseRawSeasonsFromNewData(season, condition, out HashSet<Season> parsedSeasons, out HashSet<Season> parsedNotSeasons))
			{
				ParseSeasonsCache[key] = null;
				return null;
			}
			if (parsedNotSeasons.Count > 0 && parsedNotSeasons.Count > 0)
			{
				Season[] validSeasons = ValidSeasons;
				foreach (Season item in validSeasons)
				{
					bool flag = parsedSeasons.Contains(item);
					bool flag2 = parsedNotSeasons.Contains(item);
					if (flag & flag2)
					{
						parsedSeasons.Remove(item);
						parsedNotSeasons.Remove(item);
					}
				}
				if (parsedSeasons.Count == 0 && parsedNotSeasons.Count == 0)
				{
					ParseSeasonsCache[key] = null;
					return null;
				}
			}
			if (parsedSeasons.Count > 0 && parsedNotSeasons.Count > 0)
			{
				parsedNotSeasons.Clear();
			}
			if (parsedNotSeasons.Count > 0)
			{
				Season[] validSeasons2 = ValidSeasons;
				foreach (Season item2 in validSeasons2)
				{
					if (!parsedNotSeasons.Contains(item2))
					{
						parsedSeasons.Add(item2);
					}
				}
			}
			if (parsedSeasons.Count > 0)
			{
				ParseSeasonsCache[key] = parsedSeasons;
				return parsedSeasons;
			}
			ParseSeasonsCache[key] = null;
			return null;
		}

		private bool TryParseRawSeasonsFromNewData(Nullable season, string condition, out HashSet<Season> parsedSeasons, out HashSet<Season> parsedNotSeasons)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			parsedSeasons = new HashSet<Season>();
			parsedNotSeasons = new HashSet<Season>();
			bool flag = false;
			if (season.HasValue)
			{
				parsedSeasons.Add(season.Value);
				flag = true;
			}
			if (GameStateQuery.IsImmutablyTrue(condition))
			{
				if (!flag)
				{
					parsedSeasons.Add((Season)0);
					parsedSeasons.Add((Season)1);
					parsedSeasons.Add((Season)2);
					parsedSeasons.Add((Season)3);
				}
				return parsedSeasons.Count > 0;
			}
			ParsedGameStateQuery[] array = GameStateQuery.Parse(condition);
			Season item = default;
			foreach (ParsedGameStateQuery val in array)
			{
				if (val.Error != null)
				{
					return false;
				}
				string[] query = val.Query;
				string a = ArgUtility.Get(query, 0, (string)null, true);
				int num;
				if (string.Equals(a, "LOCATION_SEASON"))
				{
					num = 2;
				}
				else
				{
					if (!string.Equals(a, "SEASON"))
					{
						return false;
					}
					num = 1;
				}
				bool negated = val.Negated;
				for (int j = num; j < query.Length; j++)
				{
					if (!Utility.TryParseEnum<Season>(query[j], ref item))
					{
						return false;
					}
					if (negated)
					{
						parsedNotSeasons.Add(item);
					}
					else
					{
						parsedSeasons.Add(item);
					}
				}
			}
			return parsedSeasons.Count > 0 || parsedNotSeasons.Count > 0;
		}

		public LocationsMigrator()
		{
			Season[] array = new Season[4];
			RuntimeHelpers.InitializeArray(array, __ldtoken(_003CPrivateImplementationDetails_003E.BAED642339816AFFB3FE8719792D0E4CE82F12DB72B7373D244EAA65445800FE));
			ValidSeasons = array;
			ParseSeasonsCache = new Dictionary<string, IReadOnlySet<Season>>();
			OriginalData = new VanillaAssetFactory<Dictionary<string, LocationData>>((Func<LocalizedContentManager, Dictionary<string, LocationData>>)DataLoader.Locations);
			base._002Ector();
		}
	}

	public record FishSpawnValue(string ObjectId, string? FishZoneId);

	private class NpcDispositionsMigrator : IEditAssetMigrator
	{
		private const string OldAssetName = "Data/NPCDispositions";

		private const string NewAssetName = "Data/Characters";

		private readonly HashSet<string> NpcNamesAddedIn16 = new HashSet<string>
		{
			"???", "Bear", "Birdie", "Bouncer", "Gil", "Governor", "Grandpa", "Gunther", "Henchman", "Mister Qi",
			"Morris", "Old Mariner", "Welwick"
		};

		private readonly VanillaAssetFactory<Dictionary<string, CharacterData>> OriginalData = new VanillaAssetFactory<Dictionary<string, CharacterData>>((Func<LocalizedContentManager, Dictionary<string, CharacterData>>)DataLoader.Characters);

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/NPCDispositions", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected Obj, but got Unknown
			return (IAssetName?)new AssetName("Data/Characters", (string)null, (Nullable)null);
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, CharacterData> freshCopy = OriginalData.GetFreshCopy();
			Dictionary<string, string> oldFormat = GetOldFormat(freshCopy);
			Dictionary<string, string> dictionary = patch.Load<Dictionary<string, string>>(GetOldAssetName(assetName));
			MergeIntoNewFormat(freshCopy, dictionary, oldFormat);
			asset = (T)(object)freshCopy;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, CharacterData> data = asset.GetData<Dictionary<string, CharacterData>>();
			Dictionary<string, string> oldFormat = GetOldFormat(data);
			Dictionary<string, string> fromBackup = new Dictionary<string, string>(oldFormat);
			patch.Edit<Dictionary<string, string>>((IAssetData)(object)new FakeAssetData(asset, GetOldAssetName(((IAssetInfo)asset).Name), oldFormat), onWarning);
			MergeIntoNewFormat(data, oldFormat, fromBackup);
			error = null;
			return true;
		}

		private IAssetName GetOldAssetName(IAssetName newName)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected Obj, but got Unknown
			return (IAssetName)new AssetName("Data/NPCDispositions", newName.LocaleCode, newName.LanguageCode);
		}

		private Dictionary<string, string> GetOldFormat(IDictionary<string, CharacterData> from)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = new string[12];
			foreach (var (text2, val2) in from)
			{
				array[0] = ((object)val2.Age/*cast due to constrained. prefix*/).ToString().ToLowerInvariant();
				array[1] = ((object)val2.Manner/*cast due to constrained. prefix*/).ToString().ToLowerInvariant();
				array[2] = ((object)val2.SocialAnxiety/*cast due to constrained. prefix*/).ToString().ToLowerInvariant();
				array[3] = ((object)val2.Optimism/*cast due to constrained. prefix*/).ToString().ToLowerInvariant();
				array[4] = ((object)val2.Gender/*cast due to constrained. prefix*/).ToString().ToLowerInvariant();
				array[5] = GetOldDateableField(text2, val2.CanBeRomanced);
				array[6] = val2.LoveInterest ?? "null";
				array[7] = val2.HomeRegion;
				array[8] = GetOldBirthdayField(val2.BirthSeason, val2.BirthDay);
				array[9] = GetOldFriendsAndFamilyField(val2.FriendsAndFamily);
				array[10] = "default_home_position 0 0";
				array[11] = TokenParser.ParseText(val2.DisplayName, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				dictionary[text2] = string.Join('/', array);
			}
			return dictionary;
		}

		private void MergeIntoNewFormat(IDictionary<string, CharacterData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected Obj, but got Unknown
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			foreach (string key2 in asset.Keys)
			{
				if (!from.ContainsKey(key2) && !NpcNamesAddedIn16.Contains(key2))
				{
					asset.Remove(key2);
				}
			}
			foreach (var (key, text3) in from)
			{
				bool flag = false;
				if (!asset.TryGetValue(key, out CharacterData value))
				{
					flag = true;
					value = new CharacterData();
				}
				string[] array = null;
				if (fromBackup != null)
				{
					if (fromBackup.TryGetValue(key, out string value2) && value2 == text3)
					{
						continue;
					}
					array = value2?.Split('/');
				}
				string[] array2 = text3.Split('/');
				value.Age = ArgUtility.GetEnum<NpcAge>(array2, 0, value.Age);
				value.Manner = ArgUtility.GetEnum<NpcManner>(array2, 1, value.Manner);
				value.SocialAnxiety = ArgUtility.GetEnum<NpcSocialAnxiety>(array2, 2, value.SocialAnxiety);
				value.Optimism = ArgUtility.GetEnum<NpcOptimism>(array2, 3, value.Optimism);
				value.Gender = ArgUtility.GetEnum<Gender>(array2, 4, value.Gender);
				MergeDateableFieldIntoNewFormat(value, ArgUtility.Get(array2, 5, (string)null, true));
				value.LoveInterest = ArgUtility.Get(array2, 6, value.LoveInterest, true);
				if (value.LoveInterest == "null")
				{
					value.LoveInterest = null;
				}
				value.HomeRegion = ArgUtility.Get(array2, 7, value.HomeRegion, true);
				string text4 = ArgUtility.Get(array2, 8, (string)null, true);
				if (text4 != ArgUtility.Get(array, 8, (string)null, true))
				{
					MergeBirthdayIntoNewFormat(value, text4);
				}
				string text5 = ArgUtility.Get(array2, 9, (string)null, true);
				if (text5 != ArgUtility.Get(array, 9, (string)null, true))
				{
					MergeFriendsAndFamilyIntoNewFormat(value, text5);
				}
				string text6 = ArgUtility.Get(array2, 10, (string)null, true);
				if (text6 != ArgUtility.Get(array, 10, (string)null, true))
				{
					MergeHomeIntoNewFormat(value, text6);
				}
				value.DisplayName = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 11, (string)null, true), ArgUtility.Get(array, 11, (string)null, true), value.DisplayName);
				if (flag)
				{
					asset[key] = value;
				}
			}
		}

		private string GetOldDateableField(string npcName, bool canRomance)
		{
			if (canRomance)
			{
				return "datable";
			}
			return (npcName == "Krobus") ? "secret" : "not-datable";
		}

		private void MergeDateableFieldIntoNewFormat(CharacterData entry, string dateable)
		{
			if (dateable == "datable")
			{
				entry.CanBeRomanced = true;
			}
			else
			{
				entry.CanBeRomanced = false;
			}
		}

		private string GetOldBirthdayField(Nullable season, int day)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			return season.HasValue ? $"{((object)season.Value/*cast due to constrained. prefix*/).ToString().ToLowerInvariant()} {day}" : "null";
		}

		private void MergeBirthdayIntoNewFormat(CharacterData entry, string birthday)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			string[] array = ArgUtility.SplitBySpace(birthday, 2);
			string text = ArgUtility.Get(array, 0, (string)null, true);
			int birthDay = ArgUtility.GetInt(array, 1, 0);
			Season value = default;
			if (Utility.TryParseEnum<Season>(text, ref value))
			{
				entry.BirthSeason = value;
				entry.BirthDay = birthDay;
			}
			else
			{
				entry.BirthSeason = null;
				entry.BirthDay = 0;
			}
		}

		private string GetOldFriendsAndFamilyField(Dictionary<string, string> data)
		{
			int? num = data?.Count;
			if (!num.HasValue || num.GetValueOrDefault() <= 0)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, string> datum in data)
			{
				string key = datum.Key;
				string value = TokenParser.ParseText(datum.Value, (Random)null, (TokenParserDelegate)null, (Farmer)null)?.Replace("'", "``");
				stringBuilder.Append(key).Append(" '").Append(value)
					.Append("' ");
			}
			return stringBuilder.ToString(0, stringBuilder.Length - 1);
		}

		private void MergeFriendsAndFamilyIntoNewFormat(CharacterData entry, string rawFriendsAndFamily)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = 0;
			while (num < rawFriendsAndFamily.Length - 1)
			{
				int num2 = rawFriendsAndFamily.IndexOf('\'', num);
				if (num2 == -1)
				{
					break;
				}
				int num3 = rawFriendsAndFamily.IndexOf('\'', num2 + 1);
				if (num3 == -1)
				{
					num3 = rawFriendsAndFamily.Length;
				}
				string key = rawFriendsAndFamily.Substring(num, num2 - num);
				string value = rawFriendsAndFamily.Substring(num2 + 1, num3 - 1 - num2).Replace("``", "'").Replace('_', ' ');
				dictionary[key] = value;
				num = num3 + 1;
			}
			Dictionary<string, string> friendsAndFamily = entry.FriendsAndFamily;
			string value2;
			string key2;
			if (friendsAndFamily != null && friendsAndFamily.Count > 0)
			{
				foreach (KeyValuePair<string, string> item in entry.FriendsAndFamily)
				{
					item.Deconstruct(out value2, out key2);
					string key3 = value2;
					string text = key2;
					if (!dictionary.TryGetValue(key3, out var value3))
					{
						entry.FriendsAndFamily.Remove(key3);
						continue;
					}
					if (value3 != text && value3 != TokenParser.ParseText(text, (Random)null, (TokenParserDelegate)null, (Farmer)null))
					{
						entry.FriendsAndFamily[key3] = value3;
					}
					dictionary.Remove(key3);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			if (entry.FriendsAndFamily == null)
			{
				entry.FriendsAndFamily = new Dictionary<string, string>();
			}
			foreach (KeyValuePair<string, string> item2 in dictionary)
			{
				item2.Deconstruct(out key2, out value2);
				string key4 = key2;
				string value4 = value2;
				entry.FriendsAndFamily[key4] = value4;
			}
		}

		private void MergeHomeIntoNewFormat(CharacterData entry, string rawHome)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Expected Obj, but got Unknown
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			string[] array = ArgUtility.SplitBySpace(rawHome);
			string text = array[0];
			Point zero = default;
			string text2 = default;
			if (!ArgUtility.TryGetPoint(array, 1, ref zero, ref text2, "Point tile"))
			{
				zero = Point.Zero;
			}
			int num = default;
			if (!ArgUtility.TryGetInt(array, 3, ref num, ref text2, "int direction"))
			{
				num = 0;
			}
			List<CharacterHomeData> home = entry.Home;
			if (home != null && home.Count > 0)
			{
				foreach (CharacterHomeData item in entry.Home)
				{
					if (item.Location == text && GameStateQuery.IsImmutablyTrue(item.Condition))
					{
						item.Tile = zero;
						return;
					}
				}
			}
			if (entry.Home == null)
			{
				entry.Home = new List<CharacterHomeData>();
			}
			entry.Home.Clear();
			List<CharacterHomeData> home2 = entry.Home;
			CharacterHomeData val = new CharacterHomeData();
			val.Id = "Default";
			val.Location = text;
			val.Tile = zero;
			CharacterHomeData val2 = val;
			if (1 == 0)
			{
			}
			text2 = num switch
			{
				2 => "down", 
				3 => "left", 
				1 => "right", 
				_ => "up", 
			};
			if (1 == 0)
			{
			}
			val2.Direction = text2;
			home2.Add(val);
		}
	}

	private class ObjectContextTagsMigrator : IEditAssetMigrator
	{
		private const string OldAssetName = "Data/ObjectContextTags";

		private const string NewAssetName = "Data/Objects";

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/ObjectContextTags", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected Obj, but got Unknown
			return (IAssetName?)new AssetName("Data/Objects", (string)null, (Nullable)null);
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			error = "can't migrate load patches for 'Data/ObjectContextTags' to Stardew Valley 1.6";
			return false;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, ObjectData> data = asset.GetData<Dictionary<string, ObjectData>>();
			Dictionary<string, string> oldFormat = GetOldFormat(data);
			Dictionary<string, string> contextTagsBackup = new Dictionary<string, string>(oldFormat);
			patch.Edit<Dictionary<string, string>>((IAssetData)(object)new FakeAssetData(asset, GetOldAssetName(((IAssetInfo)asset).Name), oldFormat), onWarning);
			MergeIntoNewFormat(data, oldFormat, contextTagsBackup, patch.ContentPack.Manifest.UniqueID);
			error = null;
			return true;
		}

		private IAssetName GetOldAssetName(IAssetName newName)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected Obj, but got Unknown
			return (IAssetName)new AssetName("Data/ObjectContextTags", newName.LocaleCode, newName.LanguageCode);
		}

		private Dictionary<string, string> GetOldFormat(IDictionary<string, ObjectData> asset)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (var (objectId, val2) in asset)
			{
				if (val2.Name != null)
				{
					string oldEntryKey = GetOldEntryKey(objectId, val2);
					List<string> contextTags = val2.ContextTags;
					dictionary[oldEntryKey] = ((contextTags != null && contextTags.Count > 0) ? string.Join(", ", val2.ContextTags) : string.Empty);
				}
			}
			return dictionary;
		}

		private void MergeIntoNewFormat(IDictionary<string, ObjectData> asset, IDictionary<string, string> contextTags, IDictionary<string, string>? contextTagsBackup, string modId)
		{
			string value;
			string key;
			if (contextTagsBackup != null)
			{
				bool flag = false;
				foreach (KeyValuePair<string, string> contextTag in contextTags)
				{
					contextTag.Deconstruct(out value, out key);
					string key2 = value;
					string text = key;
					if (!contextTagsBackup.TryGetValue(key2, out string value2) || value2 != text)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
			}
			Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
			ILookup<string, string> lookup = asset.ToLookup((KeyValuePair<string, ObjectData> p) => p.Value.Name, (KeyValuePair<string, ObjectData> p) => p.Key);
			foreach (KeyValuePair<string, string> contextTag2 in contextTags)
			{
				contextTag2.Deconstruct(out key, out value);
				string text2 = key;
				string text3 = value;
				string[] tags = text3.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				if (text2.StartsWith("id_"))
				{
					if (text2.StartsWith("id_o_"))
					{
						string objectId = text2.Substring("id_o_".Length);
						TrackRawContextTagsById(dictionary, objectId, tags);
					}
					continue;
				}
				foreach (string item in lookup[text2])
				{
					TrackRawContextTagsById(dictionary, item, tags);
				}
			}
			foreach (KeyValuePair<string, HashSet<string>> item2 in dictionary)
			{
				item2.Deconstruct(out value, out var value3);
				string key3 = value;
				HashSet<string> hashSet = value3;
				if (!asset.TryGetValue(key3, out ObjectData value4))
				{
					continue;
				}
				if (hashSet.Count == 0)
				{
					value4.ContextTags?.Clear();
					continue;
				}
				ObjectData val = value4;
				if (val.ContextTags == null)
				{
					val.ContextTags = new List<string>();
				}
				value4.ContextTags.Clear();
				value4.ContextTags.AddRange(hashSet);
			}
		}

		private void TrackRawContextTagsById(Dictionary<string, HashSet<string>> contextTagsById, string objectId, string[] tags)
		{
			if (contextTagsById.TryGetValue(objectId, out HashSet<string> value))
			{
				CollectionExtensions.AddRange<string>((ISet<string>)value, (IEnumerable<string>)tags);
			}
			else
			{
				contextTagsById[objectId] = new HashSet<string>(tags);
			}
		}

		private string GetOldEntryKey(string objectId, ObjectData entry)
		{
			switch (objectId)
			{
			case "113":
			case "126":
			case "127":
			case "340":
			case "342":
			case "344":
			case "348":
			case "350":
			case "447":
			case "812":
				return "id_0_" + objectId;
			default:
				return entry.Name ?? ("id_0_" + objectId);
			}
		}
	}

	private class ObjectInformationMigrator : IEditAssetMigrator
	{
		private const string OldAssetName = "Data/ObjectInformation";

		private const string NewAssetName = "Data/Objects";

		private readonly HashSet<string> NumericIdsAddedIn16 = new HashSet<string>
		{
			"6", "8", "10", "12", "14", "32", "34", "36", "38", "40",
			"42", "44", "46", "48", "50", "52", "54", "56", "58", "742"
		};

		private readonly VanillaAssetFactory<Dictionary<string, ObjectData>> OriginalData = new VanillaAssetFactory<Dictionary<string, ObjectData>>((Func<LocalizedContentManager, Dictionary<string, ObjectData>>)DataLoader.Objects);

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/ObjectInformation", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected Obj, but got Unknown
			return (IAssetName?)new AssetName("Data/Objects", (string)null, (Nullable)null);
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, ObjectData> freshCopy = OriginalData.GetFreshCopy();
			Dictionary<string, string> oldFormat = GetOldFormat(freshCopy);
			Dictionary<string, string> dictionary = patch.Load<Dictionary<string, string>>(GetOldAssetName(assetName));
			MergeIntoNewFormat(freshCopy, dictionary, oldFormat, patch.ContentPack.Manifest.UniqueID);
			asset = (T)(object)freshCopy;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, ObjectData> data = asset.GetData<Dictionary<string, ObjectData>>();
			Dictionary<string, string> oldFormat = GetOldFormat(data);
			Dictionary<string, string> fromBackup = new Dictionary<string, string>(oldFormat);
			patch.Edit<Dictionary<string, string>>((IAssetData)(object)new FakeAssetData(asset, GetOldAssetName(((IAssetInfo)asset).Name), oldFormat), onWarning);
			MergeIntoNewFormat(data, oldFormat, fromBackup, patch.ContentPack.Manifest.UniqueID);
			error = null;
			return true;
		}

		private IAssetName GetOldAssetName(IAssetName newName)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected Obj, but got Unknown
			return (IAssetName)new AssetName("Data/ObjectInformation", newName.LocaleCode, newName.LanguageCode);
		}

		private Dictionary<string, string> GetOldFormat(IDictionary<string, ObjectData> from)
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = new string[9];
			foreach (var (text2, val2) in from)
			{
				ObjectBuffData val3 = val2.Buffs?.FirstOrDefault();
				BuffEffects val4 = ((((val3 != null) ? val3.CustomAttributes : null) != null) ? new BuffEffects(val3.CustomAttributes) : ((BuffEffects)null));
				array[0] = val2.Name;
				array[1] = val2.Price.ToString();
				array[2] = val2.Edibility.ToString();
				array[3] = $"{val2.Type} {val2.Category}";
				array[4] = TokenParser.ParseText(val2.DisplayName, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				array[5] = TokenParser.ParseText(val2.Description, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				array[6] = GetOldMiscellaneousField(text2, val2);
				array[7] = ((((val4 != null) ? new bool?(val4.HasAnyValue()) : ((bool?)null)) ?? false) ? string.Join(" ", val4.ToLegacyAttributeFormat()) : string.Empty);
				array[8] = ((val3 != null) ? val3.Duration.ToString() : null) ?? string.Empty;
				dictionary[text2] = string.Join('/', array);
			}
			return dictionary;
		}

		private void MergeIntoNewFormat(IDictionary<string, ObjectData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup, string modId)
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected Obj, but got Unknown
			foreach (string key in asset.Keys)
			{
				if (!from.ContainsKey(key) && int.TryParse(key, out var _) && !NumericIdsAddedIn16.Contains(key))
				{
					asset.Remove(key);
				}
			}
			foreach (var (text3, text4) in from)
			{
				bool flag = false;
				if (!asset.TryGetValue(text3, out ObjectData value))
				{
					flag = true;
					value = new ObjectData();
				}
				string[] array = null;
				if (fromBackup != null)
				{
					if (fromBackup.TryGetValue(text3, out string value2) && value2 == text4)
					{
						continue;
					}
					array = value2?.Split('/');
				}
				string[] array2 = text4.Split('/');
				value.Name = ArgUtility.Get(array2, 0, value.Name, false);
				value.Price = ArgUtility.GetInt(array2, 1, value.Price);
				value.Edibility = ArgUtility.GetInt(array2, 2, value.Edibility);
				string text5 = ArgUtility.Get(array2, 3, (string)null, true);
				if (!string.IsNullOrWhiteSpace(text5) && text5 != ArgUtility.Get(array, 3, (string)null, true))
				{
					string[] array3 = text5.Split(' ', 2);
					value.Type = array3[0];
					value.Category = ArgUtility.GetInt(array3, 1, value.Category);
				}
				value.DisplayName = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 4, (string)null, true), ArgUtility.Get(array, 4, (string)null, true), value.DisplayName);
				value.Description = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 5, (string)null, true), ArgUtility.Get(array, 5, (string)null, true), TokenParser.ParseText(value.Description, (Random)null, (TokenParserDelegate)null, (Farmer)null));
				string text6 = ArgUtility.Get(array2, 6, (string)null, true);
				if (text6 != ArgUtility.Get(array, 6, (string)null, true))
				{
					MergeMiscellaneousFieldIntoNewFormat(text3, value, text6, modId);
				}
				string text7 = ArgUtility.Get(array2, 7, (string)null, true);
				if (text7 != null && text7 != ArgUtility.Get(array, 7, (string)null, true))
				{
					MergeBuffFieldIntoNewFormat(value, text7);
				}
				List<ObjectBuffData> buffs = value.Buffs;
				if (buffs != null && buffs.Count > 0)
				{
					value.Buffs[0].Duration = ArgUtility.GetInt(array2, 8, value.Buffs[0].Duration);
				}
				if (flag)
				{
					asset[text3] = value;
				}
			}
		}

		private string GetOldMiscellaneousField(string objectId, ObjectData data)
		{
			bool flag;
			switch (objectId)
			{
			case "275":
			case "535":
			case "536":
			case "537":
			case "749":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				if (data.GeodeDrops == null)
				{
					return string.Empty;
				}
				return string.Join(' ', from drop in data.GeodeDrops
					where GameStateQuery.IsImmutablyTrue(((GenericSpawnItemDataWithCondition)drop).Condition)
					let itemId = RuntimeMigrationHelper.ParseObjectId(((GenericSpawnItemData)drop).ItemId)
					where itemId != null
					select itemId);
			}
			if (data.Type == "Arch")
			{
				if (data.ArtifactSpotChances == null)
				{
					return string.Empty;
				}
				return string.Join(' ', data.ArtifactSpotChances.Select((KeyValuePair<string, float> p) => $"{p.Key} {p.Value}"));
			}
			List<ObjectBuffData> buffs = data.Buffs;
			if (buffs != null && buffs.Count > 0)
			{
				return data.IsDrink ? "drink" : "food";
			}
			return string.Empty;
		}

		private void MergeBuffFieldIntoNewFormat(ObjectData entry, string field)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected Obj, but got Unknown
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected Obj, but got Unknown
			//IL_0044: Expected Obj, but got Unknown
			string[] array = field.Split(' ');
			ObjectBuffData val = new ObjectBuffData
			{
				BuffId = (entry.IsDrink ? "drink" : "food")
			};
			ObjectBuffData val2 = val;
			if (val2.CustomAttributes == null)
			{
				BuffAttributesData val3 = new BuffAttributesData();
				BuffAttributesData val4 = val3;
				val2.CustomAttributes = val3;
			}
			BuffAttributesData customAttributes = val.CustomAttributes;
			customAttributes.FarmingLevel = ArgUtility.GetFloat(array, 0, customAttributes.FarmingLevel);
			customAttributes.FishingLevel = ArgUtility.GetFloat(array, 1, customAttributes.FishingLevel);
			customAttributes.MiningLevel = ArgUtility.GetFloat(array, 2, customAttributes.MiningLevel);
			customAttributes.LuckLevel = ArgUtility.GetFloat(array, 4, customAttributes.LuckLevel);
			customAttributes.ForagingLevel = ArgUtility.GetFloat(array, 5, customAttributes.ForagingLevel);
			customAttributes.MaxStamina = ArgUtility.GetFloat(array, 7, customAttributes.MaxStamina);
			customAttributes.MagneticRadius = ArgUtility.GetFloat(array, 8, customAttributes.MagneticRadius);
			customAttributes.Speed = ArgUtility.GetFloat(array, 9, customAttributes.Speed);
			customAttributes.Defense = ArgUtility.GetFloat(array, 10, customAttributes.Defense);
			customAttributes.Attack = ArgUtility.GetFloat(array, 11, customAttributes.Attack);
			if (new BuffEffects(val.CustomAttributes).HasAnyValue())
			{
				if (entry.Buffs == null)
				{
					entry.Buffs = new List<ObjectBuffData>();
				}
				entry.Buffs.Add(val);
			}
			else
			{
				entry.Buffs = null;
			}
		}

		private void MergeMiscellaneousFieldIntoNewFormat(string objectId, ObjectData data, string miscellaneous, string modId)
		{
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Expected Obj, but got Unknown
			bool flag;
			switch (objectId)
			{
			case "275":
			case "535":
			case "536":
			case "537":
			case "749":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			ObjectData val2;
			if (flag)
			{
				HashSet<string> hashSet = new HashSet<string>(miscellaneous.Split(' '));
				if (data.GeodeDrops != null)
				{
					for (int i = 0; i < data.GeodeDrops.Count; i++)
					{
						ObjectGeodeDropData val = data.GeodeDrops[i];
						string text = RuntimeMigrationHelper.ParseObjectId(((GenericSpawnItemData)val).ItemId);
						if (text != null && GameStateQuery.IsImmutablyTrue(((GenericSpawnItemDataWithCondition)val).Condition))
						{
							if (!hashSet.Contains(text))
							{
								data.GeodeDrops.RemoveAt(i);
								i--;
							}
							else
							{
								hashSet.Remove(text);
							}
						}
					}
				}
				if (hashSet.Count > 0)
				{
					val2 = data;
					if (val2.GeodeDrops == null)
					{
						val2.GeodeDrops = new List<ObjectGeodeDropData>();
					}
					foreach (string item in hashSet)
					{
						string text2 = "(O)" + item;
						data.GeodeDrops.Add(new ObjectGeodeDropData
						{
							Id = modId + "_" + text2,
							ItemId = text2
						});
					}
				}
			}
			if (!(data.Type == "Arch"))
			{
				return;
			}
			Dictionary<string, double> dictionary = new Dictionary<string, double>();
			string[] array = miscellaneous.Split(' ');
			for (int j = 0; j < array.Length - 1; j += 2)
			{
				string key = array[j];
				if (double.TryParse(array[j + 1], out var result))
				{
					dictionary[key] = result;
				}
			}
			Dictionary<string, float> artifactSpotChances = data.ArtifactSpotChances;
			if (artifactSpotChances != null && artifactSpotChances.Count > 0)
			{
				foreach (string key2 in data.ArtifactSpotChances.Keys)
				{
					if (!dictionary.TryGetValue(key2, out var value))
					{
						data.ArtifactSpotChances.Remove(key2);
						continue;
					}
					data.ArtifactSpotChances[key2] = (float)value;
					dictionary.Remove(key2);
				}
			}
			if (dictionary.Count <= 0)
			{
				return;
			}
			val2 = data;
			if (val2.ArtifactSpotChances == null)
			{
				val2.ArtifactSpotChances = new Dictionary<string, float>();
			}
			foreach (var (text4, num2) in dictionary)
			{
				string text5 = "(O)" + text4;
				data.ArtifactSpotChances[text4] = (float)num2;
			}
		}
	}

	private class WeaponsMigrator : IEditAssetMigrator
	{
		private const string AssetName = "Data/Weapons";

		private readonly HashSet<string> NumericIdsAddedIn16 = new HashSet<string> { "65", "66" };

		private readonly VanillaAssetFactory<Dictionary<string, WeaponData>> OriginalData = new VanillaAssetFactory<Dictionary<string, WeaponData>>((Func<LocalizedContentManager, Dictionary<string, WeaponData>>)DataLoader.Weapons);

		public bool AppliesTo(IAssetName assetName)
		{
			return ((assetName != null) ? new bool?(assetName.IsEquivalentTo("Data/Weapons", true)) : ((bool?)null)) ?? false;
		}

		public IAssetName? RedirectTarget(IAssetName assetName, IPatch patch)
		{
			return null;
		}

		public bool TryApplyLoadPatch<T>(LoadPatch patch, IAssetName assetName, [NotNullWhen(true)] ref T? asset, out string? error)
		{
			Dictionary<string, WeaponData> freshCopy = OriginalData.GetFreshCopy();
			Dictionary<string, string> oldFormat = GetOldFormat(freshCopy);
			Dictionary<string, string> dictionary = patch.Load<Dictionary<string, string>>(assetName);
			MergeIntoNewFormat(freshCopy, dictionary, oldFormat);
			asset = (T)(object)freshCopy;
			error = null;
			return true;
		}

		public bool TryApplyEditPatch<T>(EditDataPatch patch, IAssetData asset, Action<string, IMonitor> onWarning, out string? error)
		{
			Dictionary<string, WeaponData> data = asset.GetData<Dictionary<string, WeaponData>>();
			Dictionary<string, string> oldFormat = GetOldFormat(data);
			Dictionary<string, string> fromBackup = new Dictionary<string, string>(oldFormat);
			patch.Edit<Dictionary<string, string>>((IAssetData)(object)new FakeAssetData(asset, ((IAssetInfo)asset).Name, oldFormat), onWarning);
			MergeIntoNewFormat(data, oldFormat, fromBackup);
			error = null;
			return true;
		}

		private Dictionary<string, string> GetOldFormat(IDictionary<string, WeaponData> from)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = new string[15];
			foreach (var (key, val2) in from)
			{
				array[0] = val2.Name;
				array[1] = TokenParser.ParseText(val2.Description, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				array[2] = val2.MinDamage.ToString();
				array[3] = val2.MaxDamage.ToString();
				array[4] = val2.Knockback.ToString();
				array[5] = val2.Speed.ToString();
				array[6] = val2.Precision.ToString();
				array[7] = val2.Defense.ToString();
				array[8] = val2.Type.ToString();
				array[9] = val2.MineBaseLevel.ToString();
				array[10] = val2.MineMinLevel.ToString();
				array[11] = val2.AreaOfEffect.ToString();
				array[12] = val2.CritChance.ToString();
				array[13] = val2.CritMultiplier.ToString();
				array[14] = TokenParser.ParseText(val2.DisplayName, (Random)null, (TokenParserDelegate)null, (Farmer)null);
				dictionary[key] = string.Join('/', array);
			}
			return dictionary;
		}

		private void MergeIntoNewFormat(IDictionary<string, WeaponData> asset, IDictionary<string, string> from, IDictionary<string, string>? fromBackup)
		{
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Expected Obj, but got Unknown
			foreach (string key2 in asset.Keys)
			{
				if (!from.ContainsKey(key2) && !NumericIdsAddedIn16.Contains(key2))
				{
					asset.Remove(key2);
				}
			}
			foreach (var (key, text3) in from)
			{
				string[] array = null;
				if (fromBackup != null)
				{
					if (fromBackup.TryGetValue(key, out string value) && value == text3)
					{
						continue;
					}
					array = value?.Split('/');
				}
				bool flag = false;
				if (!asset.TryGetValue(key, out WeaponData value2))
				{
					flag = true;
					value2 = new WeaponData();
				}
				string[] array2 = text3.Split('/');
				value2.Name = ArgUtility.Get(array2, 0, value2.Name, false);
				value2.Description = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 1, (string)null, true), ArgUtility.Get(array, 1, (string)null, true), value2.Description);
				value2.MinDamage = ArgUtility.GetInt(array2, 2, value2.MinDamage);
				value2.MaxDamage = ArgUtility.GetInt(array2, 3, value2.MaxDamage);
				value2.Knockback = ArgUtility.GetFloat(array2, 4, value2.Knockback);
				value2.Speed = ArgUtility.GetInt(array2, 5, value2.Speed);
				value2.Precision = ArgUtility.GetInt(array2, 6, value2.Precision);
				value2.Defense = ArgUtility.GetInt(array2, 7, value2.Defense);
				value2.Type = ArgUtility.GetInt(array2, 8, value2.Type);
				value2.MineBaseLevel = ArgUtility.GetInt(array2, 9, value2.MineBaseLevel);
				value2.MineMinLevel = ArgUtility.GetInt(array2, 10, value2.MineMinLevel);
				value2.AreaOfEffect = ArgUtility.GetInt(array2, 11, value2.AreaOfEffect);
				value2.CritChance = ArgUtility.GetFloat(array2, 12, value2.CritChance);
				value2.CritMultiplier = ArgUtility.GetFloat(array2, 13, value2.CritMultiplier);
				value2.DisplayName = RuntimeMigrationHelper.MigrateLiteralTextToTokenizableField(ArgUtility.Get(array2, 14, (string)null, true), ArgUtility.Get(array, 14, (string)null, true), value2.DisplayName);
				if (flag)
				{
					asset[key] = value2;
				}
			}
		}
	}

	public Migration_2_0()
		: base((ISemanticVersion)new SemanticVersion(2, 0, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("ModId");
		MigrationWarnings = new string[1] { "Some content packs haven't been updated for Stardew Valley 1.6.0. Content Patcher will try to auto-migrate them, but compatibility isn't guaranteed." };
		RuntimeEditDataMigrators = new IEditAssetMigrator[10]
		{
			new BigCraftableInformationMigrator(),
			new BlueprintsMigrator(),
			new BootsMigrator(),
			new CropsMigrator(),
			new FurnitureMigrator(),
			new LocationsMigrator(),
			new NpcDispositionsMigrator(),
			new ObjectContextTagsMigrator(),
			new ObjectInformationMigrator(),
			new WeaponsMigrator()
		};
	}

	public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref patches, out error))
		{
			return false;
		}
		PatchConfig[] array = patches;
		foreach (PatchConfig patchConfig in array)
		{
			if (patchConfig.Priority != null)
			{
				error = GetNounPhraseError("using Priority");
				return false;
			}
		}
		return true;
	}
}
