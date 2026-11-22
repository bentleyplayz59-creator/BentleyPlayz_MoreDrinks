using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class VillagerHeartsValueProvider : BaseValueProvider
{
	private readonly TokenSaveReader SaveReader;

	private readonly SortedDictionary<string, string> Values = new SortedDictionary<string, string>(HumanSortComparer.DefaultIgnoreCase);

	public VillagerHeartsValueProvider(TokenSaveReader saveReader)
		: base(ConditionType.Hearts, mayReturnMultipleValuesForRoot: false)
	{
		SaveReader = saveReader;
		EnableInputArguments(required: false, mayReturnMultipleValues: false, 1);
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(Values, () =>
		{
			Values.Clear();
			if (MarkReady(SaveReader.IsReady))
			{
				foreach (var (key, val2) in SaveReader.GetFriendships())
				{
					int num = ((val2 != null) ? val2.Points : 0);
					Values[key] = (num / 250).ToString(CultureInfo.InvariantCulture);
				}
			}
		});
	}

	public override IInvariantSet GetValidPositionalArgs()
	{
		return InvariantSets.From(Values.Keys);
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		string value;
		if (input.HasPositionalArgs)
		{
			return Values.TryGetValue(input.GetFirstPositionalArg(), out value) ? InvariantSets.FromValue(value) : InvariantSets.Empty;
		}
		return Values.Select((KeyValuePair<string, string> pair) => pair.Key + ":" + pair.Value);
	}
}
