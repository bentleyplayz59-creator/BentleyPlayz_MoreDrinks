using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Conditions;

internal class TokenString : IManagedTokenString, IContextual, IContextualInfo, ITokenString
{
	private class TokenStringPart
	{
		public ILexToken LexToken { get; }

		public TokenString? Input { get; }

		public IInputArguments InputArgs { get; }

		public TokenStringPart(ILexToken lexToken, TokenString? input)
		{
			LexToken = lexToken;
			Input = input;
			IInputArguments inputArguments;
			if (string.IsNullOrWhiteSpace(input?.Raw))
			{
				inputArguments = InputArguments.Empty;
			}
			else
			{
				IInputArguments inputArguments2 = new InputArguments(input);
				inputArguments = inputArguments2;
			}
			InputArgs = inputArguments;
		}
	}

	private static readonly Lexer Lexer = ContentPatcher.Framework.Lexing.Lexer.Instance;

	private readonly IInvariantSet TokensUsed = InvariantSets.Empty;

	private readonly ContextualState State = new ContextualState();

	private readonly TokenStringPart[] Parts;

	private StringBuilder? StringBuilder;

	public string Raw { get; }

	public IEnumerable<ILexToken> LexTokens => Parts.Select((TokenStringPart p) => p.LexToken);

	public bool HasAnyTokens { get; }

	public bool IsMutable { get; }

	public bool IsSingleTokenOnly { get; }

	public string? Value { get; private set; }

	public bool IsReady => State.IsReady;

	public string Path { get; }

	public TokenString(string raw, IContext context, LogPathBuilder path)
		: this(Lexer.ParseBits(raw, impliedBraces: false), context, path)
	{
	}

	public TokenString(IEnumerable<ILexToken> lexTokens, IContext context, LogPathBuilder path)
	{
		Path = path.ToString();
		Parts = (from lexToken in lexTokens
			let input = (lexToken is LexTokenToken lexTokenToken && lexTokenToken.HasInputArgs()) ? new TokenString(lexTokenToken.InputArgs.Parts, context, path.With(lexTokenToken.Name)) : null
			let token = lexToken
			select new TokenStringPart(token, input)).ToArray();
		Raw = string.Join("", Parts.Select((TokenStringPart p) => p.LexToken)).Trim();
		if (string.IsNullOrWhiteSpace(Raw))
		{
			Value = Raw;
			return;
		}
		bool flag = false;
		bool flag2 = false;
		List<string> list = null;
		foreach (LexTokenToken tokenPlaceholder in GetTokenPlaceholders(LexTokens, recursive: true))
		{
			flag2 = true;
			IToken token = context.GetToken(tokenPlaceholder.Name, enforceContext: false);
			if (token != null)
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(token.Name);
				if (!token.IsDeterministicForInput)
				{
					flag = flag || token.IsMutable;
				}
			}
			else
			{
				string providerModId = tokenPlaceholder.GetProviderModId();
				if (!string.IsNullOrWhiteSpace(providerModId) && !context.IsModInstalled(providerModId))
				{
					State.AddUnavailableModToken(providerModId);
				}
				else
				{
					State.AddInvalidToken(tokenPlaceholder.Name);
				}
				flag = true;
			}
		}
		if (list != null)
		{
			TokensUsed = InvariantSets.From(list);
		}
		IsMutable = flag;
		HasAnyTokens = flag2;
		IsSingleTokenOnly = GetIsSingleTokenOnly(Parts);
		if (State.IsReady)
		{
			UpdateContext(context, forceUpdate: true);
		}
	}

	public bool UpdateContext(IContext context)
	{
		return UpdateContext(context, forceUpdate: false);
	}

	public bool ShouldUpdate()
	{
		if (!IsMutable)
		{
			return false;
		}
		if (State.InvalidTokens.Any() || State.UnavailableModTokens.Any())
		{
			return false;
		}
		return true;
	}

	public IInvariantSet GetTokensUsed()
	{
		return TokensUsed;
	}

	public IEnumerable<LexTokenToken> GetTokenPlaceholders(bool recursive)
	{
		return GetTokenPlaceholders(LexTokens, recursive);
	}

	public bool UsesToken(ConditionType token)
	{
		return TokensUsed.Contains(token.ToString());
	}

	public bool UsesTokens(IEnumerable<ConditionType> tokens)
	{
		return tokens.Any(UsesToken);
	}

	public IContextualState GetDiagnosticState()
	{
		return State.Clone();
	}

	public override string? ToString()
	{
		return IsReady ? Value : Raw;
	}

	private bool UpdateContext(IContext context, bool forceUpdate)
	{
		if (!forceUpdate && !ShouldUpdate())
		{
			return false;
		}
		string value = Value;
		bool isReady = State.IsReady;
		State.Reset();
		TokenStringPart[] parts = Parts;
		if (parts != null && parts.Length == 1)
		{
			TokenStringPart tokenStringPart = parts[0];
			if (tokenStringPart != null && tokenStringPart.LexToken is LexTokenLiteral lexTokenLiteral)
			{
				Value = lexTokenLiteral.Text;
				goto IL_0130;
			}
		}
		StringBuilder stringBuilder = StringBuilder ?? (StringBuilder = new StringBuilder());
		TokenStringPart[] parts2 = Parts;
		foreach (TokenStringPart tokenStringPart2 in parts2)
		{
			stringBuilder.Append(TryGetTokenText(context, tokenStringPart2, State, out string text) ? text : tokenStringPart2.LexToken.ToString());
		}
		Value = (State.IsReady ? stringBuilder.ToString().Trim() : null);
		if (IsMutable)
		{
			stringBuilder.Clear();
		}
		else
		{
			StringBuilder = null;
		}
		goto IL_0130;
		IL_0130:
		if (Value == null && State.IsReady)
		{
			throw new InvalidOperationException("Could not apply tokens to string '" + Raw + "', but no invalid tokens or errors were reported.");
		}
		return Value != value || State.IsReady != isReady;
	}

	private IEnumerable<LexTokenToken> GetTokenPlaceholders(IEnumerable<ILexToken>? lexTokens, bool recursive)
	{
		IEnumerable<ILexToken>? enumerable = lexTokens;
		lexTokens = ((enumerable != null) ? enumerable.ToArray() : null);
		IEnumerable<ILexToken>? enumerable2 = lexTokens;
		if (enumerable2 == null || !enumerable2.Any())
		{
			yield break;
		}
		if (!recursive)
		{
			foreach (LexTokenToken item in lexTokens.OfType<LexTokenToken>())
			{
				yield return item;
			}
			yield break;
		}
		Stack<LexTokenToken> stack = new Stack<LexTokenToken>(lexTokens.OfType<LexTokenToken>());
		while (stack.Count > 0)
		{
			LexTokenToken token = stack.Pop();
			yield return token;
			if (!token.HasInputArgs())
			{
				continue;
			}
			foreach (LexTokenToken subToken in token.InputArgs.Parts.OfType<LexTokenToken>())
			{
				stack.Push(subToken);
			}
		}
	}

	private bool TryGetTokenText(IContext context, TokenStringPart part, ContextualState state, [NotNullWhen(true)] out string? text)
	{
		ILexToken lexToken = part.LexToken;
		ILexToken lexToken2 = lexToken;
		if (lexToken2 is LexTokenToken lexTokenToken)
		{
			IToken token = context.GetToken(lexTokenToken.Name, enforceContext: false);
			if (token == null)
			{
				state.AddInvalidToken(lexTokenToken.Name);
				text = null;
				return false;
			}
			if (!token.IsReady)
			{
				State.AddUnreadyToken(lexTokenToken.Name);
				text = null;
				return false;
			}
			if (part.Input != null)
			{
				part.Input.UpdateContext(context);
				if (!part.Input.IsReady)
				{
					state.MergeFrom(part.Input.GetDiagnosticState());
					text = null;
					return false;
				}
			}
			if (!token.TryValidateInput(part.InputArgs, out string error))
			{
				state.AddError(error);
				text = null;
				return false;
			}
			string[] value = token.GetValues(part.InputArgs).ToArray();
			text = string.Join(", ", value);
			return true;
		}
		text = part.LexToken.ToString();
		return true;
	}

	private static bool GetIsSingleTokenOnly(TokenStringPart[] parts)
	{
		bool flag = false;
		foreach (TokenStringPart tokenStringPart in parts)
		{
			if (tokenStringPart.LexToken is LexTokenLiteral lexTokenLiteral)
			{
				if (!string.IsNullOrWhiteSpace(lexTokenLiteral.Text))
				{
					return false;
				}
				continue;
			}
			if (tokenStringPart.LexToken.Type == LexTokenType.Token)
			{
				if (flag)
				{
					return false;
				}
				flag = true;
				continue;
			}
			return false;
		}
		return flag;
	}
}
