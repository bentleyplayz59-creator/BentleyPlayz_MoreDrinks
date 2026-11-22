using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_23 : BaseMigration
{
	private static readonly Regex ValueAtPattern = new Regex("\\|\\s*valueAt\\s*=", RegexOptions.Compiled);

	public Migration_1_23()
		: base((ISemanticVersion)new SemanticVersion(1, 23, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("ChildGenders", "ChildNames", "Count", "HasCaughtFish");
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
				if (textOperation != null && textOperation.Target.Any() && GetEnum<TextOperationTargetRoot>(textOperation.Target[0]) == TextOperationTargetRoot.Fields)
				{
					error = GetNounPhraseError("using TextOperations with the Fields target");
					return false;
				}
			}
		}
		return true;
	}

	public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref lexToken, out error))
		{
			return false;
		}
		if (lexToken is LexTokenToken lexTokenToken && lexTokenToken.HasInputArgs())
		{
			string text = lexTokenToken.InputArgs.ToString();
			if (StringExtensions.ContainsIgnoreCase(text, "valueAt") && ValueAtPattern.IsMatch(text))
			{
				error = GetNounPhraseError("using the 'valueAt' argument");
				return false;
			}
		}
		return true;
	}
}
