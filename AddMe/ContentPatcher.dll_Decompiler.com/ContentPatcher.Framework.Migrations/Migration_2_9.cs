using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_2_9 : BaseMigration
{
	public Migration_2_9()
		: base((ISemanticVersion)new SemanticVersion(2, 9, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
	}

	public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		if (!base.TryMigrate(ref patches, out error))
		{
			return false;
		}
		PatchConfig[] array = patches;
		PatchMode val = default;
		foreach (PatchConfig patchConfig in array)
		{
			if (patchConfig.PatchMode != null && Utility.TryParseEnum<PatchMode>(patchConfig.PatchMode, ref val) && (int)val == 2)
			{
				error = GetNounPhraseError("using PatchMode Mask");
				return false;
			}
		}
		return true;
	}
}
