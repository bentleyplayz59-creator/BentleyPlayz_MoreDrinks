using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class ImmutableValueProvider : BaseValueProvider
{
	private readonly IInvariantSet? AllowedRootValues;

	private readonly IInvariantSet Values;

	public ImmutableValueProvider(string name, IInvariantSet? values, IInvariantSet? allowedValues = null, bool? canHaveMultipleValues = null, bool isMutable = false)
		: base(name, mayReturnMultipleValuesForRoot: false)
	{
		Values = values ?? InvariantSets.Empty;
		AllowedRootValues = ((allowedValues != null && allowedValues.Any()) ? allowedValues : null);
		MayReturnMultipleValuesForRoot = canHaveMultipleValues ?? (Values.Count > 1 || AllowedRootValues == null || AllowedRootValues.Count > 1);
		IsMutable = isMutable;
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
