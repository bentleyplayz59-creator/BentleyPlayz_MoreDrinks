using StardewModdingAPI;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.BetterGameMenu;

internal class BetterGameMenuIntegration : BaseIntegration<IBetterGameMenuApi>
{
	public BetterGameMenuIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("BetterGameMenu", "leclair.bettergamemenu", "0.5.2", modRegistry, monitor)
	{
	}

	public IClickableMenu? GetCurrentPage(IClickableMenu? menu)
	{
		if (menu == null)
		{
			return null;
		}
		return SafelyCallApi((IBetterGameMenuApi api) => api.GetCurrentPage(menu), "Failed getting game menu page from {0}.");
	}
}
