using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_24 : BaseMigration
{
	public Migration_1_24()
		: base((ISemanticVersion)new SemanticVersion(1, 24, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("HasCookingRecipe", "HasCraftingRecipe", "LocationOwnerId", "Merge", "Roommate", "PathPart");
	}

	public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref lexToken, out error))
		{
			return false;
		}
		if (!(lexToken is LexTokenToken lexTokenToken) || !Enum.TryParse<ConditionType>(lexTokenToken.Name, ignoreCase: true, out var result))
		{
			return true;
		}
		if (lexTokenToken.HasInputArgs())
		{
			bool flag = IsOldPlayerToken(result);
			bool flag2 = !flag && IsNewPlayerToken(result);
			if (flag | flag2)
			{
				InputArguments inputArguments = new InputArguments(new LiteralString(lexTokenToken.InputArgs.ToString(), new LogPathBuilder()));
				if (flag && long.TryParse(inputArguments.PositionalArgs.FirstOrDefault(), out var _))
				{
					error = GetNounPhraseError($"using {result} token with a player ID");
					return false;
				}
				if (flag2 && inputArguments.PositionalArgs.Any())
				{
					error = GetNounPhraseError($"using {result} token with input arguments");
					return false;
				}
			}
		}
		if (result == ConditionType.Spouse)
		{
			lexToken = new LexTokenToken("Merge", new LexTokenInput(new ILexToken[3]
			{
				new LexTokenToken("Roommate", null, impliedBraces: false),
				new LexTokenLiteral(","),
				new LexTokenToken("Spouse", lexTokenToken.InputArgs, impliedBraces: false)
			}), lexTokenToken.ImpliedBraces);
		}
		return true;
	}

	private bool IsOldPlayerToken(ConditionType type)
	{
		switch (type)
		{
		case ConditionType.HasActiveQuest:
		case ConditionType.HasCaughtFish:
		case ConditionType.HasConversationTopic:
		case ConditionType.HasFlag:
		case ConditionType.HasProfession:
		case ConditionType.HasReadLetter:
		case ConditionType.HasSeenEvent:
		case ConditionType.DailyLuck:
		case ConditionType.HasDialogueAnswer:
		case ConditionType.ChildNames:
		case ConditionType.ChildGenders:
		case ConditionType.FarmhouseUpgrade:
			return true;
		default:
			return false;
		}
	}

	private bool IsNewPlayerToken(ConditionType type)
	{
		if ((uint)(type - 20) <= 3u || (uint)(type - 25) <= 2u || type == ConditionType.Spouse)
		{
			return true;
		}
		return false;
	}
}
