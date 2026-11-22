using System;
using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens;

internal interface IToken : IContextual, IContextualInfo
{
	string? Scope { get; }

	string Name { get; }

	bool RequiresInput { get; }

	bool IsDeterministicForInput { get; }

	bool BypassesContextValidation { get; }

	Func<string, string>? NormalizeValue { get; }

	bool CanHaveMultipleValues(IInputArguments input);

	bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error);

	bool TryValidateValues(IInputArguments input, IInvariantSet values, IContext context, [NotNullWhen(false)] out string? error);

	IInvariantSet? GetAllowedInputArguments();

	bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues);

	bool HasBoundedRangeValues(IInputArguments input, out int min, out int max);

	IInvariantSet GetValues(IInputArguments input);
}
