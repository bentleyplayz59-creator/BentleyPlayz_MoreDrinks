using System;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.BushBloomMod;

public interface IBushBloomModApi
{
	(string, WorldDate, WorldDate)[] GetActiveSchedules(string season, int dayofMonth, int? year = null, GameLocation? location = null, Nullable tile = null);

	bool IsReady();
}
