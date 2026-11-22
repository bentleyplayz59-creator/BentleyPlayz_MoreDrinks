using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace BentleyPlayz.MoreDrinks;

internal sealed class ModEntry : Mod
{
    private const string ContentPackId = "BentleyPlayz.MoreDrinks.Content";

    public override void Entry(IModHelper helper)
    {
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Monitor.Log("BentleyPlayz More Drinks code component loaded.", LogLevel.Info);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        bool loaded = this.Helper.ModRegistry.IsLoaded(ContentPackId);
        this.Monitor.Log(
            loaded
                ? $"Content Patcher pack '{ContentPackId}' detected."
                : $"Content Patcher pack '{ContentPackId}' wasn't detected. Make sure the Content folder is beside the Code folder.",
            loaded ? LogLevel.Info : LogLevel.Warn
        );
    }
}
