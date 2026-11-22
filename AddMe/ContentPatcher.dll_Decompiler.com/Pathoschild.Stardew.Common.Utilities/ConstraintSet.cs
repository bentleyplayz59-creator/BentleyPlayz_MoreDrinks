using System.Collections.Generic;
using StardewValley.Extensions;

namespace Pathoschild.Stardew.Common.Utilities;

internal class ConstraintSet<T>
{
	public HashSet<T> RestrictToValues { get; }

	public HashSet<T> ExcludeValues { get; }

	public bool IsBounded => RestrictToValues.Count != 0;

	public bool IsInfinite => !IsBounded;

	public bool IsConstrained => RestrictToValues.Count != 0 || ExcludeValues.Count != 0;

	public ConstraintSet()
		: this((IEqualityComparer<T>)EqualityComparer<T>.Default)
	{
	}

	public ConstraintSet(IEqualityComparer<T> comparer)
	{
		RestrictToValues = new HashSet<T>(comparer);
		ExcludeValues = new HashSet<T>(comparer);
	}

	public bool AddBound(T value)
	{
		return RestrictToValues.Add(value);
	}

	public bool AddBound(IEnumerable<T> values)
	{
		return CollectionExtensions.AddRange<T>((ISet<T>)RestrictToValues, values) > 0;
	}

	public bool Exclude(T value)
	{
		return ExcludeValues.Add(value);
	}

	public bool Exclude(IEnumerable<T> values)
	{
		return CollectionExtensions.AddRange<T>((ISet<T>)ExcludeValues, values) > 0;
	}

	public bool Intersects(ConstraintSet<T> other)
	{
		if (IsInfinite && other.IsInfinite)
		{
			return true;
		}
		if (IsBounded)
		{
			foreach (T restrictToValue in RestrictToValues)
			{
				if (Allows(restrictToValue) && other.Allows(restrictToValue))
				{
					return true;
				}
			}
		}
		if (other.IsBounded)
		{
			foreach (T restrictToValue2 in other.RestrictToValues)
			{
				if (other.Allows(restrictToValue2) && Allows(restrictToValue2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Allows(T value)
	{
		if (ExcludeValues.Contains(value))
		{
			return false;
		}
		return IsInfinite || RestrictToValues.Contains(value);
	}
}
