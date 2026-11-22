using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux;

internal class CustomFarmingReduxIntegration : BaseIntegration<ICustomFarmingApi>
{
	public CustomFarmingReduxIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Custom Farming Redux", "Platonymous.CustomFarming", "2.8.5", modRegistry, monitor)
	{
	}

	public SpriteInfo? GetSprite(Object obj)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		AssertLoaded();
		Tuple<Item, Texture2D, Rectangle, Color> tuple = SafelyCallApi((ICustomFarmingApi api) => api.getRealItemAndTexture(obj), "Failed getting sprite from {0}.");
		return (tuple != null) ? new SpriteInfo(tuple.Item2, tuple.Item3) : null;
	}
}
