using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_11 : BaseMigration
{
	public Migration_1_11()
		: base((ISemanticVersion)new SemanticVersion(1, 11, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("Lowercase", "Uppercase");
	}

	public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref lexToken, out error))
		{
			return false;
		}
		if (lexToken is LexTokenToken lexTokenToken && lexTokenToken.Name.Equals("Random", StringComparison.OrdinalIgnoreCase) && lexTokenToken.HasInputArgs() && lexTokenToken.InputArgs.ToString().Contains("|"))
		{
			error = GetNounPhraseError("using pinned keys with the Random token");
			return false;
		}
		return true;
	}
}
