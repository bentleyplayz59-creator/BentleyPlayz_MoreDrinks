using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands;

internal class GenericCommandHandler
{
	private readonly IMonitor Monitor;

	private readonly InvariantDictionary<ICommand> Commands;

	public string ModName { get; }

	public string RootName { get; }

	public GenericCommandHandler(string rootName, string modName, IEnumerable<ICommand> commands, IMonitor monitor)
	{
		RootName = rootName;
		ModName = modName;
		Monitor = monitor;
		Commands = new InvariantDictionary<ICommand>(commands.ToDictionary((ICommand p) => p.Name));
		Commands["help"] = new GenericHelpCommand(rootName, modName, monitor, () => Commands);
	}

	public bool Handle(string[] args)
	{
		string key = args.FirstOrDefault() ?? "help";
		string[] args2 = args.Skip(1).ToArray();
		if (Commands.TryGetValue(key, out ICommand value))
		{
			value.Handle(args2);
			return true;
		}
		Monitor.Log($"The '{RootName} {args[0]}' command isn't valid. Type '{RootName} {"help"}' for a list of valid commands.", (LogLevel)4);
		return false;
	}

	public void RegisterWith(ICommandHelper commandHelper)
	{
		commandHelper.Add(RootName, $"Starts a {ModName} command. Type '{RootName} {"help"}' for details.", (Action<string, string[]>)((string _, string[] args) =>
		{
			Handle(args);
		}));
	}
}
