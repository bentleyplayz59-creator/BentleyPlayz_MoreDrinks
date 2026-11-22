using System;

namespace Pathoschild.Stardew.Common.Utilities;

internal class Cached<TValue>
{
	private readonly Func<string> GetCacheKey;

	private readonly Func<TValue?, TValue> FetchNew;

	private string? LastCacheKey;

	private TValue? LastValue;

	public TValue Value
	{
		get
		{
			string text = GetCacheKey();
			if (text != LastCacheKey)
			{
				LastCacheKey = text;
				LastValue = FetchNew(LastValue);
			}
			return LastValue;
		}
	}

	public Cached(Func<string> getCacheKey, Func<TValue> fetchNew)
		: this(getCacheKey, (Func<TValue?, TValue>)((TValue? _) => fetchNew()))
	{
	}

	public Cached(Func<string> getCacheKey, Func<TValue?, TValue> fetchNew)
	{
		GetCacheKey = getCacheKey;
		FetchNew = fetchNew;
	}
}
