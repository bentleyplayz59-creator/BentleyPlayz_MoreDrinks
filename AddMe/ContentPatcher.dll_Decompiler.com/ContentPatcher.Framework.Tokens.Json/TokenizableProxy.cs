using System;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.Json;

internal class TokenizableProxy : IContextual, IContextualInfo
{
	public IManagedTokenString TokenString { get; }

	public Action<string> SetValue { get; }

	public bool IsMutable => TokenString.IsMutable;

	public bool IsReady => TokenString.IsReady;

	public TokenizableProxy(IManagedTokenString tokenString, Action<string> setValue)
	{
		TokenString = tokenString;
		SetValue = setValue;
	}

	public bool UpdateContext(IContext context)
	{
		bool result = TokenString.UpdateContext(context);
		if (IsReady)
		{
			SetValue(TokenString.Value);
		}
		return result;
	}

	public IInvariantSet GetTokensUsed()
	{
		return TokenString.GetTokensUsed();
	}

	public IContextualState GetDiagnosticState()
	{
		return TokenString.GetDiagnosticState();
	}
}
