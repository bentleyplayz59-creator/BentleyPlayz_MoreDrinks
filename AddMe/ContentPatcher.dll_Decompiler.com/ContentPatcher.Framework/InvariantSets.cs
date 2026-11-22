using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework;

internal static class InvariantSets
{
	private static readonly IInvariantSet BlankString = new InvariantSet(string.Empty);

	private static readonly IInvariantSet Zero = new InvariantSet("0");

	private static readonly IInvariantSet One = new InvariantSet("1");

	private static readonly IInvariantSet Spring = new InvariantSet("spring");

	private static readonly IInvariantSet Summer = new InvariantSet("summer");

	private static readonly IInvariantSet Fall = new InvariantSet("fall");

	private static readonly IInvariantSet Winter = new InvariantSet("winter");

	public static readonly IInvariantSet Empty = InvariantSet.Empty;

	public static readonly IInvariantSet True = new InvariantSet("true");

	public static readonly IInvariantSet False = new InvariantSet("false");

	public static readonly IInvariantSet Boolean = new InvariantSet("true", "false");

	public static IInvariantSet From(IEnumerable<string> values)
	{
		if (!(values is IInvariantSet result))
		{
			if (values is IList<string> list)
			{
				switch (list.Count)
				{
				case 0:
					return Empty;
				case 1:
					return FromValue(list[0]);
				case 2:
				{
					if (!bool.TryParse(list[0], out var result2) || !bool.TryParse(list[1], out var result3))
					{
						break;
					}
					if (result2 != result3)
					{
						return Boolean;
					}
					return result2 ? True : False;
				}
				}
			}
			IInvariantSet invariantSet2;
			if (!(values is MutableInvariantSet mutableInvariantSet))
			{
				IInvariantSet invariantSet = new InvariantSet(values);
				invariantSet2 = invariantSet;
			}
			else
			{
				invariantSet2 = mutableInvariantSet.GetImmutable();
			}
			IInvariantSet invariantSet3 = invariantSet2;
			return (invariantSet3.Count == 0) ? Empty : invariantSet3;
		}
		return result;
	}

	public static IInvariantSet FromValue(bool value)
	{
		return value ? True : False;
	}

	public static IInvariantSet FromValue(int value)
	{
		if (1 == 0)
		{
		}
		IInvariantSet result = value switch
		{
			0 => Zero, 
			1 => One, 
			_ => new InvariantSet(value.ToString()), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public static IInvariantSet FromValue(string value)
	{
		return FromPredefinedValueOnly(value) ?? new InvariantSet(value);
	}

	public static IInvariantSet FromCaseSensitiveValue(string value)
	{
		return FromPredefinedValueOnlyCaseSensitive(value) ?? new InvariantSet(value);
	}

	private static IInvariantSet? FromPredefinedValueOnly(string value)
	{
		if (bool.TryParse(value, out var result))
		{
			return result ? True : False;
		}
		if (1 == 0)
		{
		}
		IInvariantSet result2;
		switch (value)
		{
		case "":
			result2 = BlankString;
			break;
		case "0":
			result2 = Zero;
			break;
		case "1":
			result2 = One;
			break;
		case "spring":
		case "Spring":
			result2 = Spring;
			break;
		case "summer":
		case "Summer":
			result2 = Summer;
			break;
		case "fall":
		case "Fall":
			result2 = Fall;
			break;
		case "winter":
		case "Winter":
			result2 = Winter;
			break;
		default:
			result2 = null;
			break;
		}
		if (1 == 0)
		{
		}
		return result2;
	}

	private static IInvariantSet? FromPredefinedValueOnlyCaseSensitive(string value)
	{
		if (1 == 0)
		{
		}
		IInvariantSet result = value switch
		{
			"true" => True, 
			"false" => False, 
			"" => BlankString, 
			"0" => Zero, 
			"1" => One, 
			"spring" => Spring, 
			"summer" => Summer, 
			"fall" => Fall, 
			"winter" => Winter, 
			_ => null, 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
