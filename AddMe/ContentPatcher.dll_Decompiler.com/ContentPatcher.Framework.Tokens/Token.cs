using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Tokens;

internal class Token : IToken, IContextual, IContextualInfo
{
	protected IValueProvider Values { get; }

	public string? Scope { get; }

	public string Name { get; }

	public bool IsMutable => Values.IsMutable;

	public bool IsDeterministicForInput => !Values.IsMutable || Values.IsDeterministicForInput;

	public bool IsReady => Values.IsReady;

	public bool RequiresInput => Values.RequiresPositionalInput;

	public bool BypassesContextValidation => Values.BypassesContextValidation;

	public Func<string, string>? NormalizeValue => Values.NormalizeValue;

	public Token(IValueProvider provider, string? scope = null)
	{
		Values = provider;
		Scope = scope;
		Name = provider.Name;
	}

	public virtual bool UpdateContext(IContext context)
	{
		return Values.UpdateContext(context);
	}

	public virtual IInvariantSet GetTokensUsed()
	{
		return Values.GetTokensUsed();
	}

	public virtual IContextualState GetDiagnosticState()
	{
		return Values.GetDiagnosticState();
	}

	public virtual bool CanHaveMultipleValues(IInputArguments input)
	{
		if (input.ReservedArgs.ContainsKey("contains") || input.ReservedArgs.ContainsKey("valueAt"))
		{
			return false;
		}
		return Values.CanHaveMultipleValues(input);
	}

	public virtual bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		KeyValuePair<string, IInputArgumentValue>[] reservedArgsList = input.ReservedArgsList;
		for (int i = 0; i < reservedArgsList.Length; i++)
		{
			var (text2, inputArgumentValue2) = reservedArgsList[i];
			if (StringExtensions.EqualsIgnoreCase("valueAt", text2) && !int.TryParse(inputArgumentValue2.Raw, out var _))
			{
				error = $"invalid '{"valueAt"}' index '{inputArgumentValue2.Raw}', must be a numeric value.";
				return false;
			}
		}
		return Values.TryValidateInput(input, out error);
	}

	public virtual bool TryValidateValues(IInputArguments input, IInvariantSet values, IContext context, [NotNullWhen(false)] out string? error)
	{
		if (input.ReservedArgs.ContainsKey("contains"))
		{
			string[] array = values.Where((string p) => !bool.TryParse(p, out var _)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
			if (array.Any())
			{
				error = "invalid values (" + string.Join(", ", array) + "); expected 'true' or 'false' when used with 'contains'.";
				return false;
			}
			error = null;
			return true;
		}
		if (!TryValidateInput(input, out error) || !Values.TryValidateValues(input, values, out error))
		{
			return false;
		}
		error = null;
		return true;
	}

	public virtual IInvariantSet? GetAllowedInputArguments()
	{
		return Values.GetValidPositionalArgs();
	}

	public virtual bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		if (input.ReservedArgs.ContainsKey("contains"))
		{
			allowedValues = InvariantSets.Boolean;
			return true;
		}
		return Values.HasBoundedValues(input, out allowedValues);
	}

	public virtual bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
	{
		if (input.ReservedArgs.ContainsKey("contains"))
		{
			min = -1;
			max = -1;
			return false;
		}
		return Values.HasBoundedRangeValues(input, out min, out max);
	}

	public virtual IInvariantSet GetValues(IInputArguments input)
	{
		IEnumerable<string> values = Values.GetValues(input);
		if (input.ReservedArgs.Any())
		{
			KeyValuePair<string, IInputArgumentValue>[] reservedArgsList = input.ReservedArgsList;
			for (int i = 0; i < reservedArgsList.Length; i++)
			{
				var (text2, argValue) = reservedArgsList[i];
				if (StringExtensions.EqualsIgnoreCase("contains", text2))
				{
					values = ApplyContains(values, argValue);
				}
				else if (StringExtensions.EqualsIgnoreCase("valueAt", text2))
				{
					values = ApplyValueAt(values, argValue);
				}
			}
		}
		return InvariantSets.From(values);
	}

	protected Token(string name, IValueProvider provider, string? scope = null)
		: this(provider, scope)
	{
		Name = name;
	}

	private IInvariantSet ApplyContains(IEnumerable<string> values, IInputArgumentValue argValue)
	{
		if (string.IsNullOrWhiteSpace(argValue.Raw))
		{
			return InvariantSets.False;
		}
		string[] parsed = argValue.Parsed;
		if (NormalizeValue != null)
		{
			for (int i = 0; i < parsed.Length; i++)
			{
				parsed[i] = NormalizeValue(parsed[i]);
			}
		}
		bool value = ((values is IInvariantSet invariantSet) ? invariantSet.Overlaps(parsed) : InvariantSets.From(parsed).Overlaps(values));
		return InvariantSets.FromValue(value);
	}

	private IInvariantSet ApplyValueAt(IEnumerable<string> values, IInputArgumentValue argValue)
	{
		if (!int.TryParse(argValue.Raw, out var result))
		{
			throw new FormatException($"Invalid '{"valueAt"}' index '{argValue.Raw}', must be a numeric index.");
		}
		IList<string> list = values as IList<string>;
		if (list == null)
		{
			list = values.ToArray();
		}
		if (Math.Abs(result) >= list.Count)
		{
			return InvariantSets.Empty;
		}
		return InvariantSets.FromValue((result >= 0) ? list[result] : list[list.Count + result]);
	}
}
