using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using ContentPatcher.Framework.Tokens.ValueProviders.Players;
using Netcode;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Quests;

namespace ContentPatcher.Framework;

internal class TokenManager : IContext
{
	private record CachedContext(ModTokenContext Context);

	private readonly GenericTokenContext GlobalContext;

	private readonly InvariantDictionary<CachedContext> LocalTokens = new InvariantDictionary<CachedContext>();

	private readonly IInvariantSet InstalledMods;

	private bool IsFirstUpdate = true;

	public int UpdateTick { get; private set; }

	public bool IsSaveParsed { get; set; }

	public bool IsSaveBasicInfoLoaded { get; set; }

	public bool IsSaveLoaded { get; set; }

	public Tuple<UpdateRate, string, IInvariantSet>[] TokensWithSpecialUpdateRates { get; } = new Tuple<UpdateRate, string, IInvariantSet>[2]
	{
		Tuple.Create(UpdateRate.OnLocationChange, "location tokens", InvariantSets.From(new _003C_003Ez__ReadOnlyArray<string>(new string[4] { "LocationContext", "LocationName", "LocationUniqueName", "IsOutdoors" }))),
		Tuple.Create(UpdateRate.OnTimeChange, "time tokens", InvariantSets.FromValue("Time"))
	};

	public TokenManager(IGameContentHelper contentHelper, IInvariantSet installedMods, IEnumerable<IToken> modTokens)
	{
		InstalledMods = installedMods;
		GlobalContext = new GenericTokenContext(IsModInstalled, () => UpdateTick);
		foreach (IToken modToken in modTokens)
		{
			GlobalContext.Save(modToken);
		}
		foreach (IValueProvider globalValueProvider in GetGlobalValueProviders(contentHelper, installedMods))
		{
			GlobalContext.Save(new Token(globalValueProvider));
		}
	}

	public ModTokenContext TrackLocalTokens(IContentPack pack)
	{
		string text = pack.Manifest.UniqueID.Trim();
		if (!LocalTokens.TryGetValue(text, out CachedContext value))
		{
			ModTokenContext modTokenContext = new ModTokenContext(text, this);
			value = (LocalTokens[text] = new CachedContext(modTokenContext));
			foreach (IValueProvider localValueProvider in GetLocalValueProviders(pack))
			{
				modTokenContext.AddLocalToken(new Token(localValueProvider, text));
			}
		}
		return value.Context;
	}

	public string ResolveAlias(string contentPackId, string tokenName)
	{
		CachedContext value;
		return LocalTokens.TryGetValue(contentPackId, out value) ? value.Context.ResolveAlias(tokenName) : tokenName;
	}

	public IContext GetContextFor(string contentPackId)
	{
		contentPackId = contentPackId.Trim();
		if (!LocalTokens.TryGetValue(contentPackId, out CachedContext value))
		{
			throw new KeyNotFoundException("There's no content pack registered for ID '" + contentPackId + "'.");
		}
		return value.Context;
	}

	public void UpdateContext(out IInvariantSet changedGlobalTokens)
	{
		UpdateTick++;
		MutableInvariantSet mutableInvariantSet = new MutableInvariantSet();
		foreach (IToken value in GlobalContext.Tokens.Values)
		{
			if ((value.IsMutable && value.UpdateContext(this)) || (IsFirstUpdate && value.IsReady))
			{
				mutableInvariantSet.Add(value.Name);
			}
		}
		if (mutableInvariantSet.Contains("Language"))
		{
			mutableInvariantSet.Add("I18n");
		}
		changedGlobalTokens = mutableInvariantSet.Lock();
		foreach (CachedContext value2 in LocalTokens.Values)
		{
			value2.Context.UpdateContext(changedGlobalTokens);
		}
		IsFirstUpdate = false;
	}

	public bool IsModInstalled(string id)
	{
		return InstalledMods.Contains(id);
	}

	public bool Contains(string name, bool enforceContext)
	{
		return GlobalContext.Contains(name, enforceContext);
	}

	public IToken? GetToken(string name, bool enforceContext)
	{
		return GlobalContext.GetToken(name, enforceContext);
	}

	public IEnumerable<IToken> GetTokens(bool enforceContext)
	{
		return GlobalContext.GetTokens(enforceContext);
	}

	public IInvariantSet GetValues(string name, IInputArguments input, bool enforceContext)
	{
		return GlobalContext.GetValues(name, input, enforceContext);
	}

	private IEnumerable<IValueProvider> GetGlobalValueProviders(IGameContentHelper contentHelper, IInvariantSet installedMods)
	{
		TokenSaveReader save = new TokenSaveReader(() => UpdateTick, NeedsSave, () => IsSaveBasicInfoLoaded, () => IsSaveLoaded);
		return new _003C_003Ez__ReadOnlyArray<IValueProvider>(new IValueProvider[60]
		{
			new ConditionTypeValueProvider(ConditionType.Day, () => save.GetDay().ToString(), NeedsSave, mayReturnMultipleValues: false, from p in Enumerable.Range(0, 29)
				select p.ToString()),
			new ConditionTypeValueProvider(ConditionType.DayEvent, (Func<string?>)save.GetDayEvent, (Func<bool>?)NeedsSave, false, (IEnumerable<string>?)null),
			new ConditionTypeValueProvider(ConditionType.DayOfWeek, () => save.GetDayOfWeek().ToString(), NeedsSave, mayReturnMultipleValues: false, Enum.GetNames(typeof(DayOfWeek))),
			new ConditionTypeValueProvider(ConditionType.DaysPlayed, () => save.GetDaysPlayed().ToString(), NeedsSave),
			new ConditionTypeValueProvider(ConditionType.Season, (Func<string?>)save.GetSeason, (Func<bool>?)NeedsSave, false, (IEnumerable<string>?)new _003C_003Ez__ReadOnlyArray<string>(new string[4] { "Spring", "Summer", "Fall", "Winter" })),
			new ConditionTypeValueProvider(ConditionType.Year, () => save.GetYear().ToString(), NeedsSave),
			new WeatherValueProvider(save),
			new TimeValueProvider(save),
			new PerPlayerValueProvider(ConditionType.DailyLuck, (Farmer player) => save.GetDailyLuck(player).ToString(CultureInfo.InvariantCulture), save),
			new PerPlayerValueProvider(ConditionType.FarmhouseUpgrade, (Farmer player) => player.HouseUpgradeLevel.ToString(), save),
			new PerPlayerValueProvider(ConditionType.HasCaughtFish, (Farmer player) =>
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return (IEnumerable<string>)(object)((NetDictionary<string, int[], NetArray<int, NetInt>, SerializableDictionary<string, int[]>, NetStringIntArrayDictionary>)(object)player.fishCaught).Keys;
			}, save),
			new PerPlayerValueProvider(ConditionType.HasConversationTopic, (Farmer player) =>
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return (IEnumerable<string>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.activeDialogueEvents).Keys;
			}, save),
			new PerPlayerValueProvider(ConditionType.HasCookingRecipe, (Farmer player) =>
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return (IEnumerable<string>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.cookingRecipes).Keys;
			}, save),
			new PerPlayerValueProvider(ConditionType.HasCraftingRecipe, (Farmer player) =>
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return (IEnumerable<string>)(object)((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.craftingRecipes).Keys;
			}, save),
			new PerPlayerValueProvider(ConditionType.HasDialogueAnswer, (Farmer player) => (IEnumerable<string>)player.dialogueQuestionsAnswered, save),
			new PerPlayerValueProvider(ConditionType.HasFlag, (Farmer player) => save.GetFlags(player), save),
			new PerPlayerValueProvider(ConditionType.HasProfession, (Farmer player) => ((IEnumerable<int>)player.professions).Select((int id) =>
			{
				Profession profession = (Profession)id;
				return profession.ToString();
			}), save),
			new PerPlayerValueProvider(ConditionType.HasReadLetter, (Farmer player) => (IEnumerable<string>)player.mailReceived, save),
			new PerPlayerValueProvider(ConditionType.HasSeenEvent, (Farmer player) => (IEnumerable<string>)player.eventsSeen, save),
			new PerPlayerValueProvider(ConditionType.HasActiveQuest, (Farmer player) => ((IEnumerable<Quest>)player.questLog).Select((Quest p) => ((NetFieldBase<string, NetString>)(object)p.id).Value), save),
			new PerPlayerValueProvider(ConditionType.HasVisitedLocation, (Farmer player) => (IEnumerable<string>)player.locationsVisited, save),
			new ConditionTypeValueProvider(ConditionType.HasWalletItem, (Func<IEnumerable<string>>)save.GetWalletItems, (Func<bool>?)NeedsSave, false, (IEnumerable<string>?)Enum.GetNames(typeof(WalletItem))),
			new PerPlayerValueProvider(ConditionType.IsMainPlayer, (Farmer player) => player.IsMainPlayer.ToString(), save),
			new PerPlayerValueProvider(ConditionType.IsOutdoors, (Farmer player) =>
			{
				GameLocation? currentLocation = save.GetCurrentLocation(player);
				return (currentLocation != null) ? currentLocation.IsOutdoors.ToString() : null;
			}, save),
			new PerPlayerValueProvider(ConditionType.LocationContext, (Farmer player) => save.GetCurrentLocationContext(player), save),
			new PerPlayerValueProvider(ConditionType.LocationName, (Farmer player) =>
			{
				GameLocation? currentLocation = save.GetCurrentLocation(player);
				return (currentLocation != null) ? currentLocation.Name : null;
			}, save),
			new PerPlayerValueProvider(ConditionType.LocationOwnerId, (Farmer player) => save.GetLocationOwnerId(save.GetCurrentLocation(player))?.ToString(), save),
			new PerPlayerValueProvider(ConditionType.LocationUniqueName, (Farmer player) =>
			{
				GameLocation? currentLocation = save.GetCurrentLocation(player);
				return (currentLocation != null) ? currentLocation.NameOrUniqueName : null;
			}, save),
			new PerPlayerValueProvider(ConditionType.PlayerGender, (Farmer player) =>
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return ((object)(Gender)(!player.IsMale)/*cast due to constrained. prefix*/).ToString();
			}, save),
			new PerPlayerValueProvider(ConditionType.PlayerName, (Farmer player) => ((Character)player).Name, save),
			new ConditionTypeValueProvider(ConditionType.PreferredPet, () =>
			{
				Farmer? currentPlayer = save.GetCurrentPlayer();
				return ((currentPlayer == null || !currentPlayer.catPerson) ? PetType.Dog : PetType.Cat).ToString();
			}, NeedsSave),
			new SkillLevelValueProvider(save),
			new PerPlayerValueProvider(ConditionType.ChildNames, (Farmer player) => save.GetChildValues(player, ConditionType.ChildNames), save),
			new PerPlayerValueProvider(ConditionType.ChildGenders, (Farmer player) => save.GetChildValues(player, ConditionType.ChildGenders), save),
			new VillagerHeartsValueProvider(save),
			new VillagerRelationshipValueProvider(save),
			new PerPlayerValueProvider(ConditionType.Roommate, (Farmer player) => player.hasRoommate() ? player.spouse : null, save),
			new PerPlayerValueProvider(ConditionType.Spouse, (Farmer player) => (!player.hasRoommate()) ? player.spouse : null, save),
			new ConditionTypeValueProvider(ConditionType.FarmCave, () => save.GetFarmCaveType().ToString(), NeedsSave),
			new FarmMapAssetValueProvider(save),
			new ConditionTypeValueProvider(ConditionType.FarmName, (Func<string?>)save.GetFarmName, (Func<bool>?)NeedsSave, false, (IEnumerable<string>?)null),
			new ConditionTypeValueProvider(ConditionType.FarmType, () => save.GetFarmType(), NeedsSave),
			new ConditionTypeValueProvider(ConditionType.IsCommunityCenterComplete, () => save.GetIsCommunityCenterComplete().ToString(), NeedsSave),
			new ConditionTypeValueProvider(ConditionType.IsJojaMartComplete, () => save.GetIsJojaMartComplete().ToString(), NeedsSave),
			new HavingChildValueProvider(ConditionType.Pregnant, save),
			new HavingChildValueProvider(ConditionType.HavingChild, save),
			new CountValueProvider(),
			new QueryValueProvider(),
			new RandomValueProvider(),
			new RangeValueProvider(),
			new RoundValueProvider(),
			new LetterCaseValueProvider(ConditionType.Lowercase),
			new LetterCaseValueProvider(ConditionType.Uppercase),
			new MergeValueProvider(),
			new PathPartValueProvider(),
			new RenderValueProvider(),
			new ImmutableValueProvider("HasMod", installedMods, null, true),
			new HasValueValueProvider(),
			new ConditionTypeValueProvider(ConditionType.Language, () => GetLanguage(contentHelper)),
			new FormatAssetNameValueProvider()
		});
		bool NeedsSave()
		{
			return IsSaveParsed;
		}
	}

	private IEnumerable<IValueProvider> GetLocalValueProviders(IContentPack contentPack)
	{
		InvariantSet invariantSet = new InvariantSet(contentPack.Manifest.UniqueID);
		return new _003C_003Ez__ReadOnlyArray<IValueProvider>(new IValueProvider[6]
		{
			new AbsoluteFilePathValueProvider(contentPack.DirectoryPath),
			new ImmutableValueProvider("ModId", invariantSet, invariantSet),
			new FirstValidFileValueProvider((Func<string, bool>)contentPack.HasFile),
			new HasFileValueProvider((Func<string, bool>)contentPack.HasFile),
			new InternalAssetKeyValueProvider((Func<string, IAssetName>)contentPack.ModContent.GetInternalAssetName),
			new TranslationValueProvider(contentPack.Translation)
		});
	}

	private IEnumerable<string> GetLanguage(IGameContentHelper contentHelper)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		LanguageCode currentLocaleConstant = contentHelper.CurrentLocaleConstant;
		string text = ((object)currentLocaleConstant/*cast due to constrained. prefix*/).ToString();
		if ((int)currentLocaleConstant == 13)
		{
			text = contentHelper.CurrentLocale ?? text;
		}
		return new _003C_003Ez__ReadOnlySingleElementList<string>(text);
	}
}
