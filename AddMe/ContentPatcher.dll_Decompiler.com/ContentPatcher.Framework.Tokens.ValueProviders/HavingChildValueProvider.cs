using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;
using StardewValley.GameData.Characters;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class HavingChildValueProvider : BaseValueProvider
{
	private readonly TokenSaveReader SaveReader;

	private readonly SortedSet<string> PartnersHavingChild = new SortedSet<string>(HumanSortComparer.DefaultIgnoreCase);

	private readonly bool PregnancyOnly;

	public HavingChildValueProvider(ConditionType type, TokenSaveReader saveReader)
		: base(type, mayReturnMultipleValuesForRoot: true)
	{
		if (type != ConditionType.HavingChild && type != ConditionType.Pregnant)
		{
			throw new ArgumentException($"The condition type must be {ConditionType.HavingChild} or {ConditionType.Pregnant}.");
		}
		SaveReader = saveReader;
		PregnancyOnly = type == ConditionType.Pregnant;
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(PartnersHavingChild, () =>
		{
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Invalid comparison between Unknown and I4
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Invalid comparison between Unknown and I4
			PartnersHavingChild.Clear();
			if (MarkReady(SaveReader.IsReady))
			{
				foreach (Farmer allPlayer in SaveReader.GetAllPlayers())
				{
					if (SaveReader.TryGetSpouseInfo(allPlayer, out string name, out Friendship friendship, out Gender gender, out bool isPlayer) && !(friendship.NextBirthingDate == (WorldDate)null) && !(friendship.NextBirthingDate <= Game1.Date))
					{
						if (PregnancyOnly)
						{
							if (CanHavePregnancyWith(allPlayer, name, gender, isPlayer))
							{
								if ((int)gender == 1)
								{
									PartnersHavingChild.Add((isPlayer ? "@" : "") + name);
								}
								else if ((int)((Character)allPlayer).Gender == 1)
								{
									PartnersHavingChild.Add("@" + ((Character)allPlayer).Name);
								}
							}
						}
						else
						{
							PartnersHavingChild.Add("@" + ((Character)allPlayer).Name);
							PartnersHavingChild.Add((isPlayer ? "@" : "") + name);
						}
					}
				}
			}
		});
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return PartnersHavingChild;
	}

	private bool CanHavePregnancyWith(Farmer player, string spouseName, Gender spouseGender, bool spouseIsPlayer)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		CharacterData val = default;
		if (!spouseIsPlayer && NPC.TryGetData(spouseName, ref val) && val.SpouseAdopts != null)
		{
			return !GameStateQuery.CheckConditions(val.SpouseAdopts, (GameLocation)null, player, (Item)null, (Item)null, (Random)null, (HashSet<string>)null);
		}
		return spouseGender != ((Character)player).Gender;
	}
}
