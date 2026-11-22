using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_4 : BaseMigration
{
	public Migration_1_4()
		: base((ISemanticVersion)new SemanticVersion(1, 4, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("DayEvent", "HasFlag", "HasSeenEvent", "Hearts", "Relationship", "Spouse");
	}
}
