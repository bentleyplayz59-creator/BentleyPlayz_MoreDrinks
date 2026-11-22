using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class RenderValueProvider : BaseValueProvider
{
	public RenderValueProvider()
		: base(ConditionType.Render, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		EnableInputArguments(required: false, mayReturnMultipleValues: false, null);
	}

	public override bool UpdateContext(IContext context)
	{
		bool result = !IsReady;
		MarkReady(ready: true);
		return result;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		string value = (input.HasPositionalArgs ? string.Join(", ", input.PositionalArgs) : string.Empty);
		return InvariantSets.FromValue(value);
	}
}
