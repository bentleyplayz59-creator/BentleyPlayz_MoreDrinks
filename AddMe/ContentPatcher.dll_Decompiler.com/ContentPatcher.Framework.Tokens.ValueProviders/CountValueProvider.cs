using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class CountValueProvider : BaseValueProvider
{
	public CountValueProvider()
		: base(ConditionType.Count, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		EnableInputArguments(required: false, mayReturnMultipleValues: false, null);
	}

	public override bool UpdateContext(IContext context)
	{
		bool result = !IsReady;
		MarkReady(ready: true);
		return result;
	}

	public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
	{
		min = 0;
		max = int.MaxValue;
		return true;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return InvariantSets.FromValue(input.PositionalArgs.Length);
	}
}
