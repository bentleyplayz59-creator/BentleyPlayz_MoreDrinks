using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class VillagerRelationshipValueProvider : BaseValueProvider
{
	private readonly TokenSaveReader SaveReader;

	private readonly SortedDictionary<string, string> Values = new SortedDictionary<string, string>(HumanSortComparer.DefaultIgnoreCase);

	public VillagerRelationshipValueProvider(TokenSaveReader saveReader)
		: base(ConditionType.Relationship, mayReturnMultipleValuesForRoot: false)
	{
		SaveReader = saveReader;
		EnableInputArguments(required: false, mayReturnMultipleValues: false, 1);
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(Values, () =>
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			Values.Clear();
			if (MarkReady(SaveReader.IsReady))
			{
				foreach (var (key, val2) in SaveReader.GetFriendships())
				{
					Values[key] = ((val2 != null) ? ((object)val2.Status/*cast due to constrained. prefix*/).ToString() : null) ?? "Unmet";
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
