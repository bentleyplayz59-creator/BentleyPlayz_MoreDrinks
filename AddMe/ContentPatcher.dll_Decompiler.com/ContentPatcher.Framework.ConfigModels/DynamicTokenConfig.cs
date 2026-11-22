using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels;

internal class DynamicTokenConfig
{
	public string? Name { get; }

	public string? Value { get; }

	public InvariantDictionary<string?> When { get; }

	public DynamicTokenConfig(string name, string value, InvariantDictionary<string?>? when)
	{
		Name = name;
		Value = value;
		When = when ?? new InvariantDictionary<string>();
	}
}
