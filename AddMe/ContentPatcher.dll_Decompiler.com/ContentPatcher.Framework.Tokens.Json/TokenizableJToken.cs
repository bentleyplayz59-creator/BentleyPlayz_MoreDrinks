using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.Json;

internal class TokenizableJToken : IContextual, IContextualInfo
{
	private readonly AggregateContextual Contextuals = new AggregateContextual();

	public JToken Value { get; }

	public bool IsMutable => Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady;

	[MemberNotNullWhen(true, "Value")]
	public bool IsString
	{
		[MemberNotNullWhen(true, "Value")]
		get
		{
			return Value is JValue;
		}
	}

	public TokenizableJToken(JToken value, IEnumerable<TokenizableProxy> proxyFields)
	{
		Value = value;
		Contextuals.Add(proxyFields);
	}

	public bool UpdateContext(IContext context)
	{
		return Contextuals.UpdateContext(context);
	}

	public IInvariantSet GetTokensUsed()
	{
		return Contextuals.GetTokensUsed();
	}

	public IContextualState GetDiagnosticState()
	{
		return Contextuals.GetDiagnosticState();
	}

	public IEnumerable<IManagedTokenString> GetTokenStrings()
	{
		foreach (IContextual contextual in Contextuals.Values)
		{
			if (contextual is IManagedTokenString tokenStr)
			{
				yield return tokenStr;
			}
			if (contextual is TokenizableProxy proxy)
			{
				yield return proxy.TokenString;
			}
		}
	}
}
