using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class LetterCaseValueProvider : BaseValueProvider
{
	private readonly ConditionType Type;

	public LetterCaseValueProvider(ConditionType type)
		: base(type, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		if (type != ConditionType.Lowercase && type != ConditionType.Uppercase)
		{
			throw new ArgumentException($"The {"type"} must be one of {ConditionType.Lowercase} or {ConditionType.Uppercase}.", "type");
		}
		Type = type;
		EnableInputArguments(required: false, mayReturnMultipleValues: false, null);
	}

	public override bool UpdateContext(IContext context)
	{
		bool result = !IsReady;
		MarkReady(ready: true);
		return result;
	}

	public override bool HasBoundedValues(IInputArguments input, out IInvariantSet allowedValues)
	{
		allowedValues = InvariantSets.From(GetValues(input));
		return true;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		ConditionType type = Type;
		if (1 == 0)
		{
		}
		string text = type switch
		{
			ConditionType.Lowercase => input.TokenString?.Value?.ToLowerInvariant(), 
			ConditionType.Uppercase => input.TokenString?.Value?.ToUpperInvariant(), 
			_ => throw new NotSupportedException($"Unimplemented letter case type '{Type}'."), 
		};
		if (1 == 0)
		{
		}
		string text2 = text;
		return InvariantSets.FromValue(text2 ?? string.Empty);
	}
}
