using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.Common.Integrations.CustomBush;

internal class CustomBushIntegration : BaseIntegration<ICustomBushApi>
{
	public CustomBushIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("CustomBush", "furyx639.CustomBush", "1.2.0", modRegistry, monitor)
	{
	}

	public bool TryGetCustomBush(Bush bush, out ICustomBush? customBush)
	{
		AssertLoaded();
		customBush = SafelyCallApi((ICustomBushApi api) => api.TryGetCustomBush(bush, out ICustomBush customBush2) ? customBush2 : null, "Failed fetching bush from {0}.");
		return customBush != null;
	}

	public bool TryGetCustomBush(Bush bush, [NotNullWhen(true)] out string? id)
	{
		AssertLoaded();
		id = SafelyCallApi((ICustomBushApi api) => api.TryGetCustomBush(bush, out ICustomBush _, out string id2) ? id2 : null, "Failed fetching bush ID from {0}.");
		return id != null;
	}

	public bool TryGetDrops(string id, [NotNullWhen(true)] out IList<ICustomBushDrop>? drops)
	{
		AssertLoaded();
		drops = SafelyCallApi((ICustomBushApi api) => api.TryGetDrops(id, out IList<ICustomBushDrop> drops2) ? drops2 : null, "Failed fetching bush drops from {0}.");
		return drops != null;
	}
}
