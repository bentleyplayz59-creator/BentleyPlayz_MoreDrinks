using System;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands;

internal class UpdateCommand : BaseCommand
{
	private readonly Action UpdateContext;

	public UpdateCommand(IMonitor monitor, Action updateContext)
		: base(monitor, "update")
	{
		UpdateContext = updateContext;
	}

	public override string GetDescription()
	{
		return "patch update\n   Usage: patch update\n   Immediately refreshes the condition context and rechecks all patches.";
	}

	public override void Handle(string[] args)
	{
		UpdateContext();
		Monitor.Log("Updated the Content Patcher context.", (LogLevel)2);
	}
}
