using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Common;

public static class MachineDataHelper
{
	public static void GetBuildingChestNames(BuildingData? data, ISet<string> inputChests, ISet<string> outputChests)
	{
		int? num = data?.ItemConversions?.Count;
		if (!num.HasValue || num.GetValueOrDefault() <= 0)
		{
			return;
		}
		foreach (BuildingItemConversion itemConversion in data.ItemConversions)
		{
			if (itemConversion?.SourceChest != null)
			{
				inputChests.Add(itemConversion.SourceChest);
			}
			if (itemConversion?.DestinationChest != null)
			{
				outputChests.Add(itemConversion.DestinationChest);
			}
		}
	}

	public static bool TryGetBuildingChestNames(BuildingData? data, out ISet<string> inputChests, out ISet<string> outputChests)
	{
		inputChests = new HashSet<string>();
		outputChests = new HashSet<string>();
		GetBuildingChestNames(data, inputChests, outputChests);
		return inputChests.Count > 0 || outputChests.Count > 0;
	}

	public static IEnumerable<Chest> GetBuildingChests(Building building, ISet<string> chestNames)
	{
		Enumerator<Chest, NetRef<Chest>> enumerator = building.buildingChests.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Chest chest = enumerator.Current;
				if (chestNames.Contains(((Item)chest).Name))
				{
					yield return chest;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static bool TryGetUniqueItemFromContextTag(string contextTag, [NotNullWhen(true)] out ParsedItemData? data)
	{
		if (contextTag.StartsWith("id_"))
		{
			string text = contextTag.Substring(3, contextTag.Length - 3);
			string text2 = null;
			if (text.StartsWith('('))
			{
				text2 = text;
			}
			else
			{
				string[] array = text.Split('_', 2);
				foreach (IItemDataDefinition itemType in ItemRegistry.ItemTypes)
				{
					if (string.Equals(array[0], itemType.StandardDescriptor, StringComparison.InvariantCultureIgnoreCase))
					{
						text2 = itemType.Identifier + array[1];
						break;
					}
				}
			}
			data = ItemRegistry.GetData(text2);
			return data != null;
		}
		data = null;
		return false;
	}

	public static bool TryGetUniqueItemFromGameStateQuery(string query, [NotNullWhen(true)] out ParsedItemData? data)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		data = null;
		ParsedGameStateQuery[] array = GameStateQuery.Parse(query);
		foreach (ParsedGameStateQuery val in array)
		{
			if (val.Error != null)
			{
				continue;
			}
			string text = ArgUtility.Get(val.Query, 0, (string)null, true);
			ParsedItemData data2;
			if (StringExtensions.EqualsIgnoreCase(text, "ITEM_ID"))
			{
				if (val.Query.Length == 3 && StringExtensions.EqualsIgnoreCase(val.Query[1], "Input"))
				{
					string text2 = ItemRegistry.QualifyItemId(val.Query[2]);
					if (data == null)
					{
						data = ItemRegistry.GetData(text2);
					}
					else if (data.QualifiedItemId != text2)
					{
						data = null;
						return false;
					}
				}
			}
			else if (StringExtensions.EqualsIgnoreCase(text, "ITEM_CONTEXT_TAG") && val.Query.Length == 3 && StringExtensions.EqualsIgnoreCase(val.Query[1], "Input") && TryGetUniqueItemFromContextTag(val.Query[2], out data2))
			{
				if (data == null)
				{
					data = data2;
				}
				else if (data.QualifiedItemId != data2.QualifiedItemId)
				{
					data = null;
					return false;
				}
			}
		}
		return data != null;
	}
}
