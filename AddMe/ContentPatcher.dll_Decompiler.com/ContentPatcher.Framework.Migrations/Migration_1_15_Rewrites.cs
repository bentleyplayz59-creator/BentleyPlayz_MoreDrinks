using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_15_Rewrites : BaseMigration
{
	private static readonly Regex IgnoreRandomArgumentPattern = new Regex("\\|\\s*key\\s*=", RegexOptions.Compiled);

	private readonly HashSet<ConditionType> TokensWhichDroppedSearchForm = new HashSet<ConditionType>
	{
		ConditionType.Day,
		ConditionType.DayEvent,
		ConditionType.DayOfWeek,
		ConditionType.DaysPlayed,
		ConditionType.Season,
		ConditionType.Year,
		ConditionType.Weather,
		ConditionType.HasDialogueAnswer,
		ConditionType.HasFlag,
		ConditionType.HasProfession,
		ConditionType.HasReadLetter,
		ConditionType.HasSeenEvent,
		ConditionType.HasWalletItem,
		ConditionType.IsMainPlayer,
		ConditionType.IsOutdoors,
		ConditionType.LocationName,
		ConditionType.PlayerGender,
		ConditionType.PlayerName,
		ConditionType.PreferredPet,
		ConditionType.Spouse,
		ConditionType.FarmCave,
		ConditionType.FarmhouseUpgrade,
		ConditionType.FarmName,
		ConditionType.FarmType,
		ConditionType.IsCommunityCenterComplete,
		ConditionType.IsJojaMartComplete,
		ConditionType.Pregnant,
		ConditionType.HavingChild,
		ConditionType.HasMod,
		ConditionType.Language
	};

	private readonly Lazy<IInvariantSet> LocalTokenNames;

	public Migration_1_15_Rewrites(ContentConfig? content)
		: base((ISemanticVersion)new SemanticVersion(1, 15, 0, (string)null, (string)null))
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected Obj, but got Unknown
		Migration_1_15_Rewrites migration_1_15_Rewrites = this;
		LocalTokenNames = new Lazy<IInvariantSet>(() => migration_1_15_Rewrites.GetLocalTokenNames(content));
	}

	public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref lexToken, out error))
		{
			return false;
		}
		if (lexToken is LexTokenToken lexTokenToken && lexTokenToken.HasInputArgs())
		{
			ConditionType? conditionType = GetEnum<ConditionType>(lexTokenToken.Name);
			if (LocalTokenNames.Value.Contains(lexTokenToken.Name) || (conditionType.HasValue && TokensWhichDroppedSearchForm.Contains(conditionType.Value)))
			{
				List<ILexToken> list = new List<ILexToken>(lexTokenToken.InputArgs.Parts);
				if (list[0] is LexTokenLiteral lexTokenLiteral)
				{
					lexTokenLiteral.MigrateTo("|contains=" + lexTokenLiteral.Text);
				}
				else
				{
					list.Insert(0, new LexTokenLiteral("|contains="));
				}
				lexTokenToken.InputArgs.MigrateTo(list.ToArray());
			}
			if (conditionType == ConditionType.Random)
			{
				LexTokenLiteral lexTokenLiteral2 = lexTokenToken.InputArgs.Parts.OfType<LexTokenLiteral>().FirstOrDefault((LexTokenLiteral p) => p.ToString().Contains("|"));
				if (lexTokenLiteral2 != null && !IgnoreRandomArgumentPattern.IsMatch(lexTokenLiteral2.Text))
				{
					int num = lexTokenLiteral2.Text.IndexOf("|", StringComparison.Ordinal);
					string text = lexTokenLiteral2.Text.Substring(0, num + 1) + "key=" + lexTokenLiteral2.Text.Substring(num + 1).TrimStart();
					lexTokenLiteral2.MigrateTo(text);
				}
			}
		}
		return true;
	}

	private IInvariantSet GetLocalTokenNames(ContentConfig? content)
	{
		if (content == null)
		{
			return InvariantSet.Empty;
		}
		MutableInvariantSet mutableInvariantSet = null;
		foreach (string item2 in content.DynamicTokens.Select((DynamicTokenConfig p) => p.Name))
		{
			if (!string.IsNullOrWhiteSpace(item2))
			{
				if (mutableInvariantSet == null)
				{
					mutableInvariantSet = new MutableInvariantSet();
				}
				mutableInvariantSet.Add(item2);
			}
		}
		foreach (string item3 in content.ConfigSchema.Select((KeyValuePair<string, ConfigSchemaFieldConfig> p) => p.Key))
		{
			if (!string.IsNullOrWhiteSpace(item3))
			{
				if (mutableInvariantSet == null)
				{
					mutableInvariantSet = new MutableInvariantSet();
				}
				mutableInvariantSet.Add(item3);
			}
		}
		if (mutableInvariantSet != null)
		{
			string[] array = mutableInvariantSet.Where((string p) => GetEnum<ConditionType>(p).HasValue).ToArray();
			foreach (string item in array)
			{
				mutableInvariantSet.Remove(item);
			}
		}
		return mutableInvariantSet?.Lock() ?? InvariantSets.Empty;
	}
}
