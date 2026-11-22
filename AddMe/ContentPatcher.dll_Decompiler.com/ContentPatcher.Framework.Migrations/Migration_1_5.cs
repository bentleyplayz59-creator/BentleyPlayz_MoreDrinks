using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_5 : BaseMigration
{
	public Migration_1_5()
		: base((ISemanticVersion)new SemanticVersion(1, 5, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("FarmCave", "FarmhouseUpgrade", "FarmName", "HasFile", "HasProfession", "PlayerGender", "PlayerName", "PreferredPet", "Year");
	}

	public override bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrateMainContent(content, out error))
		{
			return false;
		}
		if (content.DynamicTokens.Any())
		{
			error = GetNounPhraseError("using the DynamicTokens field");
			return false;
		}
		return true;
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
			string? target = patchConfig.Target;
			if (target != null && target.Contains(","))
			{
				error = GetNounPhraseError("specifying multiple Target values");
				return false;
			}
		}
		return true;
	}
}
