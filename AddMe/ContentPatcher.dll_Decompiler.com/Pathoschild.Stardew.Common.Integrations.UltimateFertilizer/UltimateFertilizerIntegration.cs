using System;
using System.Collections.Generic;
using Netcode;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Common.Integrations.UltimateFertilizer;

internal class UltimateFertilizerIntegration : BaseIntegration
{
	public UltimateFertilizerIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Ultimate Fertilizer", "fox_white25.ultimate_fertilizer", "1.2.5", modRegistry, monitor)
	{
	}

	public IEnumerable<string> GetAppliedFertilizers(HoeDirt dirt)
	{
		if (IsLoaded && !string.IsNullOrWhiteSpace(((NetFieldBase<string, NetString>)(object)dirt.fertilizer).Value))
		{
			string[] array = ((NetFieldBase<string, NetString>)(object)dirt.fertilizer).Value.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			for (int i = 0; i < array.Length; i++)
			{
				yield return array[i];
			}
		}
	}
}
