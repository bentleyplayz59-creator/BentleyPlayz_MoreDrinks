using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_10 : BaseMigration
{
	public Migration_1_10()
		: base((ISemanticVersion)new SemanticVersion(1, 10, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("HasDialogueAnswer", "HavingChild", "IsJojaMartComplete", "Pregnant", "Random");
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
			if (patchConfig.FromFile != null && HasAction(patchConfig, PatchType.EditData))
			{
				error = GetNounPhraseError("using FromFile with action EditData");
				return false;
			}
			if (patchConfig.MapProperties.Any())
			{
				error = GetNounPhraseError("using MapProperties");
				return false;
			}
		}
		return true;
	}
}
