using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.SimpleSprinkler;

internal class SimpleSprinklerIntegration : BaseIntegration<ISimplerSprinklerApi>
{
	public SimpleSprinklerIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Simple Sprinklers", "tZed.SimpleSprinkler", "1.6.0", modRegistry, monitor)
	{
	}

	public IDictionary<int, Vector2[]> GetNewSprinklerTiles()
	{
		AssertLoaded();
		return SafelyCallApi((ISimplerSprinklerApi api) => api.GetNewSprinklerCoverage(), "Failed getting sprinkler coverage from {0}.") ?? new Dictionary<int, Vector2[]>();
	}
}
