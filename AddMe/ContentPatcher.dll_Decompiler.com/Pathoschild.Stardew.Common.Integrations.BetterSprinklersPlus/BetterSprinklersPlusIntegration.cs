using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.BetterSprinklersPlus;

internal class BetterSprinklersPlusIntegration : BaseIntegration<IBetterSprinklersPlusApi>
{
	public int MaxRadius { get; }

	public BetterSprinklersPlusIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Better Sprinklers Plus", "com.CodesThings.BetterSprinklersPlus", "2.6.0", modRegistry, monitor)
	{
		if (base.IsLoaded)
		{
			MaxRadius = SafelyCallApi((IBetterSprinklersPlusApi api) => api.GetMaxGridSize(), "Failed fetching max radius from {0}.", 0);
		}
	}

	public IDictionary<int, Vector2[]> GetSprinklerTiles()
	{
		AssertLoaded();
		return SafelyCallApi((IBetterSprinklersPlusApi api) => api.GetSprinklerCoverage(), "Failed fetching sprinkler tiles from {0}.") ?? new Dictionary<int, Vector2[]>();
	}
}
