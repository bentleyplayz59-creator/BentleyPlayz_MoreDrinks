using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_6 : BaseMigration
{
	public Migration_1_6()
		: base((ISemanticVersion)new SemanticVersion(1, 6, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("HasWalletItem", "SkillLevel");
	}

	public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref patches, out error))
		{
			return false;
		}
		PatchConfig[] array = patches;
		foreach (PatchConfig patchConfig in array)
		{
			if (patchConfig.When.TryGetValue("Weather", out string value) && value != null && value.Contains("Sun"))
			{
				patchConfig.When["Weather"] = value + ", Wind";
			}
		}
		return true;
	}
}
