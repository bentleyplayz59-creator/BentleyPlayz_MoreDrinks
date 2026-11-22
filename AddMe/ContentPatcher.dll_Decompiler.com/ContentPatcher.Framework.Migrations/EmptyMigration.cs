using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal sealed class EmptyMigration : BaseMigration
{
	public EmptyMigration(int majorVersion, int minorVersion)
		: base((ISemanticVersion)new SemanticVersion(majorVersion, minorVersion, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
	}
}
