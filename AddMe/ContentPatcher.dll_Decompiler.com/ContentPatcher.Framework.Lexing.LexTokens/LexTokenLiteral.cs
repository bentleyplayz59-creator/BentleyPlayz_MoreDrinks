using System.Diagnostics.CodeAnalysis;

namespace ContentPatcher.Framework.Lexing.LexTokens;

internal class LexTokenLiteral : ILexToken
{
	public LexTokenType Type => LexTokenType.Literal;

	public string Text { get; private set; }

	public LexTokenLiteral(string? text)
	{
		MigrateTo(text);
	}

	[MemberNotNull("Text")]
	public void MigrateTo(string? text)
	{
		Text = text ?? string.Empty;
	}

	public override string ToString()
	{
		return Text;
	}
}
