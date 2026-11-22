using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class ConditionTypeValueProvider : BaseValueProvider
{
	private readonly IInvariantSet? AllowedRootValues;

	private readonly Func<IInvariantSet> FetchValues;

	private readonly Func<bool>? IsValidInContextImpl;

	private IInvariantSet Values = InvariantSets.Empty;

	public ConditionTypeValueProvider(ConditionType type, Func<IEnumerable<string>> values, Func<bool>? isValidInContext = null, bool mayReturnMultipleValues = false, IEnumerable<string>? allowedValues = null)
		: base(type, mayReturnMultipleValues)
	{
		IsValidInContextImpl = isValidInContext;
		AllowedRootValues = ((allowedValues != null) ? InvariantSets.From(allowedValues) : null);
		FetchValues = () => InvariantSets.From(values());
	}

	public ConditionTypeValueProvider(ConditionType type, Func<string?> value, Func<bool>? isValidInContext = null, bool mayReturnMultipleValues = false, IEnumerable<string>? allowedValues = null)
		: this(type, () => BaseValueProvider.WrapOptionalValue(value()), isValidInContext, mayReturnMultipleValues, allowedValues)
	{
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(Values, () => Values = (MarkReady(IsValidInContextImpl == null || IsValidInContextImpl()) ? InvariantSets.From(FetchValues()) : InvariantSets.Empty));
	}

	public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		allowedValues = AllowedRootValues;
		return allowedValues != null;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return Values;
	}
}
