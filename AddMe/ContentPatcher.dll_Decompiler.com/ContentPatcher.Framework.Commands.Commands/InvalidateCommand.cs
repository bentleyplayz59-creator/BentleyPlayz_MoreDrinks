using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands;

internal class InvalidateCommand : BaseCommand
{
	private readonly IGameContentHelper ContentHelper;

	public InvalidateCommand(IMonitor monitor, IGameContentHelper contentHelper)
		: base(monitor, "invalidate")
	{
		ContentHelper = contentHelper;
	}

	public override string GetDescription()
	{
		return "patch invalidate\n   Usage: patch invalidate \"<asset name>\"\n   Invalidates an asset from the game/SMAPI content cache. If it's an asset handled by SMAPI, the asset will be reloaded immediately and Content Patcher will reapply its changes to it. Otherwise the next code which loads the same asset will get a new instance.";
	}

	public override void Handle(string[] args)
	{
		if (args.Length != 1)
		{
			Monitor.Log("The 'patch invalidate' command expects one argument containing the target asset name. See 'patch help' for more info.", (LogLevel)4);
			return;
		}
		string text = args[0];
		if (ContentHelper.InvalidateCache(text))
		{
			Monitor.Log("Invalidated asset '" + text + "'.", (LogLevel)2);
		}
		else
		{
			Monitor.Log("The asset '" + text + "' wasn't found in the cache.", (LogLevel)3);
		}
	}
}
