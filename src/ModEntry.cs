using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace BentleyPlayz.MoreDrinks;

internal sealed class ModEntry : Mod
{
    private const string ContentPackId = "BentleyPlayz.MoreDrinks.Content";
    private const string ShopId = "BentleyPlayz_MoreDrinks";

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.ConsoleCommands.Add(
            "bentley_more_drinks_status",
            "Checks whether the More Drinks Content Patcher pack is loaded. The Saloon kiosk uses the game's OpenShop tile action.",
            this.OnStatusCommand
        );
        this.Monitor.Log("BentleyPlayz More Drinks code component loaded. The kiosk shop is connected through the Saloon map's OpenShop action.", LogLevel.Info);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        bool loaded = this.Helper.ModRegistry.IsLoaded(ContentPackId);
        this.Monitor.Log(
            loaded
                ? $"Content Patcher pack '{ContentPackId}' detected. Shop ID: {ShopId}."
                : $"Content Patcher pack '{ContentPackId}' wasn't detected. Install the Content folder beside the Code folder.",
            loaded ? LogLevel.Info : LogLevel.Warn
        );
    }

    private void OnStatusCommand(string command, string[] args)
    {
        bool loaded = this.Helper.ModRegistry.IsLoaded(ContentPackId);
        this.Monitor.Log(
            loaded
                ? $"More Drinks code DLL is loaded; Content Patcher pack '{ContentPackId}' is registered. Kiosk shop ID: {ShopId}."
                : $"More Drinks code DLL is loaded, but Content Patcher pack '{ContentPackId}' is missing.",
            loaded ? LogLevel.Info : LogLevel.Warn
        );
    }
}
