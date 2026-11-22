using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_2_5 : BaseMigration
{
	public Migration_2_5()
		: base((ISemanticVersion)new SemanticVersion(2, 5, 0, (string)null, (string)null))
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
			InvariantDictionary<string?>? localTokens = patchConfig.LocalTokens;
			if (localTokens != null && localTokens.Count > 0)
			{
				error = GetNounPhraseError("using LocalTokens");
				return false;
			}
		}
		return true;
	}
}
