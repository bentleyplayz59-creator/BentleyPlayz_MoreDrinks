namespace Pathoschild.Stardew.Common.Integrations.StardewAccess;

public interface IStardewAccessApi
{
	bool Say(string text, bool interrupt);

	bool SayWithMenuChecker(string text, bool interrupt, string? customQuery = null);
}
