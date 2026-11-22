using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions;

internal class LiteralString : IManagedTokenString, IContextual, IContextualInfo, ITokenString
{
	public string? Raw { get; }

	public IEnumerable<ILexToken> LexTokens { get; }

	public bool HasAnyTokens { get; } = false;

	public bool IsMutable { get; } = false;

	public bool IsSingleTokenOnly { get; } = false;

	public string Value { get; }

	public bool IsReady { get; } = true;

	public string Path { get; }

	public LiteralString(string? value, LogPathBuilder path)
	{
		Raw = value;
		Value = value ?? string.Empty;
		LexTokens = new _003C_003Ez__ReadOnlySingleElementList<ILexToken>(new LexTokenLiteral(value));
		Path = path.ToString();
	}

	public bool UpdateContext(IContext context)
	{
		return false;
	}

	public IInvariantSet GetTokensUsed()
	{
		return InvariantSets.Empty;
	}

	public IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive)
	{
		return Array.Empty<LexTokenToken>();
	}

	public bool UsesToken(ConditionType token)
	{
		return false;
	}

	public bool UsesTokens(IEnumerable<ConditionType> tokens)
	{
		return false;
	}

	public bool ShouldUpdate()
	{
		return false;
	}

	public IContextualState GetDiagnosticState()
	{
		return new ContextualState();
	}

	public override string ToString()
	{
		return Value;
	}
}
