using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ContentPatcher.Framework.Tokens.ValueProviders.ModConvention;

internal static class ConventionDelegates
{
	internal delegate bool IsMutable();

	internal delegate bool IsDeterministicForInput();

	internal delegate bool AllowsInput();

	internal delegate bool RequiresInput();

	internal delegate bool CanHaveMultipleValues(string? input = null);

	internal delegate IEnumerable<string> GetValidInputs();

	internal delegate bool HasBoundedValues(string? input, out IEnumerable<string> allowedValues);

	internal delegate bool HasBoundedRangeValues(string? input, out int min, out int max);

	internal delegate bool TryValidateInput(string? input, [NotNullWhen(false)] out string? error);

	internal delegate bool TryValidateValues(string? input, IEnumerable<string> values, [NotNullWhen(false)] out string? error);

	internal delegate string NormalizeValue(string value);

	internal delegate bool UpdateContext();

	internal delegate bool IsReady();

	internal delegate IEnumerable<string> GetValues(string? input);
}
