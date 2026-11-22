using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal abstract class BaseValueProvider : IValueProvider, IContextual, IContextualInfo
{
	private readonly ContextualState State = new ContextualState();

	private int? MaxPositionalArgs;

	protected bool MayReturnMultipleValuesForRoot { get; set; }

	protected bool MayReturnMultipleValuesForInput { get; set; }

	protected IInvariantSet ValidNamedArguments { get; set; } = InvariantSets.Empty;

	protected bool AllowAnyNamedArguments { get; set; }

	public string Name { get; }

	public bool IsMutable { get; protected set; } = true;

	public bool IsDeterministicForInput { get; protected set; }

	public bool IsReady => State.IsReady;

	public bool AllowsPositionalInput { get; private set; }

	public bool RequiresPositionalInput { get; private set; }

	public bool BypassesContextValidation { get; protected set; } = false;

	public Func<string, string>? NormalizeValue { get; protected set; }

	public virtual bool UpdateContext(IContext context)
	{
		return false;
	}

	public virtual IInvariantSet GetTokensUsed()
	{
		return InvariantSets.Empty;
	}

	public IContextualState GetDiagnosticState()
	{
		return State.Clone();
	}

	public bool CanHaveMultipleValues(IInputArguments input)
	{
		return input.HasPositionalArgs ? MayReturnMultipleValuesForInput : MayReturnMultipleValuesForRoot;
	}

	public virtual bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		if (input.IsReady)
		{
			if (input.HasPositionalArgs)
			{
				if (!AllowsPositionalInput)
				{
					error = $"invalid input arguments ({input.TokenString}), token {Name} doesn't allow input.";
					return false;
				}
				if (input.PositionalArgs.Length > MaxPositionalArgs)
				{
					error = $"invalid input arguments ({input.TokenString}), token {Name} doesn't allow more than {MaxPositionalArgs} argument{((MaxPositionalArgs == 1) ? "" : "s")}.";
					return false;
				}
				if (input.TokenString.Value != "$~placeholder")
				{
					IInvariantSet validInputs = GetValidPositionalArgs();
					IInvariantSet invariantSet = validInputs;
					if (invariantSet != null && invariantSet.Any() && input.PositionalArgs.Any((string arg) => !validInputs.Contains(arg)))
					{
						string text = input.TokenString.Raw ?? string.Empty;
						string text2 = input.TokenString.Value ?? string.Empty;
						error = $"invalid input arguments ({((text != text2) ? (text + " => " + text2) : text2)}) for {Name} token, expected any of '{string.Join("', '", validInputs.OrderByHuman())}'";
						return false;
					}
				}
			}
			if (!AllowAnyNamedArguments && input.HasNamedArgs)
			{
				if (!ValidNamedArguments.Any())
				{
					error = $"invalid named argument '{input.NamedArgs.First().Key}' for {Name} token, which does not accept any named arguments.";
					return false;
				}
				string text3 = input.NamedArgs.Where((KeyValuePair<string, IInputArgumentValue> arg) =>
				{
					IInvariantSet validNamedArguments = ValidNamedArguments;
					KeyValuePair<string, IInputArgumentValue> keyValuePair = arg;
					return !validNamedArguments.Contains(keyValuePair.Key);
				}).Select((KeyValuePair<string, IInputArgumentValue> arg) =>
				{
					KeyValuePair<string, IInputArgumentValue> keyValuePair = arg;
					return keyValuePair.Key;
				}).FirstOrDefault();
				if (text3 != null)
				{
					error = $"invalid named argument '{text3}' for {Name} token, expected any of '{string.Join("', '", ValidNamedArguments.OrderByHuman())}'";
					return false;
				}
			}
		}
		error = null;
		return true;
	}

	public virtual bool TryValidateValues(IInputArguments input, IInvariantSet values, [NotNullWhen(false)] out string? error)
	{
		if (!TryValidateInput(input, out error))
		{
			return false;
		}
		IInvariantSet validValues;
		if (HasBoundedRangeValues(input, out var min, out var max))
		{
			string[] array = values.Where((string p) => !int.TryParse(p, out var result) || result < min || result > max).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
			if (array.Any())
			{
				error = $"invalid values ({string.Join(", ", array)}); expected an integer between {min} and {max}.";
				return false;
			}
		}
		else if (HasBoundedValues(input, out validValues))
		{
			string[] array2 = values.Where((string p) => !validValues.Contains(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
			if (array2.Any())
			{
				error = "invalid values (" + string.Join(", ", array2) + "); expected one of " + string.Join(", ", validValues);
				return false;
			}
		}
		error = null;
		return true;
	}

	public virtual IInvariantSet? GetValidPositionalArgs()
	{
		return null;
	}

	public virtual bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		allowedValues = null;
		return false;
	}

	public virtual bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
	{
		min = 0;
		max = 0;
		return false;
	}

	public virtual IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return InvariantSets.Empty;
	}

	protected BaseValueProvider(string name, bool mayReturnMultipleValuesForRoot, bool isDeterministicForInput = false)
	{
		Name = name;
		MayReturnMultipleValuesForRoot = mayReturnMultipleValuesForRoot;
		IsDeterministicForInput = isDeterministicForInput;
	}

	protected BaseValueProvider(ConditionType type, bool mayReturnMultipleValuesForRoot, bool isDeterministicForInput = false)
		: this(type.ToString(), mayReturnMultipleValuesForRoot, isDeterministicForInput)
	{
	}

	protected void EnableInputArguments(bool required, bool mayReturnMultipleValues, int? maxPositionalArgs)
	{
		AllowsPositionalInput = true;
		RequiresPositionalInput = required;
		MayReturnMultipleValuesForInput = mayReturnMultipleValues;
		MaxPositionalArgs = maxPositionalArgs;
	}

	protected void AssertInput(IInputArguments input)
	{
		if (RequiresPositionalInput && !input.HasPositionalArgs)
		{
			throw new InvalidOperationException("The '" + Name + "' token requires input arguments.");
		}
		if (!AllowsPositionalInput && input.HasPositionalArgs)
		{
			throw new InvalidOperationException("The '" + Name + "' token does not allow input arguments.");
		}
		if (input.PositionalArgs.Length > MaxPositionalArgs)
		{
			throw new InvalidOperationException($"The '{Name}' token doesn't allow more than {MaxPositionalArgs} input argument{((MaxPositionalArgs == 1) ? "" : "s")}.");
		}
	}

	protected bool TryParseEnum<TEnum>(string? raw, out TEnum result, bool mustBeNamed = true) where TEnum : struct
	{
		if (!Enum.TryParse<TEnum>(raw, ignoreCase: true, out result))
		{
			return false;
		}
		if (mustBeNamed && !Enum.IsDefined(typeof(TEnum), result))
		{
			return false;
		}
		return true;
	}

	protected bool MarkReady(bool ready)
	{
		if (ready)
		{
			State.Reset();
		}
		else
		{
			State.AddUnreadyToken(Name);
		}
		return ready;
	}

	protected bool IsChanged(IInvariantSet values, Func<IInvariantSet> action)
	{
		return IsChanged(() => IsChanged(values, action()));
	}

	protected bool IsChanged<T>(ISet<T> values, Action action)
	{
		return IsChanged(() =>
		{
			HashSet<T> oldValues = new HashSet<T>(values);
			action();
			return IsChanged(oldValues, values);
		});
	}

	protected bool IsChanged(IDictionary<string, string> values, Action action)
	{
		return IsChanged(() =>
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(values);
			action();
			return values.Count != dictionary.Count || dictionary.Any((KeyValuePair<string, string> entry) =>
			{
				int result;
				if (values.TryGetValue(entry.Key, out string value))
				{
					string value2 = entry.Value;
					result = ((value2 == null || !StringExtensions.EqualsIgnoreCase(value2, value)) ? 1 : 0);
				}
				else
				{
					result = 1;
				}
				return (byte)result != 0;
			});
		});
	}

	protected bool IsChanged(Func<bool> action)
	{
		bool isReady = IsReady;
		return action() || IsReady != isReady;
	}

	protected bool IsChanged(IInvariantSet oldValues, IInvariantSet newValues)
	{
		return oldValues != newValues && (newValues.Count != oldValues.Count || newValues.Any((string p) => !oldValues.Contains(p)));
	}

	protected bool IsChanged<T>(ISet<T> oldValues, ISet<T> newValues)
	{
		return oldValues != newValues && (newValues.Count != oldValues.Count || newValues.Any((T p) => !oldValues.Contains(p)));
	}

	protected static IInvariantSet WrapOptionalValue(string? value)
	{
		return string.IsNullOrWhiteSpace(value) ? InvariantSets.Empty : InvariantSets.FromValue(value);
	}
}
