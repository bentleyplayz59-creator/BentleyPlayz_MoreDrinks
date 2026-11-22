using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class RoundValueProvider : BaseValueProvider
{
	private enum RoundMode
	{
		Default,
		Up,
		Down
	}

	public RoundValueProvider()
		: base(ConditionType.Round, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		EnableInputArguments(required: true, mayReturnMultipleValues: false, null);
	}

	public override bool UpdateContext(IContext context)
	{
		bool result = !IsReady;
		MarkReady(ready: true);
		return result;
	}

	public override bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryValidateInput(input, out error))
		{
			return false;
		}
		decimal value;
		int decimals;
		RoundMode mode;
		if (input.IsReady)
		{
			return base.TryValidateInput(input, out error) && TryParse(input, out value, out decimals, out mode, out error);
		}
		return true;
	}

	public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		if (TryParseAndRound(input, out decimal result, out string _))
		{
			allowedValues = InvariantSets.FromValue(result.ToString(CultureInfo.InvariantCulture));
			return true;
		}
		allowedValues = null;
		return false;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		if (!TryParseAndRound(input, out decimal result, out string parseError))
		{
			throw new InvalidOperationException($"Invalid input value '{input.TokenString}': {parseError}");
		}
		return InvariantSets.FromValue(result.ToString(CultureInfo.InvariantCulture));
	}

	private bool TryParseAndRound(IInputArguments input, out decimal result, [NotNullWhen(false)] out string? parseError)
	{
		if (!TryParse(input, out var value, out var decimals, out var mode, out parseError))
		{
			result = 0m;
			return false;
		}
		result = Round(value, decimals, mode);
		return true;
	}

	private decimal Round(decimal value, int decimals, RoundMode mode)
	{
		switch (mode)
		{
		case RoundMode.Default:
			return Math.Round(value, decimals);
		case RoundMode.Up:
		case RoundMode.Down:
		{
			decimal num = (decimal)Math.Pow(10.0, decimals);
			return (mode == RoundMode.Down) ? (Math.Floor(value * num) / num) : (Math.Ceiling(value * num) / num);
		}
		default:
			throw new NotSupportedException($"Unknown round mode '{mode}'.");
		}
	}

	private bool TryParse(IInputArguments input, out decimal value, out int decimals, out RoundMode mode, [NotNullWhen(false)] out string? parseError)
	{
		value = 0m;
		decimals = 0;
		mode = RoundMode.Default;
		ITokenString? tokenString = input.TokenString;
		if (tokenString == null || !tokenString.IsReady)
		{
			parseError = "input isn't ready";
			return false;
		}
		string[] positionalArgs = input.PositionalArgs;
		int num = positionalArgs.Length;
		if ((num < 1 || num > 3) ? true : false)
		{
			parseError = $"input '{input.TokenString}' must have 1-3 comma-separated arguments.";
			return false;
		}
		if (!decimal.TryParse(positionalArgs[0], out value))
		{
			parseError = "value '" + positionalArgs[0] + "' can't be parsed as a numeric value.";
			return false;
		}
		if (positionalArgs.Length >= 2)
		{
			if (!int.TryParse(positionalArgs[1], out decimals))
			{
				parseError = "digit count '" + positionalArgs[1] + "' can't be parsed as an integer value.";
				return false;
			}
			if (decimals < 0)
			{
				parseError = "digit count '" + positionalArgs[1] + "' can't be less than zero.";
				return false;
			}
		}
		if (positionalArgs.Length >= 3 && !Enum.TryParse<RoundMode>(positionalArgs[2], ignoreCase: true, out mode))
		{
			parseError = $"round mode '{positionalArgs[2]}' is invalid; must be one of '{string.Join(", ", Enum.GetNames(typeof(RoundMode)))}'.";
			return false;
		}
		parseError = null;
		return true;
	}
}
