using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Api;

internal class ApiManagedTokenString : ContentPatcher.IManagedTokenString
{
	private readonly PerScreen<ContentPatcher.IManagedTokenString> ManagedValues;

	[MemberNotNullWhen(false, "ValidationError")]
	public bool IsValid
	{
		[MemberNotNullWhen(false, "ValidationError")]
		get
		{
			return ManagedValues.Value.IsValid;
		}
	}

	public string? ValidationError => ManagedValues.Value.ValidationError;

	public bool IsReady => ManagedValues.Value.IsReady;

	public string? Value => ManagedValues.Value.Value;

	public ApiManagedTokenString(Func<ContentPatcher.IManagedTokenString> parse)
	{
		ManagedValues = new PerScreen<ContentPatcher.IManagedTokenString>(parse);
	}

	public IEnumerable<int> UpdateContext()
	{
		return new HashSet<int>((from p in ManagedValues.GetActiveValues()
			select p.Value).SelectMany((ContentPatcher.IManagedTokenString p) => p.UpdateContext()));
	}
}
