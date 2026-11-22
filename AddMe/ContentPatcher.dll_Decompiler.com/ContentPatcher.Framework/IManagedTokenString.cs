using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;

namespace ContentPatcher.Framework;

internal interface IManagedTokenString : IContextual, IContextualInfo, ITokenString
{
	IEnumerable<ILexToken> LexTokens { get; }

	IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive);

	bool UsesToken(ConditionType token);

	bool UsesTokens(IEnumerable<ConditionType> tokens);

	bool ShouldUpdate();
}
