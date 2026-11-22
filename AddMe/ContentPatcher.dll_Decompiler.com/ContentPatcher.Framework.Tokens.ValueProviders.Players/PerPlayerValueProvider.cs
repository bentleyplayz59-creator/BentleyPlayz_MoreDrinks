using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Tokens.ValueProviders.Players;

internal class PerPlayerValueProvider : BaseValueProvider
{
	private readonly TokenSaveReader SaveReader;

	private readonly IInvariantSet? AllowedRootValues;

	private readonly Func<Farmer, IInvariantSet> FetchValues;

	private readonly Dictionary<long, IInvariantSet> Values = new Dictionary<long, IInvariantSet>();

	private long HostPlayerId;

	private long LocalPlayerId;

	public PerPlayerValueProvider(ConditionType type, Func<Farmer, IEnumerable<string>> values, TokenSaveReader saveReader, bool mayReturnMultipleValues = false, IEnumerable<string>? allowedValues = null)
		: base(type, mayReturnMultipleValues)
	{
		SaveReader = saveReader;
		AllowedRootValues = ((allowedValues != null) ? InvariantSets.From(allowedValues) : null);
		FetchValues = (Farmer player) => InvariantSets.From(values(player));
		EnableInputArguments(required: false, mayReturnMultipleValues, null);
	}

	public PerPlayerValueProvider(ConditionType type, Func<Farmer, string?> value, TokenSaveReader saveReader, bool mayReturnMultipleValues = false, IEnumerable<string>? allowedValues = null)
		: this(type, (Farmer player) => BaseValueProvider.WrapOptionalValue(value(player)), saveReader, mayReturnMultipleValues, allowedValues)
	{
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(() =>
		{
			bool flag = false;
			if (MarkReady(SaveReader.IsReady))
			{
				Farmer? player = SaveReader.GetPlayer(PlayerType.HostPlayer);
				long num = ((player != null) ? player.UniqueMultiplayerID : 0);
				Farmer? player2 = SaveReader.GetPlayer(PlayerType.CurrentPlayer);
				long num2 = ((player2 != null) ? player2.UniqueMultiplayerID : 0);
				flag |= HostPlayerId != num || LocalPlayerId != num2;
				HostPlayerId = num;
				LocalPlayerId = num2;
				HashSet<long> hashSet = new HashSet<long>();
				foreach (long key in Values.Keys)
				{
					hashSet.Add(key);
				}
				HashSet<long> hashSet2 = hashSet;
				foreach (Farmer allPlayer in SaveReader.GetAllPlayers())
				{
					long uniqueMultiplayerID = allPlayer.UniqueMultiplayerID;
					IInvariantSet invariantSet = FetchValues(allPlayer);
					IInvariantSet valueOrDefault = Values.GetValueOrDefault(uniqueMultiplayerID);
					hashSet2.Remove(uniqueMultiplayerID);
					flag |= valueOrDefault == null || IsChanged(valueOrDefault, invariantSet);
					Values[uniqueMultiplayerID] = invariantSet;
				}
				foreach (long item in hashSet2)
				{
					Values.Remove(item);
					flag = true;
				}
			}
			else
			{
				Values.Clear();
				HostPlayerId = 0L;
				LocalPlayerId = 0L;
			}
			return flag;
		});
	}

	public override bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		return base.TryValidateInput(input, out error) && (!input.HasPositionalArgs || TryParseInput(input, null, out error));
	}

	public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		allowedValues = AllowedRootValues;
		return allowedValues != null;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		if (!input.HasPositionalArgs)
		{
			return GetValuesFor(null);
		}
		HashSet<long> playerIds = new HashSet<long>();
		IEnumerable<string> result;
		if (!TryParseInput(input, playerIds, out string _))
		{
			IEnumerable<string> empty = InvariantSets.Empty;
			result = empty;
		}
		else
		{
			result = GetValuesFor(playerIds);
		}
		return result;
	}

	private bool TryParseInput(IInputArguments input, HashSet<long>? playerIds, [NotNullWhen(false)] out string? error)
	{
		string[] positionalArgs = input.PositionalArgs;
		foreach (string text in positionalArgs)
		{
			if (long.TryParse(text, out var result))
			{
				playerIds?.Add(result);
				continue;
			}
			if (Enum.TryParse<PlayerType>(text, ignoreCase: true, out var result2))
			{
				switch (result2)
				{
				case PlayerType.HostPlayer:
					playerIds?.Add(HostPlayerId);
					break;
				case PlayerType.CurrentPlayer:
					playerIds?.Add(LocalPlayerId);
					break;
				case PlayerType.AnyPlayer:
					if (playerIds != null)
					{
						CollectionExtensions.AddRange<long>((ISet<long>)playerIds, (IEnumerable<long>)Values.Keys);
					}
					break;
				default:
					throw new InvalidOperationException($"Unknown player type {result2}.");
				}
				continue;
			}
			error = $"invalid input arguments ({input.TokenString}) for {Name} token, expected any combination of '{string.Join("', '", Enum.GetNames(typeof(PlayerType)))}', or player IDs.";
			return false;
		}
		error = null;
		return true;
	}

	private IEnumerable<string> GetValuesFor(HashSet<long>? playerIds)
	{
		if (playerIds == null || playerIds.Count <= 1)
		{
			return GetValuesFor(playerIds?.FirstOrDefault() ?? LocalPlayerId);
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (long playerId in playerIds)
		{
			if (Values.TryGetValue(playerId, out IInvariantSet value))
			{
				CollectionExtensions.AddRange<string>((ISet<string>)hashSet, (IEnumerable<string>)value);
			}
		}
		return hashSet;
	}

	private IInvariantSet GetValuesFor(long playerId)
	{
		IInvariantSet value;
		return Values.TryGetValue(playerId, out value) ? value : InvariantSets.Empty;
	}
}
