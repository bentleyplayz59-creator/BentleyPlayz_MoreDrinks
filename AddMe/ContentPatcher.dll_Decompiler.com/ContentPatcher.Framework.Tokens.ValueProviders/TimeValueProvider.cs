using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class TimeValueProvider : BaseValueProvider
{
	private readonly TokenSaveReader SaveReader;

	private string? TimeOfDay;

	public TimeValueProvider(TokenSaveReader saveReader)
		: base(ConditionType.Time, mayReturnMultipleValuesForRoot: false)
	{
		SaveReader = saveReader;
		NormalizeValue = NormalizeValueImpl;
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(() =>
		{
			string timeOfDay = TimeOfDay;
			TimeOfDay = (MarkReady(SaveReader.IsReady) ? NormalizeValueImpl(SaveReader.GetTime()) : null);
			return timeOfDay != TimeOfDay;
		});
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return BaseValueProvider.WrapOptionalValue(TimeOfDay);
	}

	public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
	{
		min = 600;
		max = 2600;
		return true;
	}

	private string NormalizeValueImpl(string value)
	{
		if (value.Length > 4)
		{
			value = value.TrimStart('0');
		}
		if (value.Length < 4)
		{
			value = value.PadLeft(4, '0');
		}
		return value;
	}

	private string NormalizeValueImpl(int value)
	{
		return value.ToString("D4");
	}
}
