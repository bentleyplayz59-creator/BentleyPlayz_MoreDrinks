using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands;

internal class GenericHelpCommand : BaseCommand
{
	private readonly string ModName;

	private readonly string RootName;

	private readonly Func<InvariantDictionary<ICommand>> GetCommands;

	internal const string CommandName = "help";

	public GenericHelpCommand(string rootName, string modName, IMonitor monitor, Func<InvariantDictionary<ICommand>> getCommands)
		: base(monitor, "help")
	{
		ModName = modName;
		RootName = rootName;
		GetCommands = getCommands;
	}

	public override string GetDescription()
	{
		return $"\n                {RootName} {"help"}\n                   Usage: {RootName} {"help"}\n                   Lists all available {RootName} commands.\n\n                   Usage: {RootName} {"help"} <cmd>\n                   Provides information for a specific {RootName} command.\n                   - cmd: The {RootName} command name.\n            ";
	}

	public override void Handle(string[] args)
	{
		InvariantDictionary<ICommand> invariantDictionary = GetCommands();
		StringBuilder stringBuilder = new StringBuilder();
		ICommand value;
		if (!args.Any())
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(150, 5, stringBuilder2);
			handler.AppendLiteral("The '");
			handler.AppendFormatted(RootName);
			handler.AppendLiteral("' command is the entry point for ");
			handler.AppendFormatted(ModName);
			handler.AppendLiteral(" commands. You use it by specifying a more ");
			handler.AppendLiteral("specific command (like '");
			handler.AppendFormatted("help");
			handler.AppendLiteral("' in '");
			handler.AppendFormatted(RootName);
			handler.AppendLiteral(" ");
			handler.AppendFormatted("help");
			handler.AppendLiteral("'). Here are the available commands:\n\n");
			stringBuilder3.AppendLine(ref handler);
			foreach (KeyValuePair<string, ICommand> item in invariantDictionary.OrderBy((KeyValuePair<string, ICommand> p) => p.Key, HumanSortComparer.DefaultIgnoreCase))
			{
				stringBuilder.AppendLine(item.Value.Description);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
		}
		else if (invariantDictionary.TryGetValue(args[0], out value))
		{
			stringBuilder.AppendLine(value.Description);
		}
		else
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(53, 4, stringBuilder2);
			handler.AppendLiteral("Unknown command '");
			handler.AppendFormatted(RootName);
			handler.AppendLiteral(" ");
			handler.AppendFormatted(args[0]);
			handler.AppendLiteral("'. Type '");
			handler.AppendFormatted(RootName);
			handler.AppendLiteral(" ");
			handler.AppendFormatted("help");
			handler.AppendLiteral("' for available commands.");
			stringBuilder4.AppendLine(ref handler);
		}
		Monitor.Log(stringBuilder.ToString().Trim(), (LogLevel)2);
	}
}
