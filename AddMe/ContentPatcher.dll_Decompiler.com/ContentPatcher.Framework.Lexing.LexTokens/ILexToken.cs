namespace ContentPatcher.Framework.Lexing.LexTokens;

internal interface ILexToken
{
	LexTokenType Type { get; }

	new string ToString();
}
