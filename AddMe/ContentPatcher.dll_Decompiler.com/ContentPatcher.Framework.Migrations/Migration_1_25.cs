using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_1_25 : BaseMigration
{
	public Migration_1_25()
		: base((ISemanticVersion)new SemanticVersion(1, 25, 0, (string)null, (string)null))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected Obj, but got Unknown
		AddedTokens = new InvariantSet("AbsoluteFilePath", "FormatAssetName", "InternalAssetKey");
	}

	public override bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref lexToken, out error))
		{
			return false;
		}
		if (lexToken is LexTokenToken lexTokenToken && Enum.TryParse<ConditionType>(lexTokenToken.Name, ignoreCase: true, out var result) && IsPlayerToken(result) && lexTokenToken.HasInputArgs() && IsPlayerToken(result))
		{
			InputArguments inputArguments = new InputArguments(new LiteralString(lexTokenToken.InputArgs.ToString(), new LogPathBuilder()));
			string[] positionalArgs = inputArguments.PositionalArgs;
			foreach (string value in positionalArgs)
			{
				if (Enum.TryParse<PlayerType>(value, ignoreCase: true, out var result2) && result2 == PlayerType.AnyPlayer)
				{
					error = GetNounPhraseError($"using the '{PlayerType.AnyPlayer}' player type");
					return false;
				}
			}
		}
		return true;
	}

	public override bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected Obj, but got Unknown
		if (!base.TryMigrateMainContent(content, out error))
		{
			return false;
		}
		if (content.Format.PatchVersion != 0 || content.Format.PrereleaseTag != null)
		{
			error = GetNounPhraseError($"using {"Format"} with a patch version (like {content.Format} instead of {new SemanticVersion(content.Format.MajorVersion, content.Format.MinorVersion, 0, (string)null, (string)null)})");
			return false;
		}
		return true;
	}

	public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref patches, out error))
		{
			return false;
		}
		PatchConfig[] array = patches;
		foreach (PatchConfig patchConfig in array)
		{
			if (patchConfig.TargetField.Any())
			{
				error = GetNounPhraseError("using TargetField");
				return false;
			}
		}
		return true;
	}

	private bool IsPlayerToken(ConditionType type)
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
		case ConditionType.IsMainPlayer:
		case ConditionType.IsOutdoors:
		case ConditionType.LocationContext:
		case ConditionType.LocationName:
		case ConditionType.LocationUniqueName:
		case ConditionType.PlayerGender:
		case ConditionType.PlayerName:
		case ConditionType.ChildNames:
		case ConditionType.ChildGenders:
		case ConditionType.Spouse:
		case ConditionType.FarmhouseUpgrade:
			return true;
		default:
			return false;
		}
	}
}
