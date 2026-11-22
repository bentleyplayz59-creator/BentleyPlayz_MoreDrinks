using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_28 : BaseMigration
{
	public Migration_1_28()
		: base((ISemanticVersion)new SemanticVersion(1, 28, 0, (string)null, (string)null))
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
		foreach (PatchConfig patchConfig in array)
		{
			if (!HasAction(patchConfig, PatchType.EditData))
			{
				continue;
			}
			foreach (TextOperationConfig textOperation in patchConfig.TextOperations)
			{
				TextOperationType? textOperationType = GetEnum<TextOperationType>(textOperation?.Operation);
				if (textOperationType.HasValue && textOperationType == TextOperationType.RemoveDelimited)
				{
					error = GetNounPhraseError($"using {"TextOperations"} of type {textOperationType.Value}");
					return false;
				}
			}
		}
		return true;
	}
}
