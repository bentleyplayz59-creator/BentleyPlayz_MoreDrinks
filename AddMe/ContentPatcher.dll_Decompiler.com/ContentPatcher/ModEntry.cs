using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework;
using ContentPatcher.Framework.Api;
using ContentPatcher.Framework.Commands;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.TriggerActions;
using ContentPatcher.Framework.Validators;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Integrations.Profiler;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;

namespace ContentPatcher;

internal class ModEntry : Mod
{
	private PerScreen<ScreenManager> ScreenManager = null;

	private LoadedContentPack[]? ContentPacks;

	private readonly Func<ContentConfig?, IMigration[]> GetFormatVersions = (ContentConfig? content) => new IMigration[42]
	{
		new EmptyMigration(1, 0),
		new EmptyMigration(1, 1),
		new EmptyMigration(1, 2),
		new Migration_1_3(),
		new Migration_1_4(),
		new Migration_1_5(),
		new Migration_1_6(),
		new Migration_1_7(),
		new Migration_1_8(),
		new Migration_1_9(),
		new Migration_1_10(),
		new Migration_1_11(),
		new EmptyMigration(1, 12),
		new Migration_1_13(),
		new Migration_1_14(),
		new Migration_1_15_Prevalidation(),
		new Migration_1_15_Rewrites(content),
		new Migration_1_16(),
		new Migration_1_17(),
		new Migration_1_18(),
		new Migration_1_19(),
		new Migration_1_20(),
		new Migration_1_21(),
		new Migration_1_22(),
		new Migration_1_23(),
		new Migration_1_24(),
		new Migration_1_25(),
		new Migration_1_26(),
		new EmptyMigration(1, 27),
		new Migration_1_28(),
		new EmptyMigration(1, 29),
		new EmptyMigration(1, 30),
		new Migration_2_0(),
		new Migration_2_1(),
		new Migration_2_2(),
		new EmptyMigration(2, 3),
		new Migration_2_4(),
		new Migration_2_5(),
		new Migration_2_6(),
		new Migration_2_7(),
		new EmptyMigration(2, 8),
		new Migration_2_9()
	};

	private readonly Func<IAssetValidator[]> AssetValidators = () => new IAssetValidator[1]
	{
		new StardewValley_1_3_36_Validator()
	};

	private CommandHandler CommandHandler = null;

	private ModConfig Config = null;

	private readonly PerScreen<DebugOverlay?> DebugOverlay = new PerScreen<DebugOverlay>();

	private readonly List<ModProvidedToken> QueuedModTokens = new List<ModProvidedToken>();

	private int ConditionsApiReadyTick = int.MaxValue;

	private bool IsFirstTick = true;

	private ModConfigKeys Keys => Config.Controls;

	public override void Entry(IModHelper helper)
	{
		CommonHelper.RemoveObsoleteFiles((IMod)(object)this, "ContentPatcher.pdb");
		Config = helper.ReadConfig<ModConfig>();
		ScreenManager = new PerScreen<ScreenManager>((Func<ScreenManager>)CreateScreenManager);
		helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
		helper.Events.Content.LocaleChanged += OnLocaleChanged;
		IModInfo val = helper.ModRegistry.Get("Platonymous.Toolkit");
		EditImagePatch.EnablePyTkLegacyMode = val != null && val.Manifest.Version.IsOlderThan("1.24.0") && typeof(Constants).GetProperty("ExecutionPath") != null;
	}

	public override object GetApi()
	{
		return new ContentPatcherAPI(((Mod)this).ModManifest.UniqueID, ((Mod)this).Monitor, ((Mod)this).Helper.Reflection, AddModToken, () => Game1.ticks >= ConditionsApiReadyTick, ParseConditionsForApi, ParseTokenStringForApi);
	}

	private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
	{
		if (!Config.EnableDebugFeatures)
		{
			return;
		}
		if (Keys.ToggleDebug.JustPressed())
		{
			if (DebugOverlay.Value == null)
			{
				DebugOverlay.Value = new DebugOverlay(((Mod)this).Helper.Events, ((Mod)this).Helper.Input, ((Mod)this).Helper.GameContent, ((Mod)this).Helper.Reflection);
				return;
			}
			DebugOverlay.Value.Dispose();
			DebugOverlay.Value = null;
		}
		else if (DebugOverlay.Value != null)
		{
			if (Keys.DebugPrevTexture.JustPressed())
			{
				DebugOverlay.Value.PrevTexture();
			}
			if (Keys.DebugNextTexture.JustPressed())
			{
				DebugOverlay.Value.NextTexture();
			}
		}
	}

	private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
	{
		ScreenManager.Value.OnAssetRequested(e);
	}

	private void OnLoadStageChanged(object? sender, LoadStageChangedEventArgs e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		ScreenManager.Value.OnLoadStageChanged(e.OldStage, e.NewStage);
	}

	private void OnDayStarted(object? sender, DayStartedEventArgs e)
	{
		ScreenManager.Value.OnDayStarted();
	}

	private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
	{
		ScreenManager.Value.OnTimeChanged();
	}

	private void OnWarped(object? sender, WarpedEventArgs e)
	{
		ScreenManager.Value.OnWarped();
	}

	private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
	{
		if (IsFirstTick)
		{
			IsFirstTick = false;
			Initialize();
			ConditionsApiReadyTick = Game1.ticks + 1;
		}
		LoadedContentPack[] contentPacks = ContentPacks;
		InitializeScreenManagerIfNeeded(contentPacks);
		ScreenManager.Value.OnUpdateTicked();
	}

	private void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
	{
		if (!IsFirstTick)
		{
			ScreenManager.Value.OnLocaleChanged();
		}
	}

	private void Initialize()
	{
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Expected Obj, but got Unknown
		IModHelper helper = ((Mod)this).Helper;
		ContentPacks = GetContentPacks().ToArray();
		var enumerable = from token in QueuedModTokens.OrderByHuman((ModProvidedToken p) => p.Name)
			group token by token.Mod into modGroup
			select new
			{
				ModName = modGroup.Key.Name,
				ModPrefix = modGroup.First().NamePrefix,
				TokenNames = modGroup.Select((ModProvidedToken p) => p.NameWithoutPrefix).ToArray()
			};
		foreach (var item in enumerable)
		{
			((Mod)this).Monitor.Log($"{item.ModName} added {((item.TokenNames.Length == 1) ? "a custom token" : $"{item.TokenNames.Length} custom tokens")} with prefix '{item.ModPrefix}': {string.Join(", ", item.TokenNames)}.", (LogLevel)0);
		}
		ILookup<string, string> source = ContentPacks.SelectMany((LoadedContentPack pack) => pack.Migrator.MigrationWarnings.Select((string warning) => new
		{
			Name = pack.Manifest.Name,
			Warning = warning
		})).ToLookup(p => p.Warning, p => p.Name);
		foreach (IGrouping<string, string> item2 in source.OrderBy((IGrouping<string, string> p) => p.Key, new HumanSortComparer()))
		{
			((Mod)this).Monitor.Log(item2.Key + "\n\nAffected content packs:\n- " + string.Join("\n- ", item2.OrderByHuman((string p) => p)) + "\n\nFor mod authors, see how to update a mod: https://smapi.io/cp-migrate.", (LogLevel)2);
		}
		if (Config.EnableDebugFeatures)
		{
			helper.Events.Input.ButtonsChanged += OnButtonsChanged;
		}
		helper.Events.Content.AssetRequested += OnAssetRequested;
		helper.Events.GameLoop.DayStarted += OnDayStarted;
		helper.Events.GameLoop.TimeChanged += OnTimeChanged;
		helper.Events.Player.Warped += OnWarped;
		helper.Events.Specialized.LoadStageChanged += OnLoadStageChanged;
		TriggerActionManager.RegisterAction(((Mod)this).ModManifest.UniqueID + "_MigrateIds", (TriggerActionDelegate)new MigrateIdsAction().Handle);
		InitializeScreenManagerIfNeeded(ContentPacks);
		CommandHandler = new CommandHandler(ScreenManager, ((Mod)this).Monitor, ((Mod)this).Helper.GameContent, ContentPacks, (string? modId) =>
		{
			IContext result;
			if (modId != null)
			{
				result = ScreenManager.Value.TokenManager.GetContextFor(modId);
			}
			else
			{
				IContext tokenManager = ScreenManager.Value.TokenManager;
				result = tokenManager;
			}
			return result;
		}, () =>
		{
			ScreenManager.Value.UpdateContext(ContextUpdateType.All);
		});
		CommandHandler.RegisterWith(helper.ConsoleCommands);
		LoadedContentPack[] contentPacks = ContentPacks;
		foreach (LoadedContentPack contentPack in contentPacks)
		{
			if (!contentPack.Config.Any())
			{
				continue;
			}
			InvariantDictionary<ConfigField> config = contentPack.Config;
			GenericModConfigMenuIntegration<InvariantDictionary<ConfigField>> genericModConfigMenuIntegration = new GenericModConfigMenuIntegration<InvariantDictionary<ConfigField>>(((Mod)this).Helper.ModRegistry, ((Mod)this).Monitor, contentPack.Manifest, () => config, () =>
			{
				foreach (ConfigField value in config.Values)
				{
					value.SetValue(value.DefaultValues);
				}
			}, () =>
			{
				OnContentPackConfigChanged(contentPack);
			});
			if (!genericModConfigMenuIntegration.IsLoaded)
			{
				break;
			}
			GenericModConfigMenuIntegrationForContentPack genericModConfigMenuIntegrationForContentPack = new GenericModConfigMenuIntegrationForContentPack(contentPack.ContentPack, ParseCommaDelimitedField, config);
			genericModConfigMenuIntegrationForContentPack.Register(genericModConfigMenuIntegration, ((Mod)this).Monitor);
		}
	}

	private ScreenManager CreateScreenManager()
	{
		ProfilerIntegration profiler = new ProfilerIntegration(((Mod)this).Helper.ModRegistry, ((Mod)this).Monitor);
		ModProvidedToken[] modTokens = QueuedModTokens.ToArray();
		return new ScreenManager(((Mod)this).Helper, ((Mod)this).Monitor, GetInstalledMods(), modTokens, AssetValidators(), profiler);
	}

	private void InitializeScreenManagerIfNeeded(LoadedContentPack[] contentPacks)
	{
		ScreenManager value = ScreenManager.Value;
		if (!value.IsInitialized)
		{
			value.Initialize(contentPacks, GetInstalledMods());
		}
	}

	private IInvariantSet GetInstalledMods()
	{
		return InvariantSets.From((from p in ((Mod)this).Helper.ModRegistry.GetAll()
			select p.Manifest.UniqueID).OrderByHuman());
	}

	private void OnContentPackConfigChanged(LoadedContentPack contentPack)
	{
		contentPack.ConfigFileHandler.Save(contentPack.ContentPack, contentPack.Config, ((Mod)this).Helper);
		foreach (KeyValuePair<int, ScreenManager> activeValue in ScreenManager.GetActiveValues())
		{
			activeValue.Value.OnContentPackConfigChanged(contentPack);
		}
	}

	private void AddModToken(ModProvidedToken token)
	{
		if (!IsFirstTick)
		{
			((Mod)this).Monitor.Log($"Rejected token added by {token.Mod.Name} because tokens can't be added after SMAPI's {"GameLoop"}.{"GameLaunched"} event.", (LogLevel)4);
		}
		else
		{
			QueuedModTokens.Add(token);
		}
	}

	private IManagedConditions ParseConditionsForApi(IManifest manifest, InvariantDictionary<string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null)
	{
		IInvariantSet assumeModIdsLookup = ((assumeModIds != null) ? InvariantSets.From(assumeModIds) : InvariantSets.FromValue(manifest.UniqueID));
		IMigration migrator = new AggregateMigration(formatVersion, GetFormatVersions(null));
		return new ApiManagedConditions(() =>
		{
			ScreenManager value = ScreenManager.Value;
			IContext tokenManager = value.TokenManager;
			TokenParser tokenParser = new TokenParser(tokenManager, manifest, migrator, assumeModIdsLookup);
			bool isValid = value.PatchLoader.TryParseConditions(rawConditions, tokenParser, new LogPathBuilder(), out Condition[] conditions, out IInvariantSet _, out string error);
			ApiManagedConditionsForSingleScreen apiManagedConditionsForSingleScreen = new ApiManagedConditionsForSingleScreen(conditions, tokenManager, isValid, error);
			apiManagedConditionsForSingleScreen.UpdateContext();
			return apiManagedConditionsForSingleScreen;
		});
	}

	private IManagedTokenString ParseTokenStringForApi(IManifest manifest, string rawTokenString, ISemanticVersion formatVersion, string[]? assumeModIds = null)
	{
		IInvariantSet assumeModIdsLookup = ((assumeModIds != null) ? InvariantSets.From(assumeModIds) : InvariantSets.FromValue(manifest.UniqueID));
		IMigration migrator = new AggregateMigration(formatVersion, GetFormatVersions(null));
		return new ApiManagedTokenString(() =>
		{
			ScreenManager value = ScreenManager.Value;
			IContext tokenManager = value.TokenManager;
			TokenParser tokenParser = new TokenParser(tokenManager, manifest, migrator, assumeModIdsLookup);
			LogPathBuilder path = new LogPathBuilder();
			bool isValid = tokenParser.TryParseString(rawTokenString, assumeModIdsLookup, path, out string error, out ContentPatcher.Framework.IManagedTokenString parsed);
			if (parsed == null)
			{
				parsed = new LiteralString(string.Empty, path);
			}
			ApiManagedTokenStringForSingleScreen apiManagedTokenStringForSingleScreen = new ApiManagedTokenStringForSingleScreen(parsed, tokenManager, isValid, error);
			apiManagedTokenStringForSingleScreen.UpdateContext();
			return apiManagedTokenStringForSingleScreen;
		});
	}

	private IEnumerable<LoadedContentPack> GetContentPacks()
	{
		((Mod)this).Monitor.VerboseLog("Preloading content packs...");
		int index = -1;
		foreach (IContentPack contentPack in ((Mod)this).Helper.ContentPacks.GetOwned())
		{
			RawContentPack rawContentPack;
			try
			{
				int num = index + 1;
				index = num;
				rawContentPack = new RawContentPack(contentPack, num, GetFormatVersions);
				if (!rawContentPack.TryReloadContent(out string error))
				{
					((Mod)this).Monitor.Log($"Could not load content pack '{contentPack.Manifest.Name}': {error}.", (LogLevel)4);
					continue;
				}
				error = null;
			}
			catch (Exception value)
			{
				((Mod)this).Monitor.Log($"Error preloading content pack '{contentPack.Manifest.Name}'. Technical details:\n{value}", (LogLevel)4);
				continue;
			}
			ConfigFileHandler configFileHandler;
			InvariantDictionary<ConfigField> config;
			try
			{
				configFileHandler = new ConfigFileHandler("config.json", ParseCommaDelimitedField, (IContentPack pack, string label, string reason) =>
				{
					((Mod)this).Monitor.Log($"Ignored {pack.Manifest.Name} > {label}: {reason}", (LogLevel)3);
				});
				config = configFileHandler.Read(contentPack, rawContentPack.Content.ConfigSchema, rawContentPack.Content.Format);
				configFileHandler.Save(contentPack, config, ((Mod)this).Helper);
			}
			catch (Exception value2)
			{
				((Mod)this).Monitor.Log($"Error loading configuration for content pack '{contentPack.Manifest.Name}'. Technical details:\n{value2}", (LogLevel)4);
				continue;
			}
			yield return new LoadedContentPack(rawContentPack, configFileHandler, config);
		}
	}

	private IInvariantSet ParseCommaDelimitedField(string? field)
	{
		if (string.IsNullOrWhiteSpace(field))
		{
			return InvariantSets.Empty;
		}
		return InvariantSets.From(field.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
	}
}
