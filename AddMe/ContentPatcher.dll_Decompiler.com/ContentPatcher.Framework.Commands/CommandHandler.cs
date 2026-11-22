using System;
using ContentPatcher.Framework.Commands.Commands;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Commands;

internal class CommandHandler : GenericCommandHandler
{
	public CommandHandler(PerScreen<ScreenManager> screenManager, IMonitor monitor, IGameContentHelper contentHelper, LoadedContentPack[] contentPacks, Func<string?, IContext> getContext, Action updateContext)
		: base("patch", "Content Patcher", BuildCommands(screenManager, monitor, contentHelper, contentPacks, getContext, updateContext), monitor)
	{
	}

	private static ICommand[] BuildCommands(PerScreen<ScreenManager> screenManager, IMonitor monitor, IGameContentHelper contentHelper, LoadedContentPack[] contentPacks, Func<string?, IContext> getContext, Action updateContext)
	{
		return new ICommand[7]
		{
			new DumpCommand(monitor, () => screenManager.Value.PatchManager),
			new ExportCommand(monitor, contentHelper),
			new InvalidateCommand(monitor, contentHelper),
			new ParseCommand(monitor, getContext),
			new ReloadCommand(monitor, () => screenManager.Value.PatchLoader, () => screenManager.Value.PatchManager, contentPacks, updateContext),
			new SummaryCommand(monitor, contentHelper, () => screenManager.Value.PatchManager, () => screenManager.Value.TokenManager, () => screenManager.Value.CustomLocationManager),
			new UpdateCommand(monitor, updateContext)
		};
	}
}
