using System;
using System.Collections.Generic;

namespace Pathoschild.Stardew.Common.Utilities;

internal class HumanSortComparer : IComparer<string?>
{
	private readonly IComparer<string?> AlphaComparer;

	public static readonly HumanSortComparer DefaultIgnoreCase = new HumanSortComparer(ignoreCase: true);

	public HumanSortComparer(bool ignoreCase = false)
	{
		AlphaComparer = (ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
	}

	public int Compare(string? a, string? b)
	{
		if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
		{
			return AlphaComparer.Compare(a, b);
		}
		int position = 0;
		int position2 = 0;
		int num;
		do
		{
			GetNextPart(a, ref position, out string raw, out long? numeric);
			GetNextPart(b, ref position2, out string raw2, out long? numeric2);
			bool flag = numeric.HasValue && numeric2.HasValue;
			if (raw == null && raw2 == null)
			{
				return 0;
			}
			if (raw == null)
			{
				return -1;
			}
			if (raw2 == null)
			{
				return 1;
			}
			if (flag)
			{
				for (int i = 0; i < raw.Length && i < raw2.Length; i++)
				{
					bool flag2 = raw[i] == '0';
					bool flag3 = raw2[i] == '0';
					if (!(flag2 & flag3))
					{
						if (flag2)
						{
							return -1;
						}
						if (flag3)
						{
							return 1;
						}
						break;
					}
				}
			}
			num = (flag ? numeric.Value.CompareTo(numeric2.Value) : AlphaComparer.Compare(raw, raw2));
			if (num < 0)
			{
				return -1;
			}
		}
		while (num <= 0);
		return 1;
	}

	private void GetNextPart(string str, ref int position, out string? raw, out long? numeric)
	{
		if (position >= str.Length)
		{
			raw = null;
			numeric = null;
			return;
		}
		int num = position;
		bool flag = char.IsNumber(str[num]);
		position++;
		while (position < str.Length && char.IsNumber(str[position]) == flag)
		{
			position++;
		}
		raw = str.Substring(num, position - num);
		numeric = ((flag && long.TryParse(raw, out var result)) ? new long?(result) : ((long?)null));
	}
}
