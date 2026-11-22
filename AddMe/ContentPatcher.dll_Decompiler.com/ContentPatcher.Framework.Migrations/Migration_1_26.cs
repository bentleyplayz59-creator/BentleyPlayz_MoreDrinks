using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_26 : BaseMigration
{
	public Migration_1_26()
		: base((ISemanticVersion)new SemanticVersion(1, 26, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
	}

	public override bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrateMainContent(content, out error))
		{
			return false;
		}
		foreach (ConfigSchemaFieldConfig value in content.ConfigSchema.Values)
		{
			if (!string.IsNullOrWhiteSpace(value?.Section))
			{
				error = GetNounPhraseError("using Section with a ConfigSchema entry");
				return false;
			}
		}
		return true;
	}
}
