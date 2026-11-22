using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_7 : BaseMigration
{
	public Migration_1_7()
		: base((ISemanticVersion)new SemanticVersion(1, 7, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("HasReadLetter", "HasValue", "IsCommunityCenterComplete", "IsMainPlayer");
	}

	public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref lexToken, out error))
		{
			return false;
		}
		if (lexToken is LexTokenToken lexTokenToken && !StringExtensions.EqualsIgnoreCase(lexTokenToken.Name, "HasFile"))
		{
			LexTokenInput? inputArgs = lexTokenToken.InputArgs;
			if (inputArgs != null && inputArgs.Parts.Any((ILexToken p) => p.Type == LexTokenType.Token))
			{
				error = GetNounPhraseError($"using nested tokens like '{lexToken}'");
				return false;
			}
		}
		return true;
	}

	public override bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrateMainContent(content, out error))
		{
			return false;
		}
		if (content.DynamicTokens.Any((DynamicTokenConfig p) => p.Value?.Contains("{{") ?? false))
		{
			error = GetNounPhraseError("using tokens in dynamic token values");
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
			if (patchConfig.Fields.Keys.Any((string key) => key.Contains("{{")))
			{
				error = GetNounPhraseError("using tokens in field keys");
				return false;
			}
			if (patchConfig.When.Any((KeyValuePair<string, string> condition) =>
			{
				string value = condition.Value;
				return value != null && value.Contains("{{") && !StringExtensions.ContainsIgnoreCase(condition.Value, "HasFile");
			}))
			{
				error = GetNounPhraseError("using tokens in condition values");
				return false;
			}
		}
		return true;
	}
}
