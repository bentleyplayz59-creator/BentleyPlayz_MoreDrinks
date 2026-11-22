using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_20 : BaseMigration
{
	public Migration_1_20()
		: base((ISemanticVersion)new SemanticVersion(1, 20, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("LocationContext", "LocationUniqueName");
	}

	public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref lexToken, out error))
		{
			return false;
		}
		string text = "Weather";
		if (lexToken is LexTokenToken lexTokenToken && StringExtensions.EqualsIgnoreCase(lexTokenToken.Name, text))
		{
			if (lexTokenToken.HasInputArgs())
			{
				InputArguments inputArguments = new InputArguments(new LiteralString(lexTokenToken.InputArgs.ToString(), new LogPathBuilder()));
				if (inputArguments.HasPositionalArgs)
				{
					error = GetNounPhraseError("using " + text + " token with an input argument");
					return false;
				}
			}
			ILexToken[] array = new ILexToken[1]
			{
				new LexTokenLiteral("Default")
			};
			ILexToken[] array2 = lexTokenToken.InputArgs?.Parts;
			lexToken = new LexTokenToken(lexTokenToken.Name, new LexTokenInput((array2 != null && array2.Any()) ? array.Concat(array2).ToArray() : array), impliedBraces: true);
		}
		return true;
	}
}
