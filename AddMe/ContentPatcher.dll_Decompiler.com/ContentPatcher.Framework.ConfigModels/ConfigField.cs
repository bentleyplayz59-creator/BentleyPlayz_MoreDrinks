using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels;

internal class ConfigField
{
	public IInvariantSet AllowValues { get; }

	public IInvariantSet DefaultValues { get; }

	public bool AllowBlank { get; }

	public bool AllowMultiple { get; }

	public IInvariantSet Value { get; private set; }

	public string? Description { get; }

	public string? Section { get; }

	public ConfigField(IInvariantSet? allowValues, IInvariantSet? defaultValues, IInvariantSet? value, bool allowBlank, bool allowMultiple, string? description, string? section)
	{
		AllowValues = allowValues ?? InvariantSets.Empty;
		DefaultValues = defaultValues ?? InvariantSets.Empty;
		Value = value ?? InvariantSets.Empty;
		AllowBlank = allowBlank;
		AllowMultiple = allowMultiple;
		Description = description;
		Section = section;
	}

	public bool IsBoolean()
	{
		if (AllowValues == InvariantSets.Boolean)
		{
			return true;
		}
		return AllowValues.Count == 2 && AllowValues.Contains(true.ToString()) && AllowValues.Contains(false.ToString());
	}

	public bool IsNumericRange(out int min, out int max)
	{
		List<int> list = new List<int>(AllowValues.Count);
		foreach (string allowValue in AllowValues)
		{
			if (!int.TryParse(allowValue, out var result))
			{
				min = -1;
				max = -1;
				return false;
			}
			list.Add(result);
		}
		list.Sort(Comparer<int>.Default);
		min = list.First();
		max = list.Last();
		return max - min == list.Count - 1;
	}

	public void SetValue(IInvariantSet value)
	{
		Value = value;
	}
}
