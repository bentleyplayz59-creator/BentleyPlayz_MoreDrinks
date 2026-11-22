using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Api;

internal class ApiManagedConditions : IManagedConditions
{
	private readonly PerScreen<IManagedConditions> Conditions;

	[MemberNotNullWhen(false, "ValidationError")]
	public bool IsValid
	{
		[MemberNotNullWhen(false, "ValidationError")]
		get
		{
			return Conditions.Value.IsValid;
		}
	}

	public string? ValidationError => Conditions.Value.ValidationError;

	public bool IsReady => Conditions.Value.IsReady;

	public bool IsMatch => Conditions.Value.IsMatch;

	public bool IsMutable => Conditions.Value.IsMutable;

	public ApiManagedConditions(Func<IManagedConditions> parse)
	{
		Conditions = new PerScreen<IManagedConditions>(parse);
	}

	public IEnumerable<int> UpdateContext()
	{
		return new HashSet<int>((from p in Conditions.GetActiveValues()
			select p.Value).SelectMany((IManagedConditions p) => p.UpdateContext()));
	}

	public string? GetReasonNotMatched()
	{
		return Conditions.Value.GetReasonNotMatched();
	}
}
