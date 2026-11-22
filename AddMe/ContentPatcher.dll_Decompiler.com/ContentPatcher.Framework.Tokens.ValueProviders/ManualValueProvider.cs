using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class ManualValueProvider : BaseValueProvider
{
	private readonly bool IsBounded;

	private readonly MutableInvariantSet TokensUsed = new MutableInvariantSet();

	private MutableInvariantSet? AllowedRootValues = new MutableInvariantSet();

	private IInvariantSet Values = InvariantSets.Empty;

	public ManualValueProvider(string name, bool isBounded)
		: base(name, mayReturnMultipleValuesForRoot: false)
	{
		IsBounded = isBounded;
		SetReady(ready: false);
	}

	public void AddAllowedValues(ITokenString possibleValues)
	{
		if (possibleValues.HasAnyTokens || possibleValues.IsMutable || AllowedRootValues == null)
		{
			AllowedRootValues = null;
			MayReturnMultipleValuesForRoot = true;
			return;
		}
		IInvariantSet invariantSet = possibleValues.SplitValuesUnique();
		CollectionExtensions.AddRange<string>((ISet<string>)AllowedRootValues, (IEnumerable<string>)possibleValues.SplitValuesUnique());
		if (invariantSet.Count > 1)
		{
			MayReturnMultipleValuesForRoot = true;
		}
	}

	public void AddTokensUsed(IEnumerable<string> tokens)
	{
		CollectionExtensions.AddRange<string>((ISet<string>)TokensUsed, tokens);
	}

	public void SetValue(ITokenString? values)
	{
		Values = ((values?.IsReady ?? false) ? values.SplitValuesUnique() : InvariantSet.Empty);
	}

	public void SetValue(IInvariantSet values)
	{
		Values = values ?? throw new ArgumentNullException("values");
	}

	public void SetReady(bool ready)
	{
		MarkReady(ready);
	}

	public override IInvariantSet GetTokensUsed()
	{
		return TokensUsed.GetImmutable();
	}

	public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		allowedValues = ((!IsBounded) ? null : AllowedRootValues?.GetImmutable());
		return allowedValues != null;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return Values;
	}
}
