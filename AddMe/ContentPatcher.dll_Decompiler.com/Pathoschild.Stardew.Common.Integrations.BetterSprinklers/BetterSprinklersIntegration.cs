using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.BetterSprinklers;

internal class BetterSprinklersIntegration : BaseIntegration<IBetterSprinklersApi>
{
	public int MaxRadius { get; }

	public BetterSprinklersIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Better Sprinklers", "Speeder.BetterSprinklers", "2.3.1-unofficial.6-pathoschild", modRegistry, monitor)
	{
		if (base.IsLoaded)
		{
			MaxRadius = ModApi.GetMaxGridSize();
		}
	}

	public IDictionary<int, Vector2[]> GetSprinklerTiles()
	{
		AssertLoaded();
		return SafelyCallApi((IBetterSprinklersApi api) => api.GetSprinklerCoverage(), "Failed getting sprinkler tiles from {0}.") ?? new Dictionary<int, Vector2[]>();
	}
}
