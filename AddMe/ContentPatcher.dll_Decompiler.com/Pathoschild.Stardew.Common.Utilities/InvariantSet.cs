using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathoschild.Stardew.Common.Utilities;

internal class InvariantSet : IInvariantSet, IReadOnlySet<string>, IEnumerable<string>, IEnumerable, IReadOnlyCollection<string>
{
	private static readonly HashSet<string> EmptyHashSet = new HashSet<string>();

	private readonly HashSet<string> Set;

	public static IInvariantSet Empty { get; } = new InvariantSet();

	public int Count => Set.Count;

	public InvariantSet()
	{
		Set = EmptyHashSet;
	}

	public InvariantSet(IEnumerable<string> values)
	{
		if (1 == 0)
		{
		}
		HashSet<string> set = ((values is InvariantSet invariantSet) ? invariantSet.Set : ((values is HashSet<string> collection) ? collection.ToNonNullCaseInsensitive() : ((!(values is ICollection<string> { Count: 0 })) ? CreateSet(values) : EmptyHashSet)));
		if (1 == 0)
		{
		}
		Set = set;
	}

	public InvariantSet(params string[] values)
	{
		Set = CreateSet(values);
	}

	public IEnumerator<string> GetEnumerator()
	{
		return Set.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Set.GetEnumerator();
	}

	public bool Contains(string item)
	{
		return Set.Contains(item);
	}

	public bool IsProperSubsetOf(IEnumerable<string> other)
	{
		return Set.IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<string> other)
	{
		return Set.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<string> other)
	{
		return Set.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<string> other)
	{
		return Set.IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<string> other)
	{
		return Set.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<string> other)
	{
		return Set.SetEquals(other);
	}

	public IInvariantSet GetWith(string other)
	{
		if (Count == 0)
		{
			return new InvariantSet(other);
		}
		if (Contains(other))
		{
			return this;
		}
		HashSet<string> hashSet = CreateSet(Set);
		hashSet.Add(other);
		return new InvariantSet(hashSet);
	}

	public IInvariantSet GetWith(ICollection<string> other)
	{
		if (other.Count == 0)
		{
			return this;
		}
		if (Count == 0)
		{
			return new InvariantSet(CreateSet(other));
		}
		bool flag = false;
		HashSet<string> hashSet = CreateSet(Set);
		foreach (string item in other)
		{
			flag |= hashSet.Add(item);
		}
		return flag ? new InvariantSet(hashSet) : this;
	}

	public IInvariantSet GetWithout(string other)
	{
		if (!Contains(other))
		{
			return this;
		}
		HashSet<string> hashSet = CreateSet(this);
		hashSet.Remove(other);
		return new InvariantSet(hashSet);
	}

	public IInvariantSet GetWithout(IEnumerable<string> other)
	{
		HashSet<string> hashSet = null;
		foreach (string item in other)
		{
			if (hashSet == null)
			{
				if (!Contains(item))
				{
					continue;
				}
				hashSet = CreateSet(this);
			}
			hashSet.Remove(item);
		}
		return (hashSet != null) ? new InvariantSet(hashSet) : this;
	}

	private HashSet<string> CreateSet(IEnumerable<string> values)
	{
		return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
	}
}
