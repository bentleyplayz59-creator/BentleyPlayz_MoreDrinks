using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal interface IValueProvider : IContextual, IContextualInfo
{
	string Name { get; }

	bool AllowsPositionalInput { get; }

	bool RequiresPositionalInput { get; }

	bool IsDeterministicForInput { get; }

	bool BypassesContextValidation { get; }

	Func<string, string>? NormalizeValue { get; }

	bool CanHaveMultipleValues(IInputArguments input);

	bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error);

	bool TryValidateValues(IInputArguments input, IInvariantSet values, [NotNullWhen(false)] out string? error);

	IInvariantSet? GetValidPositionalArgs();

	bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues);

	bool HasBoundedRangeValues(IInputArguments input, out int min, out int max);

	IEnumerable<string> GetValues(IInputArguments input);
}
