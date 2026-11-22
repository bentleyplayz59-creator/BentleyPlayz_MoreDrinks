using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_14 : BaseMigration
{
	public Migration_1_14()
		: base((ISemanticVersion)new SemanticVersion(1, 14, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("FromFile", "Round");
	}
}
