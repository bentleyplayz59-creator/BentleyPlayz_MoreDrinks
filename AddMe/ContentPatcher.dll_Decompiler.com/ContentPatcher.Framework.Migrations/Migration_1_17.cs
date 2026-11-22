using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_17 : BaseMigration
{
	private static readonly Regex LocationTokenPattern = new Regex("\\b(?:IsOutdoors|LocationName)\\b", RegexOptions.Compiled);

	public Migration_1_17()
		: base((ISemanticVersion)new SemanticVersion(1, 17, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("TargetPathOnly");
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
			if (patchConfig.Update != null)
			{
				error = GetNounPhraseError("specifying the patch update rate ('Update' field)");
				return false;
			}
			InvariantDictionary<string?> when = patchConfig.When;
			if (when != null && when.Keys.Any((string key) => !string.IsNullOrWhiteSpace(key) && (StringExtensions.ContainsIgnoreCase(key, "IsOutdoors") || StringExtensions.ContainsIgnoreCase(key, "LocationName")) && LocationTokenPattern.IsMatch(key)))
			{
				patchConfig.Update = UpdateRate.OnLocationChange.ToString();
			}
		}
		return true;
	}
}
