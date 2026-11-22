using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_21 : BaseMigration
{
	private readonly Lexer Lexer = ContentPatcher.Framework.Lexing.Lexer.Instance;

	private readonly HashSet<string> IgnoreRenderStrings = new HashSet<string>();

	public Migration_1_21()
		: base((ISemanticVersion)new SemanticVersion(1, 21, 0, (string)null, (string)null))
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected Obj, but got Unknown
	}

	public override bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrateMainContent(content, out error))
		{
			return false;
		}
		if (content.CustomLocations.Any())
		{
			error = GetNounPhraseError("using the CustomLocations field");
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
			if (patchConfig.AddWarps.Any())
			{
				error = GetNounPhraseError("using the AddWarps field");
				return false;
			}
			if (!string.IsNullOrWhiteSpace(patchConfig.Enabled))
			{
				ILexToken[] array2 = Lexer.ParseBits(patchConfig.Enabled, impliedBraces: false, trim: true).Take(2).ToArray();
				if (array2.Length == 1 && array2[0].Type == LexTokenType.Token)
				{
					string text = NormalizeLexicalString($"{ConditionType.Render}:{array2[0].ToString()}", impliedBraces: true);
					IgnoreRenderStrings.Add(text);
					patchConfig.When[text] = "true";
					patchConfig.Enabled = null;
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
		if (IsCustomRender(lexToken))
		{
			error = GetNounPhraseError($"using token {ConditionType.Render}");
			return false;
		}
		return true;
	}

	private bool IsCustomRender(ILexToken lexToken)
	{
		return lexToken is LexTokenToken lexTokenToken && StringExtensions.EqualsIgnoreCase(lexTokenToken.Name, "Render") && !IgnoreRenderStrings.Contains(lexToken.ToString());
	}

	private string NormalizeLexicalString(string str, bool impliedBraces)
	{
		IEnumerable<ILexToken> source = Lexer.ParseBits(str, impliedBraces);
		return string.Join("", source.Select((ILexToken p) => p.ToString()));
	}
}
