using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace Pathoschild.Stardew.Common.Integrations.HaveMoreKids;

internal class HaveMoreKidsIntegration : BaseIntegration<IHaveMoreKidsAPI>
{
	public HaveMoreKidsIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("HaveMoreKids", "mushymato.HaveMoreKids", "1.1.0", modRegistry, monitor)
	{
	}

	public int? GetDaysToNextChildGrowth(Child kid)
	{
		return SafelyCallApi((IHaveMoreKidsAPI api) => api.GetDaysToNextChildGrowth(kid), "Failed getting days to next growth for '" + ((Character)kid).Name + "' from '{0}'.", 0);
	}

	public string? GetChildBirthdayString(Child kid)
	{
		return SafelyCallApi((IHaveMoreKidsAPI api) => api.GetChildBirthdayString(kid), "Failed getting birthday string for '" + ((Character)kid).Name + "' from '{0}'.");
	}
}
