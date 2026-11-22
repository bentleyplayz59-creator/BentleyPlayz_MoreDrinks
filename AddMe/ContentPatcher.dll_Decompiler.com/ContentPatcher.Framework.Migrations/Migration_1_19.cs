using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_19 : BaseMigration
{
	private readonly Lexer Lexer = ContentPatcher.Framework.Lexing.Lexer.Instance;

	public Migration_1_19()
		: base((ISemanticVersion)new SemanticVersion(1, 19, 0, (string)null, (string)null))
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("Time");
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
			if (patchConfig.PatchMode != null && HasAction(patchConfig, PatchType.EditMap))
			{
				error = GetNounPhraseError("using PatchMode for a map patch");
				return false;
			}
			if (GetEnum<UpdateRate>(patchConfig.Update) == UpdateRate.OnTimeChange)
			{
				error = GetNounPhraseError("using the OnTimeChange update rate");
				return false;
			}
			if (HasMultipleValues(patchConfig.FromFile))
			{
				error = GetNounPhraseError("specifying multiple FromFile values");
				return false;
			}
		}
		return true;
	}

	public override bool TryMigrate(Condition condition, [NotNullWhen(false)] out string? error)
	{
		string name = condition.Name;
		if (name != null && StringExtensions.EqualsIgnoreCase(name, "Query"))
		{
			IInvariantSet invariantSet = condition.Values?.SplitValuesUnique();
			if (invariantSet != null && invariantSet.Any() && invariantSet.All((string p) => bool.TryParse(p, out var _)))
			{
				error = "using boolean query expressions";
				return false;
			}
		}
		return base.TryMigrate(condition, out error);
	}

	private bool HasMultipleValues(string? raw)
	{
		if (raw == null || !raw.Contains(","))
		{
			return false;
		}
		return Lexer.SplitLexically(raw).Take(2).Count() > 1;
	}
}
