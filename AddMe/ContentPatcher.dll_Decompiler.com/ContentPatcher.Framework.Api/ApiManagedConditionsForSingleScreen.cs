using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Commands;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Api;

internal class ApiManagedConditionsForSingleScreen : IManagedConditions
{
	private readonly Condition[] Conditions;

	private readonly IContext Context;

	private readonly AggregateContextual Contextuals;

	private int LastUpdateTick = -1;

	[MemberNotNullWhen(false, "ValidationError")]
	public bool IsValid
	{
		[MemberNotNullWhen(false, "ValidationError")]
		get;
	}

	public string? ValidationError { get; }

	public bool IsReady => Contextuals.IsReady;

	public bool IsMatch { get; private set; }

	public bool IsMutable => Contextuals.IsMutable;

	public ApiManagedConditionsForSingleScreen(Condition[] conditions, IContext context, bool isValid = true, string? validationError = null)
	{
		Conditions = conditions;
		Context = context;
		IsValid = isValid;
		ValidationError = validationError;
		Contextuals = new AggregateContextual().Add(conditions);
	}

	public IEnumerable<int> UpdateContext()
	{
		if (!ShouldUpdate())
		{
			return Array.Empty<int>();
		}
		LastUpdateTick = Context.UpdateTick;
		bool isMatch = IsMatch;
		if (IsValid)
		{
			Contextuals.UpdateContext(Context);
			IsMatch = IsReady && Conditions.All((Condition p) => p.IsMatch);
		}
		return (IsMatch == isMatch) ? Array.Empty<int>() : new int[1] { Context.ScreenId };
	}

	public string? GetReasonNotMatched()
	{
		if (IsMatch)
		{
			return null;
		}
		PatchBaseInfo patchBaseInfo = new PatchBaseInfo(Conditions, IsMatch, Contextuals.GetDiagnosticState());
		return patchBaseInfo.GetReasonNotLoaded();
	}

	private bool ShouldUpdate()
	{
		if (!IsMutable)
		{
			return !Contextuals.WasEverUpdated;
		}
		return LastUpdateTick < Context.UpdateTick;
	}
}
