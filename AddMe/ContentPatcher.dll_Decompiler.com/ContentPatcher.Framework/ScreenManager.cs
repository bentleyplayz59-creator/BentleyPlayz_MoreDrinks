using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Locations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using ContentPatcher.Framework.Validators;
using Pathoschild.Stardew.Common.Integrations.Profiler;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.Logging;
using StardewValley;

namespace ContentPatcher.Framework;

internal class ScreenManager
{
	private readonly IModHelper Helper;

	private readonly IMonitor Monitor;

	private readonly HashSet<LoadedContentPack> QueuedForConfigUpdates = new HashSet<LoadedContentPack>();

	private bool IsFirstTick = true;

	public PatchLoader PatchLoader { get; }

	public TokenManager TokenManager { get; }

	public PatchManager PatchManager { get; }

	public CustomLocationManager CustomLocationManager { get; }

	public bool IsInitialized { get; private set; }

	public ScreenManager(IModHelper helper, IMonitor monitor, IInvariantSet installedMods, ModProvidedToken[] modTokens, IAssetValidator[] assetValidators, ProfilerIntegration profiler)
	{
		Helper = helper;
		Monitor = monitor;
		TokenManager = new TokenManager(helper.GameContent, installedMods, modTokens);
		PatchManager = new PatchManager(Monitor, TokenManager, assetValidators, profiler);
		PatchLoader = new PatchLoader(PatchManager, TokenManager, Monitor, installedMods, (Func<string, IAssetName>)helper.GameContent.ParseAssetName);
		CustomLocationManager = new CustomLocationManager(Monitor, helper.GameContent);
	}

	public void Initialize(LoadedContentPack[] contentPacks, IInvariantSet installedMods)
	{
		if (IsInitialized)
		{
			Monitor.Log($"{"ScreenManager"}.{"Initialize"} was called more than once for screen {Context.ScreenId}.", (LogLevel)4);
		}
		IsInitialized = true;
		UpdateContext(ContextUpdateType.All);
		LoadContentPacks(contentPacks, installedMods);
		UpdateContext(ContextUpdateType.All);
		CustomLocationManager.OnScreenInitialized();
	}

	public void OnAssetRequested(AssetRequestedEventArgs e)
	{
		bool ignoreLoadPatches = CustomLocationManager.OnAssetRequested(e);
		PatchManager.OnAssetRequested(e, ignoreLoadPatches);
	}

	public void OnLoadStageChanged(LoadStage oldStage, LoadStage newStage)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected I4, but got Unknown
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = newStage - 10 <= 1;
		if (flag)
		{
			CustomLocationManager.AddTmxlLocations(SaveGame.loaded?.locations, Game1.locations);
		}
		switch ((int)newStage)
		{
		case 1:
		case 4:
		case 5:
		{
			IMonitor monitor2 = Monitor;
			IMonitor val3 = monitor2;
			VerboseLogStringHandler val4 = new VerboseLogStringHandler(41, 1, monitor2, ref flag);
			if (flag)
			{
				val4.AppendLiteral("Updating context: load stage changed to ");
				val4.AppendFormatted<LoadStage>(newStage);
				val4.AppendLiteral(".");
			}
			val3.VerboseLog(ref val4);
			TokenManager.IsSaveParsed = true;
			TokenManager.IsSaveBasicInfoLoaded = (int)newStage != 4;
			UpdateContext(ContextUpdateType.All);
			break;
		}
		case 7:
		case 8:
		{
			IMonitor monitor3 = Monitor;
			IMonitor val5 = monitor3;
			VerboseLogStringHandler val6 = new VerboseLogStringHandler(41, 1, monitor3, ref flag);
			if (flag)
			{
				val6.AppendLiteral("Updating context: load stage changed to ");
				val6.AppendFormatted<LoadStage>(newStage);
				val6.AppendLiteral(".");
			}
			val5.VerboseLog(ref val6);
			if (!TokenManager.IsSaveLoaded)
			{
				TokenManager.IsSaveParsed = true;
				TokenManager.IsSaveBasicInfoLoaded = true;
				TokenManager.IsSaveLoaded = true;
				UpdateContext(ContextUpdateType.All);
			}
			break;
		}
		case 0:
		case 12:
		{
			IMonitor monitor = Monitor;
			IMonitor val = monitor;
			VerboseLogStringHandler val2 = new VerboseLogStringHandler(41, 1, monitor, ref flag);
			if (flag)
			{
				val2.AppendLiteral("Updating context: load stage changed to ");
				val2.AppendFormatted<LoadStage>(newStage);
				val2.AppendLiteral(".");
			}
			val.VerboseLog(ref val2);
			TokenManager.IsSaveParsed = false;
			TokenManager.IsSaveBasicInfoLoaded = false;
			TokenManager.IsSaveLoaded = false;
			UpdateContext(ContextUpdateType.All);
			break;
		}
		case 2:
		case 3:
		case 6:
		case 9:
		case 10:
		case 11:
			break;
		}
	}

	public void OnDayStarted()
	{
		Monitor.VerboseLog("Updating context: new day started.");
		TokenManager.IsSaveParsed = true;
		TokenManager.IsSaveBasicInfoLoaded = true;
		UpdateContext(ContextUpdateType.All);
	}

	public void OnTimeChanged()
	{
		Monitor.VerboseLog("Updating context: clock changed.");
		UpdateContext(ContextUpdateType.OnTimeChange);
	}

	public void OnWarped()
	{
		Monitor.VerboseLog("Updating context: player warped.");
		UpdateContext(ContextUpdateType.OnLocationChange);
	}

	public void OnUpdateTicked()
	{
		IsFirstTick = false;
		if (!QueuedForConfigUpdates.Any())
		{
			return;
		}
		foreach (LoadedContentPack queuedForConfigUpdate in QueuedForConfigUpdates)
		{
			ApplyNewConfig(queuedForConfigUpdate);
		}
		QueuedForConfigUpdates.Clear();
	}

	public void OnLocaleChanged()
	{
		if (!IsFirstTick)
		{
			UpdateContext(ContextUpdateType.All);
		}
	}

	public void OnContentPackConfigChanged(LoadedContentPack contentPack)
	{
		QueuedForConfigUpdates.Add(contentPack);
	}

	public void UpdateContext(ContextUpdateType updateType)
	{
		TokenManager.UpdateContext(out IInvariantSet changedGlobalTokens);
		PatchManager.UpdateContext(Helper.GameContent, changedGlobalTokens, updateType);
	}

	private void LoadContentPacks(IEnumerable<LoadedContentPack> contentPacks, IInvariantSet installedMods)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		bool flag = default;
		foreach (LoadedContentPack contentPack in contentPacks)
		{
			LoadedContentPack current = contentPack;
			IMonitor monitor = Monitor;
			IMonitor val = monitor;
			VerboseLogStringHandler val2 = new VerboseLogStringHandler(26, 1, monitor, ref flag);
			if (flag)
			{
				val2.AppendLiteral("Loading content pack '");
				val2.AppendFormatted<string>(current.Manifest.Name);
				val2.AppendLiteral("'...");
			}
			val.VerboseLog(ref val2);
			try
			{
				ContentConfig content = current.Content;
				InvariantDictionary<ConfigField> config = current.Config;
				ModTokenContext modTokenContext = TokenManager.TrackLocalTokens(current.ContentPack);
				TokenParser tokenParser = new TokenParser(modTokenContext, current.Manifest, current.Migrator, installedMods);
				if (config.Any())
				{
					IMonitor monitor2 = Monitor;
					IMonitor val3 = monitor2;
					VerboseLogStringHandler val4 = new VerboseLogStringHandler(36, 1, monitor2, ref flag);
					if (flag)
					{
						val4.AppendLiteral("   found config.json with ");
						val4.AppendFormatted<int>(config.Count);
						val4.AppendLiteral(" fields...");
					}
					val3.VerboseLog(ref val4);
				}
				foreach (KeyValuePair<string, ConfigField> item in config)
				{
					AddConfigToken(item.Key, item.Value, modTokenContext, current);
				}
				IDictionary<string, int> dictionary = new InvariantDictionary<int>();
				DynamicTokenConfig[] dynamicTokens = content.DynamicTokens;
				string key;
				ConditionType result;
				foreach (DynamicTokenConfig entry in dynamicTokens)
				{
					LogPathBuilder logPathBuilder = current.LogPath.With("DynamicTokens");
					string text = (string.IsNullOrWhiteSpace(entry.Name) ? "unnamed" : entry.Name);
					dictionary.TryAdd(text, -1);
					key = text;
					int value = ++dictionary[key];
					logPathBuilder = logPathBuilder.With($"{entry.Name} {value}");
					if (string.IsNullOrWhiteSpace(entry.Name))
					{
						LogSkip("the token name can't be empty.");
						continue;
					}
					if (entry.Name.Contains(":"))
					{
						LogSkip("the token name can't have positional input arguments (: character).");
						continue;
					}
					if (Enum.TryParse<ConditionType>(entry.Name, ignoreCase: true, out result))
					{
						LogSkip("the token name is already used by a global token.");
						continue;
					}
					if (config.ContainsKey(entry.Name))
					{
						LogSkip("the token name is already used by a config token.");
						continue;
					}
					if (!PatchLoader.TryParseConditions(entry.When, tokenParser, logPathBuilder.With("When"), out Condition[] conditions, out IInvariantSet immutableRequiredModIds, out string error))
					{
						Monitor.Log($"Ignored {current.Manifest.Name} > '{entry.Name}' token: its {"When"} field is invalid: {error}.", (LogLevel)3);
						continue;
					}
					IManagedTokenString parsed;
					if (!string.IsNullOrWhiteSpace(entry.Value))
					{
						if (!tokenParser.TryParseString(entry.Value, immutableRequiredModIds, logPathBuilder.With("Value"), out string error2, out parsed))
						{
							LogSkip("the token value is invalid: " + error2);
							continue;
						}
					}
					else
					{
						parsed = new LiteralString("", logPathBuilder.With("Value"));
					}
					modTokenContext.AddDynamicToken(entry.Name, parsed, conditions);
					void LogSkip(string reason)
					{
						Monitor.Log($"Ignored {current.Manifest.Name} > dynamic token '{entry.Name}': {reason}", (LogLevel)3);
					}
				}
				InvariantDictionary<string> invariantDictionary = new InvariantDictionary<string>();
				string value2;
				foreach (KeyValuePair<string, string> aliasTokenName in content.AliasTokenNames)
				{
					aliasTokenName.Deconstruct(out key, out value2);
					string text2 = key;
					string text3 = value2;
					invariantDictionary[text2.Trim()] = text3?.Trim();
				}
				foreach (KeyValuePair<string, string> item2 in invariantDictionary)
				{
					item2.Deconstruct(out value2, out key);
					string key2 = value2;
					string text4 = key;
					if (string.IsNullOrWhiteSpace(key2))
					{
						LogSkip2("the alias can't be blank.");
					}
					else if (string.IsNullOrWhiteSpace(text4))
					{
						LogSkip2("the target value can't be blank.");
					}
					else if (invariantDictionary.ContainsKey(text4))
					{
						LogSkip2("you can't create an alias which targets another alias.");
					}
					else if (Enum.TryParse<ConditionType>(key2, ignoreCase: true, out result))
					{
						LogSkip2("you can't create an alias with the same name as a global token.");
					}
					else if (config.ContainsKey(key2))
					{
						LogSkip2("you can't create an alias with the same name as a config token.");
					}
					else
					{
						modTokenContext.AddAliasTokenName(key2, text4);
					}
					void LogSkip2(string reason)
					{
						Monitor.Log($"Ignored {current.Manifest.Name} > alias token name '{key2}': {reason}", (LogLevel)3);
					}
				}
				PatchLoader.LoadPatches(current, content.Changes, null, new int[1] { current.Index }, current.LogPath, null);
				CustomLocationConfig[] customLocations = content.CustomLocations;
				foreach (CustomLocationConfig customLocationConfig in customLocations)
				{
					if (!CustomLocationManager.TryAddCustomLocationConfig(customLocationConfig, current.ContentPack, out string error3))
					{
						Monitor.Log($"Ignored {current.Manifest.Name} > custom location '{customLocationConfig?.Name}': {error3}", (LogLevel)3);
					}
				}
			}
			catch (Exception value3)
			{
				Monitor.Log($"Error loading content pack '{current.Manifest.Name}'. Technical details:\n{value3}", (LogLevel)4);
			}
		}
		CustomLocationManager.EnforceUniqueNames();
	}

	private void ApplyNewConfig(LoadedContentPack contentPack)
	{
		ModTokenContext modContext = TokenManager.TrackLocalTokens(contentPack.ContentPack);
		foreach (KeyValuePair<string, ConfigField> item in contentPack.Config)
		{
			AddConfigToken(item.Key, item.Value, modContext, contentPack);
		}
		TokenManager.UpdateContext(out IInvariantSet _);
		PatchLoader.UnloadPatchesLoadedBy(contentPack);
		if (!contentPack.TryReloadContent(out string error))
		{
			Monitor.Log($"Failed to reload content pack '{contentPack.Manifest.Name}' for configuration changes: {error}. The content pack may not be in a valid state.", (LogLevel)4);
		}
		else
		{
			PatchLoader.LoadPatches(contentPack, contentPack.Content.Changes, null, new int[1] { contentPack.Index }, contentPack.LogPath, null);
			UpdateContext(ContextUpdateType.All);
		}
	}

	private void AddConfigToken(string name, ConfigField field, ModTokenContext modContext, LoadedContentPack contentPack)
	{
		modContext.RemoveLocalToken(name);
		IValueProvider provider = new ImmutableValueProvider(name, field.Value, field.AllowValues, field.AllowMultiple, isMutable: true);
		IToken token = new Token(provider, contentPack.Manifest.UniqueID);
		modContext.AddLocalToken(token);
	}
}
