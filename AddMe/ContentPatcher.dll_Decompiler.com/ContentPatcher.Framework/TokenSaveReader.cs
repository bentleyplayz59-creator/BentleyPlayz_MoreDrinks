using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Util;

namespace ContentPatcher.Framework;

internal class TokenSaveReader
{
	private readonly Func<int> GetUpdateTick;

	private readonly Func<bool> IsParsedImpl;

	private readonly Func<bool> IsBasicInfoLoadedImpl;

	private readonly Func<bool> IsLoadedImpl;

	private readonly Dictionary<string, object?> Cache = new Dictionary<string, object>();

	private int LastCacheTick;

	private bool AreLocationsLoaded => IsLoaded || (IsBasicInfoLoaded && SaveGame.loaded == null);

	public bool IsReady => IsParsed;

	public bool IsParsed => IsParsedImpl();

	public bool IsBasicInfoLoaded => IsBasicInfoLoadedImpl();

	public bool IsLoaded => IsLoadedImpl();

	public TokenSaveReader(Func<int> updateTick, Func<bool> isParsed, Func<bool> isBasicInfoLoaded, Func<bool> isLoaded)
	{
		GetUpdateTick = updateTick;
		IsParsedImpl = isParsed;
		IsBasicInfoLoadedImpl = isBasicInfoLoaded;
		IsLoadedImpl = isLoaded;
	}

	public Farmer? GetPlayer(PlayerType type)
	{
		if (IsBasicInfoLoaded)
		{
			return (type == PlayerType.HostPlayer) ? Game1.MasterPlayer : Game1.player;
		}
		if (IsParsed)
		{
			return SaveGame.loaded.player;
		}
		return null;
	}

	public Farmer? GetCurrentPlayer()
	{
		return GetPlayer(PlayerType.CurrentPlayer);
	}

	public IEnumerable<Farmer> GetAllPlayers()
	{
		return GetCached("AllPlayers", () =>
		{
			if (IsBasicInfoLoaded)
			{
				return Game1.getAllFarmers();
			}
			return IsParsed ? ((IEnumerable<Farmer>)new Farmer[1] { SaveGame.loaded.player }).Concat((IEnumerable<Farmer>)SaveGame.loaded.farmhands) : Array.Empty<Farmer>();
		});
	}

	public GameLocation? GetCurrentLocation(Farmer? player)
	{
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
		defaultInterpolatedStringHandler.AppendLiteral("CurrentLocation:");
		Farmer? val = player;
		defaultInterpolatedStringHandler.AppendFormatted((val != null) ? new long?(val.UniqueMultiplayerID) : ((long?)null));
		return GetCached(defaultInterpolatedStringHandler.ToStringAndClear(), (Func<GameLocation>)(() =>
		{
			if (Context.IsWorldReady)
			{
				Farmer? val2 = player;
				return (val2 != null) ? ((Character)val2).currentLocation : null;
			}
			if (AreLocationsLoaded)
			{
				Farmer? val3 = player;
				return ((val3 != null) ? ((Character)val3).currentLocation : null) ?? ((player != null) ? GetLocationFromName(((NetFieldBase<string, NetString>)(object)player.lastSleepLocation).Value) : null);
			}
			return IsParsed ? ((player != null) ? GetLocationFromName(((NetFieldBase<string, NetString>)(object)player.lastSleepLocation).Value) : null) : null;
		}));
	}

	public string GetCurrentLocationContext(Farmer? player)
	{
		return GetLocationContext(GetCurrentLocation(player));
	}

	public long? GetLocationOwnerId(GameLocation? location)
	{
		if (location == null)
		{
			return null;
		}
		long? cached = GetCached("LocationOwnerId:" + location.Name, () =>
		{
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			GameLocation val = location;
			GameLocation val2 = val;
			FarmHouse val3 = (FarmHouse)(object)((val2 is FarmHouse) ? val2 : null);
			if (val3 != null)
			{
				Farmer owner = val3.owner;
				return (owner != null) ? new long?(owner.UniqueMultiplayerID) : ((long?)null);
			}
			if (val2 is IslandFarmHouse)
			{
				Farmer? player = GetPlayer(PlayerType.HostPlayer);
				return (player != null) ? new long?(player.UniqueMultiplayerID) : ((long?)null);
			}
			if (!(val2 is Cellar))
			{
				if (!location.IsOutdoors)
				{
					IDictionary<GameLocation, long> buildingInteriorOwners = GetBuildingInteriorOwners();
					if (buildingInteriorOwners.TryGetValue(location, out var value))
					{
						return value;
					}
				}
				return (long?)null;
			}
			int result = 1;
			if (location.Name.StartsWith("Cellar"))
			{
				if (location.Name.Length > 6)
				{
					string name = location.Name;
					if (!int.TryParse(name.Substring(6, name.Length - 6), out result))
					{
						goto IL_00df;
					}
				}
				long value2 = default;
				if (IsBasicInfoLoaded)
				{
					return ((NetDictionary<int, long, NetLong, SerializableDictionary<int, long>, NetIntDictionary<long, NetLong>>)(object)Game1.player.team.cellarAssignments).TryGetValue(result, ref value2) ? new long?(value2) : ((long?)null);
				}
				if (IsParsed)
				{
					SaveablePair<int, long>[] cellarAssignments = SaveGame.loaded.cellarAssignments;
					for (int i = 0; i < cellarAssignments.Length; i++)
					{
						SaveablePair<int, long> val4 = cellarAssignments[i];
						if (val4.Key == result)
						{
							return val4.Value;
						}
					}
				}
				return (long?)null;
			}
			goto IL_00df;
			IL_00df:
			return (long?)null;
		});
		return (cached != 0) ? cached : ((long?)null);
	}

	public int GetDay()
	{
		if (IsBasicInfoLoaded)
		{
			return Game1.dayOfMonth;
		}
		if (IsParsed)
		{
			return SaveGame.loaded.dayOfMonth;
		}
		return 0;
	}

	public string? GetDayEvent()
	{
		if (SaveGame.loaded?.weddingToday ?? Game1.weddingToday)
		{
			return "wedding";
		}
		IDictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates", (LanguageCode)0);
		if (dictionary.TryGetValue($"{GetSeason()}{GetDay()}", out var value))
		{
			return value;
		}
		return null;
	}

	public DayOfWeek GetDayOfWeek()
	{
		int num = GetDay() % 7;
		if (1 == 0)
		{
		}
		DayOfWeek result = num switch
		{
			1 => DayOfWeek.Monday, 
			2 => DayOfWeek.Tuesday, 
			3 => DayOfWeek.Wednesday, 
			4 => DayOfWeek.Thursday, 
			5 => DayOfWeek.Friday, 
			6 => DayOfWeek.Saturday, 
			_ => DayOfWeek.Sunday, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public uint GetDaysPlayed()
	{
		if (IsBasicInfoLoaded)
		{
			return Game1.stats.DaysPlayed;
		}
		if (IsParsed)
		{
			return (SaveGame.loaded.obsolete_stats ?? SaveGame.loaded.player.stats).DaysPlayed;
		}
		return 0u;
	}

	public string GetSeason()
	{
		if (IsBasicInfoLoaded)
		{
			return Game1.currentSeason;
		}
		if (IsParsed)
		{
			return SaveGame.loaded.currentSeason;
		}
		return "spring";
	}

	public int GetYear()
	{
		if (IsBasicInfoLoaded)
		{
			return Game1.year;
		}
		if (IsParsed)
		{
			return SaveGame.loaded.year;
		}
		return 0;
	}

	public HashSet<string> GetContexts()
	{
		return GetCached("Contexts", () =>
		{
			HashSet<string> hashSet = new HashSet<string> { "Default", "Desert", "Island" };
			foreach (GameLocation location in GetLocations())
			{
				hashSet.Add(GetLocationContext(location));
			}
			return hashSet;
		});
	}

	public string? GetWeather(string contextName)
	{
		LocationWeather value = default;
		string text = ((NetFieldBase<string, NetString>)(object)(IsBasicInfoLoaded ? ((NetFieldBase<NetWorldState, NetRef<NetWorldState>>)(object)Game1.netWorldState).Value.GetWeatherForLocation(contextName) : ((!IsParsed) ? null : ((((Dictionary<string, LocationWeather>)(object)SaveGame.loaded.locationWeather)?.TryGetValue(contextName, out value) ?? false) ? value : null)))?.weather)?.Value;
		if (1 == 0)
		{
		}
		string result = text switch
		{
			"Festival" => "Sun", 
			"Wedding" => "Sun", 
			null => "Sun", 
			_ => text, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public int GetTime()
	{
		return IsBasicInfoLoaded ? Game1.timeOfDay : 600;
	}

	public IEnumerable<string> GetFlags(Farmer player)
	{
		return Enumerable.Concat(second: IsBasicInfoLoaded ? (Game1.worldStateIDs ?? new HashSet<string>()) : ((!IsParsed) ? ((IEnumerable<string>)Array.Empty<string>()) : ((IEnumerable<string>)(SaveGame.loaded.worldStateIDs ?? new HashSet<string>()))), first: ((IEnumerable<string>)player.mailReceived).Union((IEnumerable<string>)player.mailForTomorrow).Union((IEnumerable<string>)player.mailbox));
	}

	public IEnumerable<string> GetWalletItems()
	{
		Farmer player = GetCurrentPlayer();
		if (player != null)
		{
			if (((NetHashSet<string>)(object)player.eventsSeen).Contains("2120303"))
			{
				yield return "BearsKnowledge";
			}
			if (player.hasClubCard)
			{
				yield return "ClubCard";
			}
			if (player.hasDarkTalisman)
			{
				yield return "DarkTalisman";
			}
			if (player.canUnderstandDwarves)
			{
				yield return "DwarvishTranslationGuide";
			}
			if (player.HasTownKey)
			{
				yield return "KeyToTheTown";
			}
			if (player.hasMagicInk)
			{
				yield return "MagicInk";
			}
			if (player.hasMagnifyingGlass)
			{
				yield return "MagnifyingGlass";
			}
			if (player.hasRustyKey)
			{
				yield return "RustyKey";
			}
			if (player.hasSkullKey)
			{
				yield return "SkullKey";
			}
			if (player.hasSpecialCharm)
			{
				yield return "SpecialCharm";
			}
			if (((NetHashSet<string>)(object)player.eventsSeen).Contains("3910979"))
			{
				yield return "SpringOnionMastery";
			}
		}
	}

	public double GetDailyLuck(Farmer player)
	{
		if (IsBasicInfoLoaded)
		{
			return player.DailyLuck;
		}
		if (IsParsed)
		{
			return SaveGame.loaded.dailyLuck + (double)(player.hasSpecialCharm ? 0.025f : 0f);
		}
		return 0.0;
	}

	public IEnumerable<string> GetChildValues(Farmer player, ConditionType type)
	{
		List<Child> cached = GetCached($"ChildValues:{type}:{player.UniqueMultiplayerID}", () =>
		{
			GameLocation? locationFromName = GetLocationFromName(((NetFieldBase<string, NetString>)(object)player.homeLocation).Value);
			GameLocation? obj = ((locationFromName is FarmHouse) ? locationFromName : null);
			return (obj != null) ? ((FarmHouse)obj).getChildren() : null;
		});
		if (cached == null)
		{
			return Array.Empty<string>();
		}
		if (1 == 0)
		{
		}
		Func<Child, string> func = type switch
		{
			ConditionType.ChildNames => (Func<Child, string>)((Child child) => ((Character)child).Name), 
			ConditionType.ChildGenders => (Child child) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return ((object)((Character)child).Gender/*cast due to constrained. prefix*/).ToString();
			}, 
			_ => throw new NotSupportedException($"Invalid child token type '{type}', must be one of '{"ChildGenders"}' or '{"ChildNames"}'."), 
		};
		if (1 == 0)
		{
		}
		Func<Child, string> selector = func;
		return cached.Select(selector);
	}

	public IEnumerable<KeyValuePair<string, Friendship?>> GetFriendships()
	{
		return GetCached("Friendships", () =>
		{
			Dictionary<string, NetRef<Friendship>> met = ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)GetCurrentPlayer()?.friendshipData)?.FieldDict ?? new Dictionary<string, NetRef<Friendship>>();
			return met.Select((KeyValuePair<string, NetRef<Friendship>> pair) =>
			{
				KeyValuePair<string, NetRef<Friendship>> keyValuePair = pair;
				string key = keyValuePair.Key;
				keyValuePair = pair;
				return new KeyValuePair<string, Friendship>(key, ((NetFieldBase<Friendship, NetRef<Friendship>>)(object)keyValuePair.Value).Value);
			}).Concat(from name in GetSocialVillagerNames()
				where !met.ContainsKey(name)
				select new KeyValuePair<string, Friendship>(name, null));
		});
	}

	public bool TryGetSpouseInfo(Farmer player, [NotNullWhen(true)] out string? name, [NotNullWhen(true)] out Friendship? friendship, out Gender gender, out bool isPlayer)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected I4, but got Unknown
		(string, Friendship, Gender, bool, bool) cached = GetCached($"SpouseInfo:{player.UniqueMultiplayerID}", () =>
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			string text = null;
			Gender item = (Gender)0;
			bool item2 = false;
			if (TryGetRawSpouseInfo(player, out Friendship friendship2, out long? spousePlayerId))
			{
				if (spousePlayerId.HasValue)
				{
					Farmer val = GetAllPlayers().FirstOrDefault((Farmer p) => p.UniqueMultiplayerID == spousePlayerId);
					if (val != null)
					{
						text = ((Character)val).Name;
						item = (Gender)(!val.IsMale);
						item2 = true;
					}
				}
				else
				{
					NPC val2 = GetAllCharacters().FirstOrDefault((NPC p) => ((Character)p).Name == player.spouse && ((Character)p).IsVillager);
					if (val2 != null)
					{
						text = ((Character)val2).Name;
						item = ((Character)val2).Gender;
						item2 = false;
					}
				}
			}
			bool item3 = text != null && friendship2 != null;
			return (Name: text, Friendship: friendship2, Gender: item, IsPlayer: item2, IsValid: item3);
		});
		name = cached.Item1;
		friendship = cached.Item2;
		gender = (Gender)(int)cached.Item3;
		isPlayer = cached.Item4;
		return cached.Item5;
	}

	public FarmCaveType GetFarmCaveType()
	{
		int? num = ((NetFieldBase<int, NetInt>)(object)GetCurrentPlayer()?.caveChoice).Value;
		return num.HasValue ? GetEnum(num.Value, FarmCaveType.None) : FarmCaveType.None;
	}

	public string? GetFarmName()
	{
		return ((NetFieldBase<string, NetString>)(object)GetCurrentPlayer()?.farmName).Value;
	}

	public string GetFarmType()
	{
		if (IsBasicInfoLoaded)
		{
			return (Game1.whichFarm != 7) ? GetEnum(Game1.whichFarm, FarmType.Custom).ToString() : (Game1.whichModFarm?.Id ?? "Custom");
		}
		if (IsParsed)
		{
			string text = SaveGame.loaded.whichFarm;
			if (Enum.TryParse<FarmType>(text, out var result))
			{
				text = result.ToString();
			}
			return text;
		}
		return "Standard";
	}

	public string GetFarmMapAssetName()
	{
		if (IsBasicInfoLoaded)
		{
			return Farm.getMapNameFromTypeInt(Game1.whichFarm);
		}
		if (IsParsed)
		{
			string whichFarm = SaveGame.loaded.whichFarm;
			FarmType farmType = default;
			if (Utility.TryParseEnum<FarmType>(whichFarm, ref farmType) && farmType != FarmType.Custom)
			{
				return Farm.getMapNameFromTypeInt((int)farmType);
			}
			foreach (ModFarmType item in DataLoader.AdditionalFarms(Game1.content))
			{
				if (item?.Id == whichFarm)
				{
					return item.MapName;
				}
			}
		}
		return "Farm";
	}

	public bool GetIsCommunityCenterComplete()
	{
		Farmer player = GetPlayer(PlayerType.HostPlayer);
		return (player != null && ((NetHashSet<string>)(object)player.mailReceived).Contains("ccIsComplete")) || (player != null && player.hasCompletedCommunityCenter());
	}

	public bool GetIsJojaMartComplete()
	{
		Farmer player = GetPlayer(PlayerType.HostPlayer);
		bool? flag = ((player != null) ? new bool?(((IEnumerable<string>)player.mailReceived).Any((string text) =>
		{
			switch (text)
			{
			case "jojaVault":
			case "jojaPantry":
			case "jojaBoilerRoom":
			case "jojaCraftsRoom":
			case "jojaFishTank":
			case "JojaMember":
				return true;
			default:
				return false;
			}
		})) : ((bool?)null));
		return flag.HasValue && flag == true && ((NetHashSet<string>)(object)player.mailReceived).Contains("ccVault") && ((NetHashSet<string>)(object)player.mailReceived).Contains("ccPantry") && ((NetHashSet<string>)(object)player.mailReceived).Contains("ccBoilerRoom") && ((NetHashSet<string>)(object)player.mailReceived).Contains("ccCraftsRoom") && ((NetHashSet<string>)(object)player.mailReceived).Contains("ccFishTank");
	}

	private GameLocation? GetLocationFromName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return null;
		}
		return GetCached("Location:" + name, (Func<GameLocation>)(() =>
		{
			if (AreLocationsLoaded)
			{
				return Game1.getLocationFromName(name);
			}
			return IsParsed ? SaveGame.loaded.locations.FirstOrDefault((GameLocation p) => p.NameOrUniqueName == name) : null;
		}));
	}

	private IEnumerable<GameLocation> GetLocations()
	{
		return GetCached("Locations", () =>
		{
			if (AreLocationsLoaded)
			{
				return CommonHelper.GetLocations();
			}
			return IsParsed ? SaveGame.loaded.locations.Concat(from location in SaveGame.loaded.locations
				from building in (IEnumerable<Building>)location.buildings
				where ((NetFieldBase<GameLocation, NetRef<GameLocation>>)(object)building.indoors).Value != null
				select ((NetFieldBase<GameLocation, NetRef<GameLocation>>)(object)building.indoors).Value) : Array.Empty<GameLocation>();
		});
	}

	private string GetLocationContext(GameLocation? location)
	{
		if (Context.IsWorldReady)
		{
			return ((location != null) ? location.GetLocationContextId() : null) ?? "Default";
		}
		if (IsBasicInfoLoaded && location?.locationContextId != null)
		{
			return location.locationContextId;
		}
		bool flag = ((location is IslandLocation || location is IslandFarmHouse) ? true : false);
		return flag ? "Island" : "Default";
	}

	private IDictionary<GameLocation, long> GetBuildingInteriorOwners()
	{
		return GetCached("InteriorOwners", () =>
		{
			Dictionary<GameLocation, long> dictionary = new Dictionary<GameLocation, long>();
			GameLocation? locationFromName = GetLocationFromName("Farm");
			Farm val = (Farm)(object)((locationFromName is Farm) ? locationFromName : null);
			if (val != null)
			{
				foreach (Building building in ((GameLocation)val).buildings)
				{
					GameLocation indoors = building.GetIndoors();
					long value = ((NetFieldBase<long, NetLong>)(object)building.owner).Value;
					if (indoors != null && value != 0)
					{
						dictionary[indoors] = value;
					}
				}
			}
			return dictionary;
		});
	}

	private ISet<string> GetSocialVillagerNames()
	{
		return GetCached("SocialVillagerNames", () =>
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (Game1.characterData != null)
			{
				foreach (var (item, val2) in Game1.characterData)
				{
					if (val2 != null && GameStateQuery.CheckConditions(val2.CanSocialize, (GameLocation)null, (Farmer)null, (Item)null, (Item)null, (Random)null, (HashSet<string>)null))
					{
						hashSet.Add(item);
					}
				}
			}
			foreach (NPC allCharacter in GetAllCharacters())
			{
				if (!hashSet.Contains(((Character)allCharacter).Name) && allCharacter.CanSocialize)
				{
					hashSet.Add(((Character)allCharacter).Name);
				}
			}
			return hashSet;
		});
	}

	private IEnumerable<NPC> GetAllCharacters()
	{
		return GetCached("AllCharacters", () => GetLocations().SelectMany((GameLocation p) => (IEnumerable<NPC>)p.characters).Distinct(new ObjectReferenceComparer<NPC>()));
	}

	private bool TryGetRawSpouseInfo(Farmer player, [NotNullWhen(true)] out Friendship? friendship, out long? playerSpouseId)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (IsBasicInfoLoaded)
		{
			playerSpouseId = player.team.GetSpouse(player.UniqueMultiplayerID);
			friendship = player.GetSpouseFriendship();
			return friendship != null;
		}
		if (IsParsed)
		{
			SaveablePair<FarmerPair, Friendship>[] farmerFriendships = SaveGame.loaded.farmerFriendships;
			for (int i = 0; i < farmerFriendships.Length; i++)
			{
				SaveablePair<FarmerPair, Friendship> val = farmerFriendships[i];
				FarmerPair key = val.Key;
				if (key.Contains(player.UniqueMultiplayerID) && (val.Value.IsEngaged() || val.Value.IsMarried()))
				{
					key = val.Key;
					playerSpouseId = key.GetOther(player.UniqueMultiplayerID);
					friendship = val.Value;
					return true;
				}
			}
			playerSpouseId = null;
			string spouse = player.spouse;
			bool flag = ((spouse == null || spouse == "") ? true : false);
			Friendship val2 = default;
			friendship = ((!flag && ((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)player.friendshipData).TryGetValue(player.spouse, ref val2)) ? val2 : null);
			return friendship != null;
		}
		playerSpouseId = null;
		friendship = null;
		return false;
	}

	private TEnum GetEnum<TEnum>(int value, TEnum defaultValue)
	{
		return Enum.IsDefined(typeof(TEnum), value) ? ((TEnum)(object)value) : defaultValue;
	}

	private TValue GetCached<TValue>(string key, Func<TValue> fetch)
	{
		if (LastCacheTick != GetUpdateTick())
		{
			Cache.Clear();
			LastCacheTick = GetUpdateTick();
		}
		if (Cache.TryGetValue(key, out object value))
		{
			object obj = value;
			if (1 == 0)
			{
			}
			TValue result;
			if (!(obj is TValue val))
			{
				if (obj != null || default(TValue) != null)
				{
					throw new InvalidOperationException($"Can't fetch cached save data for the '{key}' cache key; requested a {typeof(TValue).FullName} value, but the cache contains {((value == null) ? "null" : ("'" + value.GetType().FullName + "'"))} instead.");
				}
				result = default;
			}
			else
			{
				result = val;
			}
			if (1 == 0)
			{
			}
			return result;
		}
		TValue val2 = fetch();
		Cache[key] = val2;
		return val2;
	}
}
