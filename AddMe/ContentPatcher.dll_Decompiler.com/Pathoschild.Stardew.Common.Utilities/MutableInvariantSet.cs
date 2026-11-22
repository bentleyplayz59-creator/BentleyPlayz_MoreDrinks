using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Pathoschild.Stardew.Common.Utilities;

internal class MutableInvariantSet : ISet<string>, ICollection<string>, IEnumerable<string>, IEnumerable
{
	private HashSet<string>? Set;

	private IInvariantSet? CachedImmutableSet;

	public int Count => Set?.Count ?? 0;

	public bool IsReadOnly { get; private set; }

	[MemberNotNullWhen(false, "Set")]
	public bool IsEmpty
	{
		[MemberNotNullWhen(false, "Set")]
		get
		{
			int? num = Set?.Count;
			if (!num.HasValue || num.GetValueOrDefault() == 0)
			{
				return true;
			}
			return false;
		}
	}

	public MutableInvariantSet()
	{
	}

	public MutableInvariantSet(IEnumerable<string> values)
	{
		Set = CreateSet(values);
	}

	public IEnumerator<string> GetEnumerator()
	{
		HashSet<string>.Enumerator? enumerator = Set?.GetEnumerator();
		IEnumerator<string> result;
		if (!enumerator.HasValue)
		{
			result = Enumerable.Empty<string>().GetEnumerator();
		}
		else
		{
			IEnumerator<string> enumerator2 = enumerator.GetValueOrDefault();
			result = enumerator2;
		}
		return result;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		HashSet<string>.Enumerator? enumerator = Set?.GetEnumerator();
		IEnumerator<string> result;
		if (!enumerator.HasValue)
		{
			result = Enumerable.Empty<string>().GetEnumerator();
		}
		else
		{
			IEnumerator<string> enumerator2 = enumerator.GetValueOrDefault();
			result = enumerator2;
		}
		return result;
	}

	public bool IsProperSubsetOf(IEnumerable<string> other)
	{
		if (IsEmpty && other is ICollection<string> collection)
		{
			return collection.Count > 0;
		}
		return EnsureSet().IsProperSubsetOf(other);
	}

	public bool IsProperSupersetOf(IEnumerable<string> other)
	{
		if (IsEmpty)
		{
			return false;
		}
		return Set.IsProperSupersetOf(other);
	}

	public bool IsSubsetOf(IEnumerable<string> other)
	{
		if (IsEmpty)
		{
			return true;
		}
		return Set.IsSubsetOf(other);
	}

	public bool IsSupersetOf(IEnumerable<string> other)
	{
		if (IsEmpty && other is ICollection<string> collection)
		{
			return collection.Count == 0;
		}
		return EnsureSet().IsSupersetOf(other);
	}

	public bool Overlaps(IEnumerable<string> other)
	{
		if (IsEmpty)
		{
			return false;
		}
		return Set.Overlaps(other);
	}

	public bool SetEquals(IEnumerable<string> other)
	{
		if (IsEmpty && other is ICollection<string> collection)
		{
			return collection.Count == 0;
		}
		return EnsureSet().SetEquals(other);
	}

	public bool Contains(string item)
	{
		if (IsEmpty)
		{
			return false;
		}
		return Set.Contains(item);
	}

	public void CopyTo(string[] array, int arrayIndex)
	{
		if (!IsEmpty)
		{
			Set.CopyTo(array, arrayIndex);
		}
	}

	void ICollection<string>.Add(string item)
	{
		AssertNotLocked();
		if (EnsureSet().Add(item))
		{
			ClearCache();
		}
	}

	public bool Add(string item)
	{
		AssertNotLocked();
		if (EnsureSet().Add(item))
		{
			ClearCache();
			return true;
		}
		return false;
	}

	public void Clear()
	{
		AssertNotLocked();
		if (!IsEmpty)
		{
			Set.Clear();
			ClearCache();
		}
	}

	public bool Remove(string item)
	{
		AssertNotLocked();
		if (IsEmpty)
		{
			return false;
		}
		if (Set.Remove(item))
		{
			ClearCache();
			return true;
		}
		return false;
	}

	public void ExceptWith(IEnumerable<string> other)
	{
		AssertNotLocked();
		if (!IsEmpty)
		{
			int count = Count;
			EnsureSet().ExceptWith(other);
			ClearCacheUnlessCount(count);
		}
	}

	public void IntersectWith(IEnumerable<string> other)
	{
		AssertNotLocked();
		if (!IsEmpty)
		{
			int count = Count;
			EnsureSet().IntersectWith(other);
			ClearCacheUnlessCount(count);
		}
	}

	public void SymmetricExceptWith(IEnumerable<string> other)
	{
		AssertNotLocked();
		if (!IsEmpty || !(other is ICollection<string> { Count: 0 }))
		{
			EnsureSet().SymmetricExceptWith(other);
			ClearCache();
		}
	}

	public void UnionWith(IEnumerable<string> other)
	{
		AssertNotLocked();
		if (IsEmpty)
		{
			if (!(other is ICollection<string> { Count: 0 }))
			{
				Set = CreateSet(other);
				ClearCacheUnlessCount(0);
			}
		}
		else
		{
			int count = Count;
			Set.UnionWith(other);
			ClearCacheUnlessCount(count);
		}
	}

	public IInvariantSet Lock()
	{
		IsReadOnly = true;
		return GetImmutable();
	}

	public IInvariantSet GetImmutable()
	{
		if (CachedImmutableSet == null)
		{
			int? num = Set?.Count;
			if (!num.HasValue || num.GetValueOrDefault() <= 0)
			{
				CachedImmutableSet = InvariantSet.Empty;
			}
			else
			{
				if (!IsReadOnly)
				{
					HashSet<string> hashSet = new HashSet<string>();
					foreach (string item in Set)
					{
						hashSet.Add(item);
					}
					HashSet<string> values = hashSet;
					return new InvariantSet(values);
				}
				CachedImmutableSet = new InvariantSet(Set);
			}
		}
		return CachedImmutableSet;
	}

	private void AssertNotLocked()
	{
		if (IsReadOnly)
		{
			throw new NotSupportedException("This set is locked and doesn't allow further changes.");
		}
	}

	[MemberNotNull("Set")]
	private HashSet<string> EnsureSet()
	{
		return Set ?? (Set = CreateSet());
	}

	private void ClearCache()
	{
		CachedImmutableSet = null;
	}

	private void ClearCacheUnlessCount(int count)
	{
		if (count != Count)
		{
			CachedImmutableSet = null;
		}
	}

	private HashSet<string> CreateSet()
	{
		return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private HashSet<string> CreateSet(IEnumerable<string> values)
	{
		return new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
	}
}
