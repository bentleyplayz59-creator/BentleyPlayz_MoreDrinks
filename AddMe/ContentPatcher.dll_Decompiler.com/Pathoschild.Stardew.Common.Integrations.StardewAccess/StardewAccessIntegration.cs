using StardewModdingAPI;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.Integrations.StardewAccess;

internal class StardewAccessIntegration : BaseIntegration<IStardewAccessApi>
{
	public StardewAccessIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Stardew Access", "shoaib.stardewaccess", "1.6.2", modRegistry, monitor)
	{
	}

	public bool Say(string text, bool interrupt)
	{
		return SafelyCallApi((IStardewAccessApi api) => api.Say(text, interrupt), "saying text", defaultValue: false);
	}

	public bool SayWithMenuChecker(string text, bool interrupt, string? customQuery = null)
	{
		return SafelyCallApi((IStardewAccessApi api) => api.SayWithMenuChecker(text, interrupt, customQuery), "saying menu text", defaultValue: false);
	}

	public bool SayMenuElement(IScreenReadable element, bool interrupt = true)
	{
		return SafelyCallApi((IStardewAccessApi api) => api.SayWithMenuChecker(element.ScreenReaderText, interrupt), "saying menu element", defaultValue: false);
	}
}
