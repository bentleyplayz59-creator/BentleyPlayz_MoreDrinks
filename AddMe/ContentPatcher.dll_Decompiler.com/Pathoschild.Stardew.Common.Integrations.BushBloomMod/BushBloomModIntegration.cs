using System;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.BushBloomMod;

internal class BushBloomModIntegration : BaseIntegration<IBushBloomModApi>
{
	public BushBloomModIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("CustomBush", "NCarigon.BushBloomMod", "1.2.4", modRegistry, monitor)
	{
	}

	public (string, WorldDate, WorldDate)[] GetActiveSchedules(string season, int dayOfMonth, int? year = null, GameLocation? location = null, Nullable tile = null)
	{
		return SafelyCallApi((IBushBloomModApi api) => api.GetActiveSchedules(season, dayOfMonth, year, location, tile), "Failed fetching active schedules from {0}.") ?? Array.Empty<(string, WorldDate, WorldDate)>();
	}

	public bool IsReady()
	{
		return SafelyCallApi((IBushBloomModApi api) => api.IsReady(), "Failed checking if {0} is ready.", defaultValue: false);
	}
}
