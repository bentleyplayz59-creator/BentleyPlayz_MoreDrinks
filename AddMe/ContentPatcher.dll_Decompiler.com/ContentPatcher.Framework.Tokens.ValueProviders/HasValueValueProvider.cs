using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class HasValueValueProvider : BaseValueProvider
{
	public HasValueValueProvider()
		: base(ConditionType.HasValue, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		EnableInputArguments(required: false, mayReturnMultipleValues: false, null);
	}

	public override bool UpdateContext(IContext context)
	{
		return false;
	}

	public override bool HasBoundedValues(IInputArguments input, out IInvariantSet allowedValues)
	{
		allowedValues = InvariantSets.Boolean;
		return true;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return InvariantSets.FromValue(input.HasPositionalArgs);
	}
}
