using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class SkillLevelValueProvider : BaseValueProvider
{
	private readonly TokenSaveReader SaveReader;

	private readonly Dictionary<Skill, int> SkillLevels = new Dictionary<Skill, int>();

	private readonly IInvariantSet ValidValues = InvariantSets.From(Enum.GetNames(typeof(Skill)));

	public SkillLevelValueProvider(TokenSaveReader saveReader)
		: base(ConditionType.SkillLevel, mayReturnMultipleValuesForRoot: true)
	{
		SaveReader = saveReader;
		EnableInputArguments(required: false, mayReturnMultipleValues: false, 1);
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(() =>
		{
			Dictionary<Skill, int> oldSkillLevels = new Dictionary<Skill, int>(SkillLevels);
			SkillLevels.Clear();
			if (MarkReady(SaveReader.IsReady))
			{
				Farmer currentPlayer = SaveReader.GetCurrentPlayer();
				SkillLevels[Skill.Combat] = ((currentPlayer != null) ? currentPlayer.CombatLevel : 0);
				SkillLevels[Skill.Farming] = ((currentPlayer != null) ? currentPlayer.FarmingLevel : 0);
				SkillLevels[Skill.Fishing] = ((currentPlayer != null) ? currentPlayer.FishingLevel : 0);
				SkillLevels[Skill.Foraging] = ((currentPlayer != null) ? currentPlayer.ForagingLevel : 0);
				SkillLevels[Skill.Luck] = ((currentPlayer != null) ? currentPlayer.LuckLevel : 0);
				SkillLevels[Skill.Mining] = ((currentPlayer != null) ? currentPlayer.MiningLevel : 0);
				return SkillLevels.Count != oldSkillLevels.Count || SkillLevels.Any((KeyValuePair<Skill, int> entry) => !oldSkillLevels.TryGetValue(entry.Key, out var value) || entry.Value != value);
			}
			return false;
		});
	}

	public override IInvariantSet GetValidPositionalArgs()
	{
		return ValidValues;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		Skill result;
		int value;
		if (input.HasPositionalArgs)
		{
			return (TryParseEnum<Skill>(input.GetFirstPositionalArg(), out result) && SkillLevels.TryGetValue(result, out value)) ? InvariantSets.FromValue(value) : InvariantSets.Empty;
		}
		return SkillLevels.Select((KeyValuePair<Skill, int> pair) => $"{pair.Key}:{pair.Value}");
	}
}
