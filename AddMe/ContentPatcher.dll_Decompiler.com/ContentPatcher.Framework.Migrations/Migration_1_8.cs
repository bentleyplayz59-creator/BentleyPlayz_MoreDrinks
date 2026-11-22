using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_8 : BaseMigration
{
	public Migration_1_8()
		: base((ISemanticVersion)new SemanticVersion(1, 8, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("IsOutdoors", "LocationName", "Target", "TargetWithoutPath");
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
			if (HasAction(patchConfig, PatchType.EditMap))
			{
				error = GetNounPhraseError("using action EditMap");
				return false;
			}
			if (patchConfig.MoveEntries.Any())
			{
				error = GetNounPhraseError("using MoveEntries");
				return false;
			}
		}
		return true;
	}
}
