using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands;

internal abstract class BaseCommand : ICommand
{
	protected readonly IMonitor Monitor;

	public string Name { get; }

	public string Description => GetDescription();

	public abstract string GetDescription();

	public abstract void Handle(string[] args);

	protected BaseCommand(IMonitor monitor, string name)
	{
		Monitor = monitor;
		Name = name;
	}
}
