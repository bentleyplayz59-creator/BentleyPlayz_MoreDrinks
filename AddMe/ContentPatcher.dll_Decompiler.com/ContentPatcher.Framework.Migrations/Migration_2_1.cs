using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_2_1 : BaseMigration
{
	public Migration_2_1()
		: base((ISemanticVersion)new SemanticVersion(2, 1, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
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
			if (patchConfig.TargetLocale != null)
			{
				error = GetNounPhraseError("using TargetLocale");
				return false;
			}
		}
		return true;
	}
}
