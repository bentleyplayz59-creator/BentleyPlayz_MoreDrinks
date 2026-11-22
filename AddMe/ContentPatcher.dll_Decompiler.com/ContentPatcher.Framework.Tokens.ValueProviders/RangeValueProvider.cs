using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class RangeValueProvider : BaseValueProvider
{
	private const int MaxCount = 5000;

	public RangeValueProvider()
		: base(ConditionType.Range, mayReturnMultipleValuesForRoot: true, isDeterministicForInput: true)
	{
		EnableInputArguments(required: true, mayReturnMultipleValues: true, 2);
		ValidNamedArguments = InvariantSets.FromValue("step");
		MarkReady(ready: true);
	}

	public override bool UpdateContext(IContext context)
	{
		return false;
	}

	public override bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryValidateInput(input, out error))
		{
			return false;
		}
		int min;
		int max;
		int step;
		if (input.IsReady)
		{
			return base.TryValidateInput(input, out error) && TryParseRange(input, out min, out max, out step, out error);
		}
		return true;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		if (TryParseRange(input, out int min, out int max, out int step, out string _))
		{
			int num = (max - min) / step + 1;
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (min + step * i).ToString();
			}
			return array;
		}
		return InvariantSets.Empty;
	}

	private bool TryParseRange(IInputArguments input, out int min, out int max, out int step, [NotNullWhen(false)] out string? error)
	{
		min = 0;
		max = 0;
		step = 1;
		if (!input.HasPositionalArgs)
		{
			return ParseError(input, "token " + Name + " requires input arguments", out error);
		}
		if (input.PositionalArgs.Length != 2)
		{
			return ParseError(input, "must specify a minimum and maximum value like {{" + Name + ":0,20}}", out error);
		}
		if (!int.TryParse(input.PositionalArgs[0], out min))
		{
			return ParseError(input, "can't parse min value '" + input.PositionalArgs[0] + "' as an integer", out error);
		}
		if (!int.TryParse(input.PositionalArgs[1], out max))
		{
			return ParseError(input, "can't parse max value '" + input.PositionalArgs[1] + "' as an integer", out error);
		}
		if (input.NamedArgs.TryGetValue("step", out IInputArgumentValue value) && !int.TryParse(value.Raw, out step))
		{
			return ParseError(input, "can't parse step argument '" + value.Raw + "' as an integer", out error);
		}
		if (step <= 0)
		{
			return ParseError(input, "step argument must be a positive integer greater than 0", out error);
		}
		if (min > max)
		{
			return ParseError(input, $"min value '{min}' can't be greater than max value '{max}'", out error);
		}
		int num = (max - min) / step + 1;
		if (num > 5000)
		{
			return ParseError(input, $"range can't exceed {5000} numbers (specified range would contain {num} numbers)", out error);
		}
		error = null;
		return true;
	}

	private bool ParseError(IInputArguments input, string message, out string error)
	{
		error = $"invalid input('{input.TokenString}'), {message}.";
		return false;
	}
}
