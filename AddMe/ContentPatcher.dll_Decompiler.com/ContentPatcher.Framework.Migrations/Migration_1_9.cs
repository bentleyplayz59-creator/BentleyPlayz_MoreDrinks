using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Lexing.LexTokens;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_9 : BaseMigration
{
	public Migration_1_9()
		: base((ISemanticVersion)new SemanticVersion(1, 9, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
	}

	public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref lexToken, out error))
		{
			return false;
		}
		if (lexToken is LexTokenToken lexTokenToken && lexTokenToken.Name.Contains("/"))
		{
			error = GetNounPhraseError($"using custom mod-provided tokens like '{lexToken}'");
			return false;
		}
		return true;
	}
}
