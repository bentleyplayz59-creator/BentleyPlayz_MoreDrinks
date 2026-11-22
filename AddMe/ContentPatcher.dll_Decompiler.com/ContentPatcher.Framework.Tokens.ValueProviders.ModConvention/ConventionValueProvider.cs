using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders.ModConvention;

internal class ConventionValueProvider : IValueProvider, IContextual, IContextualInfo
{
	private readonly ConventionWrapper Provider;

	private readonly ContextualState State = new ContextualState();

	public string Name { get; }

	public bool AllowsPositionalInput => Provider.AllowsInput();

	public bool RequiresPositionalInput => Provider.RequiresInput();

	public bool IsMutable => Provider.IsMutable();

	public bool IsDeterministicForInput => Provider.IsDeterministicForInput();

	public bool IsReady { get; private set; }

	public bool BypassesContextValidation => false;

	public Func<string, string>? NormalizeValue => Provider.NormalizeValue;

	public ConventionValueProvider(string name, ConventionWrapper provider)
	{
		Name = name;
		Provider = provider;
		IsReady = provider.IsReady();
	}

	public bool CanHaveMultipleValues(IInputArguments input)
	{
		return Provider.CanHaveMultipleValues(ToApiInput(input));
	}

	public bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		return Provider.TryValidateInput(ToApiInput(input), out error);
	}

	public bool TryValidateValues(IInputArguments input, IInvariantSet values, [NotNullWhen(false)] out string? error)
	{
		return Provider.TryValidateValues(ToApiInput(input), values, out error);
	}

	public IInvariantSet GetValidPositionalArgs()
	{
		return InvariantSets.From(Provider.GetValidInputs());
	}

	public bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		if (Provider.HasBoundedValues(ToApiInput(input), out IEnumerable<string> allowedValues2))
		{
			allowedValues = InvariantSets.From(allowedValues2);
			return true;
		}
		allowedValues = null;
		return false;
	}

	public bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
	{
		return Provider.HasBoundedRangeValues(ToApiInput(input), out min, out max);
	}

	public IEnumerable<string> GetValues(IInputArguments input)
	{
		return Provider.GetValues(ToApiInput(input));
	}

	public bool UpdateContext(IContext context)
	{
		bool isReady = IsReady;
		bool flag = Provider.UpdateContext();
		IsReady = Provider.IsReady();
		return flag || IsReady != isReady;
	}

	public IContextualState GetDiagnosticState()
	{
		return State;
	}

	public IInvariantSet GetTokensUsed()
	{
		return InvariantSets.Empty;
	}

	private string? ToApiInput(IInputArguments input)
	{
		if (!input.IsReady)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(string.Join(", ", input.PositionalArgs));
		foreach (KeyValuePair<string, IInputArgumentValue> namedArg in input.NamedArgs)
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder2);
			handler.AppendLiteral(" |");
			handler.AppendFormatted(namedArg.Key);
			handler.AppendLiteral("=");
			handler.AppendFormatted(string.Join(", ", namedArg.Value.Parsed));
			stringBuilder2.Append(ref handler);
		}
		return (stringBuilder.Length > 0) ? stringBuilder.ToString() : null;
	}
}
