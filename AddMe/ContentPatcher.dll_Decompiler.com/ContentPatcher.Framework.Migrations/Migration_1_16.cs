using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_16 : BaseMigration
{
	public Migration_1_16()
		: base((ISemanticVersion)new SemanticVersion(1, 16, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
	}

	public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref patches, out error))
		{
			return false;
		}
		PatchConfig[] array = patches;
		foreach (PatchConfig patch in array)
		{
			if (HasAction(patch, PatchType.Include))
			{
				error = GetNounPhraseError("using action Include");
				return false;
			}
		}
		return true;
	}
}
