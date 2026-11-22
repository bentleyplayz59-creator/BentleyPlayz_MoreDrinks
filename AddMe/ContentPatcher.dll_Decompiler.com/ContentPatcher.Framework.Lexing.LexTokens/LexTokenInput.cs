using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ContentPatcher.Framework.Lexing.LexTokens;

internal class LexTokenInput : ILexToken
{
	public LexTokenType Type => LexTokenType.TokenInput;

	public ILexToken[] Parts { get; private set; }

	public LexTokenInput(ILexToken[] tokenParts)
	{
		MigrateTo(tokenParts);
	}

	[MemberNotNull("Parts")]
	public void MigrateTo(ILexToken[] tokenParts)
	{
		Parts = tokenParts;
	}

	public override string ToString()
	{
		return string.Join("", Parts.Select((ILexToken p) => p.ToString()));
	}
}
