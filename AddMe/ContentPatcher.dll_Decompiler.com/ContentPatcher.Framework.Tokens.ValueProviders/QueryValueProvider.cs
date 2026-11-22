using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class QueryValueProvider : BaseValueProvider
{
	private static readonly Regex CannotFindColumnPattern = new Regex("Cannot find column \\[([^\\]]+)]\\.", RegexOptions.Compiled);

	private readonly DataTable DataTable = new DataTable();

	private readonly Dictionary<string, object> Cache = new Dictionary<string, object>();

	public QueryValueProvider()
		: base(ConditionType.Query, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		BypassesContextValidation = true;
		EnableInputArguments(required: true, mayReturnMultipleValues: false, null);
		MarkReady(ready: true);
	}

	public override bool UpdateContext(IContext context)
	{
		Cache.Clear();
		return false;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		string text;
		if (TryCalculate(input.GetPositionalSegment(), out object result, out string _))
		{
			text = ((result is IConvertible convertible) ? convertible.ToString(CultureInfo.InvariantCulture) : (result.ToString() ?? string.Empty));
		}
		else
		{
			text = "0";
		}
		if (1 == 0)
		{
		}
		IInvariantSet result2;
		if (text == "True")
		{
			result2 = InvariantSets.True;
		}
		else
		{
			result2 = ((!(text == "False")) ? InvariantSets.FromValue(text) : InvariantSets.False);
		}
		if (1 == 0)
		{
		}
		return result2;
	}

	public override bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryValidateInput(input, out error))
		{
			return false;
		}
		object result;
		if (input.IsReady)
		{
			return TryCalculate(input.GetPositionalSegment(), out result, out error);
		}
		return true;
	}

	private bool TryCalculate(string? input, [NotNullWhen(true)] out object? result, [NotNullWhen(false)] out string? error)
	{
		result = 0;
		if (string.IsNullOrWhiteSpace(input) || Cache.TryGetValue(input, out result))
		{
			error = null;
			return true;
		}
		try
		{
			Cache[input] = (result = DataTable.Compute(input, string.Empty));
			error = null;
			return true;
		}
		catch (Exception ex)
		{
			string message = ex.Message;
			message = CannotFindColumnPattern.Replace(message, "invalid expression '$1'.");
			result = 0;
			error = "Can't parse '" + input + "' as a math expression: " + message;
			return false;
		}
	}
}
