using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_13 : BaseMigration
{
	public Migration_1_13()
		: base((ISemanticVersion)new SemanticVersion(1, 13, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("Query");
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
			if (patchConfig.MapTiles.Any())
			{
				error = GetNounPhraseError("using MapTiles");
				return false;
			}
		}
		return true;
	}
}
