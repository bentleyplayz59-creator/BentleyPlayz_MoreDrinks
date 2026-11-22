using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_2_2 : BaseMigration
{
	public Migration_2_2()
		: base((ISemanticVersion)new SemanticVersion(2, 2, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("FarmMapAsset", "HasVisitedLocation");
	}
}
