using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.LineSprinklers;

internal class LineSprinklersIntegration : BaseIntegration<ILineSprinklersApi>
{
	public int MaxRadius { get; }

	public LineSprinklersIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Line Sprinklers", "hootless.LineSprinklers", "1.1.0", modRegistry, monitor)
	{
		if (base.IsLoaded)
		{
			MaxRadius = SafelyCallApi((ILineSprinklersApi api) => api.GetMaxGridSize(), "Failed getting max grid size from {0}.", 0);
		}
	}

	public IDictionary<int, Vector2[]> GetSprinklerTiles()
	{
		AssertLoaded();
		return SafelyCallApi((ILineSprinklersApi api) => api.GetSprinklerCoverage(), "Failed getting sprinkler tiles from {0}.") ?? new Dictionary<int, Vector2[]>();
	}
}
