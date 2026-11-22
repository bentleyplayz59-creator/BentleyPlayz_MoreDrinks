using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathoschild.Stardew.Common.Utilities;

internal class InvariantDictionary<TValue> : Dictionary<string, TValue>
{
	public InvariantDictionary()
		: base((IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase)
	{
	}

	public InvariantDictionary(IDictionary<string, TValue> values)
		: base(values, (IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase)
	{
	}

	public InvariantDictionary(IEnumerable<KeyValuePair<string, TValue>> values)
		: base((IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase)
	{
		foreach (KeyValuePair<string, TValue> value in values)
		{
			Add(value.Key, value.Value);
		}
	}

	public InvariantDictionary<TValue> Clone(Func<TValue, TValue>? cloneValue = null)
	{
		return (cloneValue != null) ? new InvariantDictionary<TValue>(this.ToDictionary((KeyValuePair<string, TValue> p) => p.Key, (KeyValuePair<string, TValue> p) => cloneValue(p.Value))) : new InvariantDictionary<TValue>(this);
	}
}
