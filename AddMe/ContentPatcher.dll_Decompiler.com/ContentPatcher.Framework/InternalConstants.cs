using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework;

internal static class InternalConstants
{
	public const string PositionalInputArgSeparator = ":";

	public const string NamedInputArgSeparator = "|";

	public const string ModTokenSeparator = "/";

	public const string PlayerNamePrefix = "@";

	public const string TokenPlaceholder = "$~placeholder";

	public static readonly ISet<ConditionType> FromFileTokens = new HashSet<ConditionType> { ConditionType.FromFile };

	public static readonly ISet<ConditionType> TargetTokens = new HashSet<ConditionType>
	{
		ConditionType.Target,
		ConditionType.TargetPathOnly,
		ConditionType.TargetWithoutPath
	};
}
