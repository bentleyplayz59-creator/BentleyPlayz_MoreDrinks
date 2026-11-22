using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Crops;

namespace Pathoschild.Stardew.Common.DataParsers;

internal class CropDataParser
{
	public Crop? Crop { get; }

	public CropData? CropData { get; }

	public Season[] Seasons { get; }

	public int HarvestablePhase { get; }

	public int DaysToFirstHarvest { get; }

	public int DaysToSubsequentHarvest { get; }

	public bool HasMultipleHarvests { get; }

	public bool CanHarvestNow { get; }

	public CropDataParser(Crop? crop, bool isPlanted)
	{
		Crop = crop;
		CropData = ((crop != null) ? crop.GetData() : null);
		CropData cropData = CropData;
		if (cropData != null)
		{
			Seasons = cropData.Seasons.ToArray();
			HasMultipleHarvests = crop.RegrowsAfterHarvest();
			HarvestablePhase = ((NetList<int, NetInt>)(object)crop.phaseDays).Count - 1;
			CanHarvestNow = ((NetFieldBase<int, NetInt>)(object)crop.currentPhase).Value >= HarvestablePhase && (!((NetFieldBase<bool, NetBool>)(object)crop.fullyGrown).Value || ((NetFieldBase<int, NetInt>)(object)crop.dayOfCurrentPhase).Value <= 0);
			DaysToFirstHarvest = ((IEnumerable<int>)crop.phaseDays).Take(((NetList<int, NetInt>)(object)crop.phaseDays).Count - 1).Sum();
			DaysToSubsequentHarvest = cropData.RegrowDays;
			if (!isPlanted && ((NetHashSet<int>)(object)Game1.player.professions).Contains(5))
			{
				DaysToFirstHarvest = (int)((double)DaysToFirstHarvest * 0.9);
			}
		}
		else
		{
			Seasons = Array.Empty<Season>();
		}
	}

	public SDate GetNextHarvest()
	{
		Crop crop = Crop;
		if (crop == null)
		{
			throw new InvalidOperationException("Can't get the harvest date because there's no crop.");
		}
		CropData cropData = CropData;
		if (cropData == null)
		{
			throw new InvalidOperationException("Can't get the harvest date because the crop has no data.");
		}
		if (CanHarvestNow)
		{
			return SDate.Now();
		}
		if (!((NetFieldBase<bool, NetBool>)(object)crop.fullyGrown).Value)
		{
			int num = DaysToFirstHarvest - ((NetFieldBase<int, NetInt>)(object)crop.dayOfCurrentPhase).Value - ((IEnumerable<int>)crop.phaseDays).Take(((NetFieldBase<int, NetInt>)(object)crop.currentPhase).Value).Sum();
			return SDate.Now().AddDays(num);
		}
		if (((NetFieldBase<int, NetInt>)(object)crop.dayOfCurrentPhase).Value >= cropData.RegrowDays)
		{
			return SDate.Now().AddDays(cropData.RegrowDays);
		}
		return SDate.Now().AddDays(((NetFieldBase<int, NetInt>)(object)crop.dayOfCurrentPhase).Value);
	}

	public Item GetSampleDrop()
	{
		if (Crop == null)
		{
			throw new InvalidOperationException("Can't get a sample drop because there's no crop.");
		}
		return ItemRegistry.Create(((NetFieldBase<string, NetString>)(object)Crop.indexOfHarvest).Value, 1, 0, false);
	}
}
