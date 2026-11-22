using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_3 : BaseMigration
{
	public Migration_1_3()
		: base((ISemanticVersion)new SemanticVersion(1, 3, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
	}

	public override bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrateMainContent(content, out error))
		{
			return false;
		}
		if (content.ConfigSchema.Any())
		{
			error = GetNounPhraseError("using the ConfigSchema field");
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
			if (patchConfig.FromFile != null && patchConfig.FromFile.Contains("{{"))
			{
				error = GetNounPhraseError("using the {{token}} feature in FromFile fields");
				return false;
			}
			if (patchConfig.When.Any())
			{
				error = GetNounPhraseError("using the condition feature (Changes.When field)");
				return false;
			}
		}
		return true;
	}
}
