using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Tokens;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Api;

internal class ApiManagedTokenStringForSingleScreen : ContentPatcher.IManagedTokenString
{
	private readonly IManagedTokenString TokenString;

	private readonly IContext Context;

	private int LastUpdateTick = -1;

	[MemberNotNullWhen(false, "ValidationError")]
	public bool IsValid
	{
		[MemberNotNullWhen(false, "ValidationError")]
		get;
	}

	public string? ValidationError { get; }

	public bool IsReady => TokenString.IsReady;

	public string? Value { get; private set; }

	public ApiManagedTokenStringForSingleScreen(IManagedTokenString tokenString, IContext context, bool isValid = true, string? validationError = null)
	{
		TokenString = tokenString;
		Context = context;
		IsValid = isValid;
		ValidationError = validationError;
	}

	public IEnumerable<int> UpdateContext()
	{
		if (!ShouldUpdate())
		{
			return Array.Empty<int>();
		}
		LastUpdateTick = Context.UpdateTick;
		string value = Value;
		if (IsValid)
		{
			TokenString.UpdateContext(Context);
			Value = (IsReady ? TokenString.Value : null);
		}
		return (!(Value != value)) ? Array.Empty<int>() : new int[1] { Context.ScreenId };
	}

	private bool ShouldUpdate()
	{
		return TokenString.ShouldUpdate() && LastUpdateTick < Context.UpdateTick;
	}
}
