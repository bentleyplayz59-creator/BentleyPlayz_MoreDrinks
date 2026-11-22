using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.TextOperations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;

namespace ContentPatcher.Framework;

internal class PatchLoader
{
	private readonly PatchManager PatchManager;

	private readonly TokenManager TokenManager;

	private readonly IMonitor Monitor;

	private readonly IInvariantSet InstalledMods;

	private readonly Func<string, IAssetName> ParseAssetNameImpl;

	private readonly Lexer Lexer = new Lexer();

	private readonly InvariantSet ReservedLocalTokenNames = new InvariantSet("FromFile", "Target", "TargetPathOnly", "TargetWithoutPath");

	public PatchLoader(PatchManager patchManager, TokenManager tokenManager, IMonitor monitor, IInvariantSet installedMods, Func<string, IAssetName> parseAssetName)
	{
		PatchManager = patchManager;
		TokenManager = tokenManager;
		Monitor = monitor;
		InstalledMods = installedMods;
		ParseAssetNameImpl = parseAssetName;
	}

	public IEnumerable<IPatch> LoadPatches(RawContentPack contentPack, PatchConfig[] rawPatches, InvariantDictionary<IManagedTokenString>? inheritLocalTokens, int[] rootIndexPath, LogPathBuilder path, Patch? parentPatch)
	{
		bool isVerbose = Monitor.IsVerbose;
		ModTokenContext parentContext = TokenManager.TrackLocalTokens(contentPack.ContentPack);
		LocalContext localContext = new LocalContext(contentPack.Manifest.UniqueID, parentContext);
		foreach (ConditionType item in InternalConstants.FromFileTokens.Concat(InternalConstants.TargetTokens))
		{
			localContext.SetLocalValue(item.ToString(), "$~placeholder");
		}
		TokenParser tokenParser = new TokenParser(localContext, contentPack.Manifest, contentPack.Migrator, InstalledMods);
		PatchConfig[] patches = SplitPatches(rawPatches).ToArray();
		if (!patches.Any())
		{
			return Array.Empty<IPatch>();
		}
		UniquelyNamePatches(patches);
		if (!contentPack.Migrator.TryMigrate(ref patches, out string error))
		{
			Monitor.Log($"Ignored {path}: {error}", (LogLevel)3);
			return Array.Empty<IPatch>();
		}
		int num = -1;
		List<IPatch> list = new List<IPatch>(patches.Length);
		PatchConfig[] array = patches;
		foreach (PatchConfig patchConfig in array)
		{
			num++;
			LogPathBuilder localPath = path.With(patchConfig.LogName);
			if (isVerbose)
			{
				Monitor.Log($"   loading {localPath}...", (LogLevel)0);
			}
			IPatch patch = LoadPatch(contentPack, patchConfig, localContext, tokenParser, inheritLocalTokens, rootIndexPath.Concat(new _003C_003Ez__ReadOnlySingleElementList<int>(num)).ToArray(), localPath, parentPatch, (string reasonPhrase) =>
			{
				Monitor.Log($"Ignored {localPath}: {reasonPhrase}", (LogLevel)3);
			});
			if (patch != null)
			{
				list.Add(patch);
			}
		}
		return list;
	}

	public void UnloadPatchesLoadedBy(IPatch parentPatch)
	{
		UnloadPatches((IPatch patch) => IsDescendant(parentPatch, patch));
	}

	public void UnloadPatchesLoadedBy(RawContentPack pack)
	{
		UnloadPatches((IPatch patch) => patch.ContentPack == pack.ContentPack);
	}

	public bool TryParseConditions(IDictionary<string, string?>? raw, TokenParser tokenParser, LogPathBuilder path, out Condition[] conditions, out IInvariantSet immutableRequiredModIds, [NotNullWhen(false)] out string? error)
	{
		if (raw == null || !raw.Any())
		{
			immutableRequiredModIds = InvariantSets.Empty;
			conditions = Array.Empty<Condition>();
			error = null;
			return true;
		}
		MutableInvariantSet immutableRequiredModIds2 = new MutableInvariantSet();
		InvariantDictionary<Condition> invariantDictionary = new InvariantDictionary<Condition>();
		foreach (var (text3, value) in raw.OrderBy((KeyValuePair<string, string> p) => GetConditionParseOrder(p.Key, p.Value)))
		{
			if (!TryParseCondition(text3, value, tokenParser, path.With(text3), out Condition condition, ref immutableRequiredModIds2, out error))
			{
				immutableRequiredModIds = InvariantSets.Empty;
				conditions = Array.Empty<Condition>();
				return false;
			}
			invariantDictionary[text3] = condition;
		}
		immutableRequiredModIds = immutableRequiredModIds2.Lock();
		conditions = invariantDictionary.Values.ToArray();
		error = null;
		return true;
	}

	public bool TryParseUpdateRate(string? raw, out UpdateRate updateRate, [NotNullWhen(false)] out string? error)
	{
		updateRate = UpdateRate.OnDayStart;
		if (string.IsNullOrWhiteSpace(raw))
		{
			error = null;
			return true;
		}
		string[] array = raw.Split(',');
		string[] array2 = array;
		foreach (string value in array2)
		{
			if (!Enum.TryParse<UpdateRate>(value, ignoreCase: true, out var result))
			{
				error = $"Invalid {"Update"} value '{value}', expected one of: {string.Join(", ", Enum.GetNames(typeof(UpdateRate)))}";
				return false;
			}
			updateRate |= result;
		}
		error = null;
		return true;
	}

	private IAssetName ParseAssetName(string rawName)
	{
		if (rawName.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase))
		{
			string text = rawName;
			rawName = text.Substring(0, text.Length - 4);
		}
		return ParseAssetNameImpl(rawName);
	}

	private IEnumerable<PatchConfig> SplitPatches(IEnumerable<PatchConfig> patches)
	{
		foreach (PatchConfig patch in patches)
		{
			string[] targets = Lexer.SplitLexically(patch.Target).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
			string[] fromFiles = Lexer.SplitLexically(patch.FromFile).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
			if (targets.Length <= 1 && fromFiles.Length <= 1)
			{
				patch.Target = targets.FirstOrDefault() ?? patch.Target;
				patch.FromFile = fromFiles.FirstOrDefault() ?? patch.FromFile;
				yield return patch;
				continue;
			}
			foreach (string target in AlwaysIterate(targets))
			{
				foreach (string fromFile in AlwaysIterate(fromFiles))
				{
					PatchConfig newPatch = new PatchConfig(patch)
					{
						Target = target,
						FromFile = fromFile
					};
					if (string.IsNullOrWhiteSpace(newPatch.LogName))
					{
						newPatch.LogName = GetDefaultPatchName(newPatch);
					}
					else
					{
						List<string> labels = new List<string>();
						if (targets.Length > 1)
						{
							labels.Add(target ?? "");
						}
						if (fromFiles.Length > 1)
						{
							labels.Add("from " + fromFile);
						}
						if (labels.Any())
						{
							newPatch.LogName = newPatch.LogName + " (" + string.Join(" ", labels) + ")";
						}
					}
					yield return newPatch;
				}
			}
		}
		static IEnumerable<string?> AlwaysIterate(string[] values)
		{
			if (values.Length != 0)
			{
				return values;
			}
			return new _003C_003Ez__ReadOnlySingleElementList<string>(null);
		}
	}

	private void UniquelyNamePatches(PatchConfig[] patches)
	{
		foreach (PatchConfig patchConfig in patches)
		{
			if (string.IsNullOrWhiteSpace(patchConfig.LogName))
			{
				patchConfig.LogName = GetDefaultPatchName(patchConfig);
			}
		}
		foreach (IGrouping<string, PatchConfig> item in from p in patches.GroupBy((PatchConfig p) => p.LogName, StringComparer.OrdinalIgnoreCase)
			where p.Count() > 1
			select p)
		{
			int num = 0;
			foreach (PatchConfig item2 in item)
			{
				item2.LogName += $" #{++num}";
			}
		}
	}

	private string GetDefaultPatchName(PatchConfig patch)
	{
		if (Enum.TryParse<PatchType>(patch.Action, ignoreCase: true, out var result))
		{
			string text = ((result == PatchType.Include) ? patch.FromFile : patch.Target);
			return (!string.IsNullOrWhiteSpace(text)) ? $"{result} {PathUtilities.NormalizeAssetName(text)}" : $"{result} invalid";
		}
		return (!string.IsNullOrWhiteSpace(patch.Action)) ? (patch.Action ?? "") : "invalid";
	}

	private void UnloadPatches(Func<IPatch, bool> where)
	{
		IPatch[] array = PatchManager.GetPatches().Where(where).ToArray();
		if (array.Any())
		{
			IPatch[] array2 = array;
			foreach (IPatch patch in array2)
			{
				PatchManager.Remove(patch);
			}
		}
	}

	private IPatch? LoadPatch(RawContentPack rawContentPack, PatchConfig entry, LocalContext validatorPatchContext, TokenParser tokenParser, InvariantDictionary<IManagedTokenString>? inheritLocalTokens, int[] indexPath, LogPathBuilder path, Patch? parentPatch, Action<string> logSkip)
	{
		//IL_0bf2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c07: Unknown result type (might be due to invalid IL or missing references)
		//IL_1326: Unknown result type (might be due to invalid IL or missing references)
		//IL_1064: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dae: Unknown result type (might be due to invalid IL or missing references)
		//IL_1efb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f04: Unknown result type (might be due to invalid IL or missing references)
		IContentPack pack = rawContentPack.ContentPack;
		PatchType? action = null;
		try
		{
			if (!Enum.TryParse<PatchType>(entry.Action, ignoreCase: true, out var result))
			{
				return TrackSkip(string.IsNullOrWhiteSpace(entry.Action) ? "must set the Action field" : $"invalid {"Action"} value '{entry.Action}', expected one of: {string.Join(", ", Enum.GetNames(typeof(PatchType)))}");
			}
			action = result;
			HashSet<string> prohibitedTokensInFromFileAndTarget = null;
			if (inheritLocalTokens != null && inheritLocalTokens.Count > 0)
			{
				foreach (KeyValuePair<string, IManagedTokenString> inheritLocalToken in inheritLocalTokens)
				{
					validatorPatchContext.SetLocalValue(inheritLocalToken.Key, inheritLocalToken.Value, inheritLocalToken.Value.IsReady);
				}
			}
			InvariantDictionary<string?>? localTokens = entry.LocalTokens;
			if (localTokens != null && localTokens.Count > 0)
			{
				foreach (KeyValuePair<string, string> localToken in entry.LocalTokens)
				{
					if (!validatorPatchContext.Contains(localToken.Key, enforceContext: false))
					{
						if (prohibitedTokensInFromFileAndTarget == null)
						{
							prohibitedTokensInFromFileAndTarget = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						}
						prohibitedTokensInFromFileAndTarget.Add(localToken.Key);
					}
					validatorPatchContext.SetLocalValue(localToken.Key, localToken.Value, ready: false);
				}
			}
			if (!TryParseConditions(entry.When, tokenParser, path.With("When"), out Condition[] conditions, out IInvariantSet immutableRequiredModIds, out string error))
			{
				return TrackSkip("the When field is invalid: " + error);
			}
			IManagedTokenString parsed = null;
			string error2;
			if (string.IsNullOrWhiteSpace(entry.Target))
			{
				if (action != PatchType.Include)
				{
					return TrackSkip("must set the Target field");
				}
			}
			else if (!tokenParser.TryParseString(entry.Target, immutableRequiredModIds, path.With("Target"), out error2, out parsed, (LexTokenToken token) => PreValidateFromFileOrTargetToken("Target", token, prohibitedTokensInFromFileAndTarget)))
			{
				return TrackSkip("the Target is invalid: " + error2);
			}
			IManagedTokenString parsed2 = null;
			if (entry.TargetLocale != null && !tokenParser.TryParseString(entry.TargetLocale, immutableRequiredModIds, path.With("TargetLocale"), out string error3, out parsed2))
			{
				return TrackSkip("the TargetLocale is invalid: " + error3);
			}
			IManagedTokenString tokenedPath = null;
			if (entry.FromFile != null && !TryPrepareLocalAsset(entry.FromFile, tokenParser, immutableRequiredModIds, path.With("FromFile"), out string error4, out tokenedPath, (LexTokenToken token) => PreValidateFromFileOrTargetToken("Target", token, prohibitedTokensInFromFileAndTarget)))
			{
				return TrackSkip(error4);
			}
			InvariantDictionary<IManagedTokenString> invariantDictionary = null;
			InvariantDictionary<string?>? localTokens2 = entry.LocalTokens;
			if (localTokens2 != null && localTokens2.Count > 0)
			{
				invariantDictionary = new InvariantDictionary<IManagedTokenString>();
				foreach (var (text3, rawValue) in entry.LocalTokens)
				{
					string text4 = text3.Trim();
					if (ReservedLocalTokenNames.Contains(text4))
					{
						return TrackSkip($"the {"LocalTokens"} field can't contain a token named '{text4}', which is a reserved token name.");
					}
					if (!tokenParser.TryParseNullableString(rawValue, immutableRequiredModIds, path.With("LocalTokens", text4), out string error5, out IManagedTokenString parsed3))
					{
						return TrackSkip(error5);
					}
					if (parsed3 != null)
					{
						invariantDictionary.Add(text4, parsed3);
					}
				}
			}
			bool parsed4 = true;
			if (entry.Enabled != null)
			{
				if (!rawContentPack.Content.Format.IsOlderThan("1.25.0") && (!bool.TryParse(entry.Enabled, out var result2) || !result2))
				{
					return TrackSkip("the Enabled field is obsolete and should be removed");
				}
				if (!TryParseEnabled(entry.Enabled, tokenParser, immutableRequiredModIds, path.With("Enabled"), out string error6, out parsed4))
				{
					return TrackSkip($"invalid {"Enabled"} value '{entry.Enabled}': {error6}");
				}
			}
			if (!TryParseUpdateRate(entry.Update, out UpdateRate updateRate, out string error7))
			{
				return TrackSkip(error7);
			}
			if (parsed != null && parsed.UsesTokens(InternalConstants.TargetTokens))
			{
				return TrackSkip($"circular field reference: {"Target"} field can't use the '{string.Join("', '", InternalConstants.TargetTokens)}' tokens.");
			}
			if (tokenedPath != null)
			{
				if (tokenedPath.UsesTokens(InternalConstants.FromFileTokens))
				{
					return TrackSkip($"circular field reference: {"FromFile"} field can't use the '{string.Join("', '", InternalConstants.FromFileTokens)}' tokens.");
				}
				if (tokenedPath.UsesTokens(InternalConstants.TargetTokens) && parsed != null && parsed.UsesTokens(InternalConstants.FromFileTokens))
				{
					return TrackSkip($"circular field reference: {"Target"} field can't use the '{string.Join("', '", InternalConstants.FromFileTokens)}' tokens if the {"FromFile"} field uses '{string.Join("', '", InternalConstants.TargetTokens)}' tokens.");
				}
			}
			IPatch patch;
			switch (action)
			{
			case PatchType.Include:
				if (tokenedPath == null)
				{
					return TrackSkip($"must set the {"FromFile"} field for an {PatchType.Include} patch.");
				}
				if (parsed != null)
				{
					return TrackSkip($"can't use the {"Target"} field with an {PatchType.Include} patch.");
				}
				patch = new IncludePatch(indexPath, path, conditions, tokenedPath, updateRate, inheritLocalTokens, invariantDictionary, rawContentPack, parentPatch, ParseAssetName, Monitor, this);
				break;
			case PatchType.Load:
			{
				if (tokenedPath == null)
				{
					return TrackSkip($"must set the {"FromFile"} field for a {PatchType.Load} patch.");
				}
				if (!TryParsePriority<AssetLoadPriority>(entry, (AssetLoadPriority)int.MaxValue, out AssetLoadPriority priority5, out string error11))
				{
					return TrackSkip(error11);
				}
				LogPathBuilder path3 = path;
				IManagedTokenString assetName2 = parsed;
				IManagedTokenString assetLocale2 = parsed2;
				AssetLoadPriority priority6 = priority5;
				UpdateRate updateRate2 = updateRate;
				InvariantDictionary<IManagedTokenString> localTokens3 = inheritLocalTokens;
				InvariantDictionary<IManagedTokenString> inheritedLocalTokens = invariantDictionary;
				IEnumerable<Condition> conditions3 = conditions;
				patch = new LoadPatch(indexPath, path3, assetName2, assetLocale2, tokenedPath, priority6, updateRate2, localTokens3, inheritedLocalTokens, conditions3, pack, rawContentPack.Migrator, parentPatch, ParseAssetName);
				break;
			}
			case PatchType.EditData:
			{
				bool flag = rawContentPack.Content.Format.IsOlderThan("1.18.0");
				bool flag2 = !entry.Entries.Any() && !entry.Fields.Any() && !entry.MoveEntries.Any() && !entry.TextOperations.Any();
				if (flag)
				{
					if (flag2 && tokenedPath == null)
					{
						return TrackSkip($"one of {"Entries"}, {"Fields"}, {"MoveEntries"}, {"TextOperations"}, or {"FromFile"} must be specified for an '{action}' change");
					}
					if (tokenedPath != null && (entry.Entries.Any() || entry.Fields.Any() || entry.MoveEntries.Any()))
					{
						return TrackSkip("FromFile is mutually exclusive with Entries, Fields, and MoveEntries");
					}
				}
				else
				{
					if (tokenedPath != null)
					{
						return TrackSkip($"the {"FromFile"} field can't be used with an '{action}' patch");
					}
					if (flag2)
					{
						return TrackSkip($"one of {"Entries"}, {"Fields"}, {"MoveEntries"}, or {"TextOperations"} must be specified for an '{action}' change");
					}
				}
				List<EditDataPatchRecord> parsedEntries = null;
				List<EditDataPatchField> parsedFields = null;
				List<EditDataPatchMoveRecord> parsedMoveEntries = null;
				List<IManagedTokenString> targetField = null;
				if (tokenedPath == null && !TryParseFields(tokenParser.Context, entry, out parsedEntries, out parsedFields, out parsedMoveEntries, out targetField, out var setParseError))
				{
					return TrackSkip(setParseError);
				}
				if (!TryParsePriority<AssetEditPriority>(entry, (AssetEditPriority)0, out AssetEditPriority priority4, out setParseError))
				{
					return TrackSkip(setParseError);
				}
				if (!TryParseTextOperations(entry, tokenParser, immutableRequiredModIds, path.With("TextOperations"), out IList<ITextOperation> textOperations2, out setParseError))
				{
					return TrackSkip(setParseError);
				}
				patch = new EditDataPatch(indexPath, path, parsed, parsed2, priority4, conditions, tokenedPath, parsedEntries, parsedFields, parsedMoveEntries, textOperations2, targetField, updateRate, inheritLocalTokens, invariantDictionary, pack, rawContentPack.Migrator, parentPatch, Monitor, ParseAssetName, TryParseFields);
				break;
			}
			case PatchType.EditImage:
			{
				if (tokenedPath == null)
				{
					return TrackSkip($"must set the {"FromFile"} field for a {PatchType.Load} patch.");
				}
				PatchImageMode result4 = PatchImageMode.Replace;
				if (!string.IsNullOrWhiteSpace(entry.PatchMode) && !Enum.TryParse<PatchImageMode>(entry.PatchMode, ignoreCase: true, out result4))
				{
					return TrackSkip($"the {"PatchMode"} is invalid. Expected one of these values: [{string.Join(", ", Enum.GetNames(typeof(PatchImageMode)))}]");
				}
				TokenRectangle parsed16 = null;
				if (entry.FromArea != null && !TryParseRectangle(entry.FromArea, tokenParser, immutableRequiredModIds, path.With("FromArea"), out string error10, out parsed16))
				{
					return TrackSkip(error10);
				}
				TokenRectangle parsed17 = null;
				if (entry.ToArea != null && !TryParseRectangle(entry.ToArea, tokenParser, immutableRequiredModIds, path.With("ToArea"), out error10, out parsed17))
				{
					return TrackSkip(error10);
				}
				if (!TryParsePriority<AssetEditPriority>(entry, (AssetEditPriority)0, out AssetEditPriority priority2, out error10))
				{
					return TrackSkip(error10);
				}
				LogPathBuilder path2 = path;
				IManagedTokenString assetName = parsed;
				IManagedTokenString assetLocale = parsed2;
				AssetEditPriority priority3 = priority2;
				Condition[] conditions2 = conditions;
				InvariantDictionary<IManagedTokenString> inheritedLocalTokens = inheritLocalTokens;
				InvariantDictionary<IManagedTokenString> localTokens3 = invariantDictionary;
				patch = new EditImagePatch(indexPath, path2, assetName, assetLocale, priority3, conditions2, tokenedPath, parsed16, parsed17, result4, updateRate, inheritedLocalTokens, localTokens3, pack, rawContentPack.Migrator, parentPatch, Monitor, ParseAssetName);
				break;
			}
			case PatchType.EditMap:
			{
				List<EditMapPatchProperty> list = new List<EditMapPatchProperty>();
				string error8;
				foreach (KeyValuePair<string, string> mapProperty in entry.MapProperties)
				{
					LogPathBuilder logPathBuilder = path.With("MapProperties", mapProperty.Key);
					if (!tokenParser.TryParseString(mapProperty.Key, immutableRequiredModIds, logPathBuilder.With("key"), out error8, out IManagedTokenString parsed5))
					{
						return TrackSkip($"{"MapProperties"} > '{mapProperty.Key}' key is invalid: {error8}");
					}
					if (!tokenParser.TryParseNullableString(mapProperty.Value, immutableRequiredModIds, logPathBuilder.With("value"), out error8, out IManagedTokenString parsed6))
					{
						return TrackSkip($"{"MapProperties"} > '{mapProperty.Key}' value '{mapProperty.Value}' is invalid: {error8}");
					}
					list.Add(new EditMapPatchProperty(parsed5, parsed6));
				}
				List<EditMapPatchTile> list2 = new List<EditMapPatchTile>();
				for (int num = 0; num < entry.MapTiles.Count; num++)
				{
					PatchMapTileConfig patchMapTileConfig = entry.MapTiles[num];
					if (patchMapTileConfig == null)
					{
						continue;
					}
					LogPathBuilder logPathBuilder2 = path.With("MapTiles", num.ToString());
					string value = $"{"MapTiles"} > entry #{num + 1}";
					if (!tokenParser.TryParseString(patchMapTileConfig.Layer, immutableRequiredModIds, logPathBuilder2.With("Layer"), out error8, out IManagedTokenString parsed7))
					{
						return TrackSkip($"{value} > {"Layer"} is invalid: {error8}");
					}
					if (!TryParsePosition(patchMapTileConfig.Position, tokenParser, immutableRequiredModIds, logPathBuilder2.With("Position"), out error8, out TokenPosition parsed8))
					{
						return TrackSkip($"{value} > {"Position"} is invalid: {error8}");
					}
					IManagedTokenString parsed9 = null;
					if (patchMapTileConfig.SetTilesheet != null && !tokenParser.TryParseString(patchMapTileConfig.SetTilesheet, immutableRequiredModIds, logPathBuilder2.With("SetTilesheet"), out error8, out parsed9))
					{
						return TrackSkip($"{value} > {"SetTilesheet"} is invalid: {error8}");
					}
					IManagedTokenString parsed10 = null;
					if (patchMapTileConfig.SetIndex != null && !TryParseInt(patchMapTileConfig.SetIndex, tokenParser, immutableRequiredModIds, logPathBuilder2.With("SetIndex"), out error8, out parsed10))
					{
						return TrackSkip($"{value} > {"SetIndex"} is invalid: {error8}");
					}
					Dictionary<IManagedTokenString, IManagedTokenString> dictionary = new Dictionary<IManagedTokenString, IManagedTokenString>();
					int num2 = 0;
					foreach (KeyValuePair<string, string> setProperty in patchMapTileConfig.SetProperties)
					{
						num2++;
						if (!tokenParser.TryParseString(setProperty.Key, immutableRequiredModIds, logPathBuilder2.With("SetProperties", "key"), out error8, out IManagedTokenString parsed11))
						{
							return TrackSkip($"{value} > {"SetProperties"} > entry #{num2 + 1} > key is invalid: {error8}");
						}
						if (!tokenParser.TryParseNullableString(setProperty.Value, immutableRequiredModIds, logPathBuilder2.With("SetProperties", "value"), out error8, out IManagedTokenString parsed12))
						{
							return TrackSkip($"{value} > {"SetProperties"} > entry #{num2 + 1} > value is invalid: {error8}");
						}
						dictionary[parsed11] = parsed12;
					}
					IManagedTokenString parsed13 = null;
					if (patchMapTileConfig.Remove != null && !TryParseBoolean(patchMapTileConfig.Remove, tokenParser, immutableRequiredModIds, logPathBuilder2.With("Remove"), out error8, out parsed13))
					{
						return TrackSkip($"{value} > {"Remove"} is invalid: {error8}");
					}
					IManagedTokenString layer = parsed7;
					list2.Add(new EditMapPatchTile(parsed8, layer, parsed10, parsed9, dictionary, parsed13));
				}
				if (!TryParseWarps(entry.AddNpcWarps, tokenParser, immutableRequiredModIds, path.With("AddNpcWarps"), out List<IManagedTokenString> warps, out string error9))
				{
					return TrackSkip("AddNpcWarps is invalid: " + error9);
				}
				if (!TryParseWarps(entry.AddWarps, tokenParser, immutableRequiredModIds, path.With("AddWarps"), out List<IManagedTokenString> warps2, out error9))
				{
					return TrackSkip("AddWarps is invalid: " + error9);
				}
				if (!TryParseTextOperations(entry, tokenParser, immutableRequiredModIds, path.With("TextOperations"), out IList<ITextOperation> textOperations, out error8))
				{
					return TrackSkip(error8);
				}
				TokenRectangle parsed14 = null;
				if (entry.FromArea != null && !TryParseRectangle(entry.FromArea, tokenParser, immutableRequiredModIds, path.With("FromArea"), out error8, out parsed14))
				{
					return TrackSkip(error8);
				}
				TokenRectangle parsed15 = null;
				if (entry.ToArea != null && !TryParseRectangle(entry.ToArea, tokenParser, immutableRequiredModIds, path.With("ToArea"), out error8, out parsed15))
				{
					return TrackSkip(error8);
				}
				PatchMapMode result3 = (PatchMapMode)1;
				if (!string.IsNullOrWhiteSpace(entry.PatchMode) && !Enum.TryParse<PatchMapMode>(entry.PatchMode, true, out result3))
				{
					return TrackSkip($"the {"PatchMode"} is invalid. Expected one of these values: [{string.Join(", ", Enum.GetNames(typeof(PatchMapMode)))}]");
				}
				if (tokenedPath == null && !list.Any() && !list2.Any() && !warps2.Any() && !warps.Any() && !textOperations.Any())
				{
					return TrackSkip("must specify at least one of AddNpcWarps, AddWarps, FromFile, MapProperties, MapTiles, or TextOperations");
				}
				if (!TryParsePriority<AssetEditPriority>(entry, (AssetEditPriority)0, out AssetEditPriority priority, out error8))
				{
					return TrackSkip(error8);
				}
				patch = new EditMapPatch(indexPath, path, parsed, parsed2, priority, conditions, tokenedPath, parsed14, parsed15, result3, list, list2, warps, warps2, textOperations, updateRate, inheritLocalTokens, invariantDictionary, pack, rawContentPack.Migrator, parentPatch, Monitor, ParseAssetName);
				break;
			}
			default:
				return TrackSkip($"unsupported patch type '{action}'");
			}
			if (!parsed4)
			{
				return TrackSkip("Enabled is false", warn: false);
			}
			IInvariantSet invariantSet = InvariantSets.From(patch.GetTokensUsed());
			foreach (ConditionType fromFileToken in InternalConstants.FromFileTokens)
			{
				if (invariantSet.Contains(fromFileToken.ToString()) && patch.RawFromAsset == null)
				{
					return TrackSkip($"can't use the {{{{{fromFileToken}}}}} token because the patch has no {"FromFile"} field.");
				}
			}
			foreach (ConditionType targetToken in InternalConstants.TargetTokens)
			{
				if (invariantSet.Contains(targetToken.ToString()) && patch.RawTargetAsset == null)
				{
					return TrackSkip($"can't use the {{{{{targetToken}}}}} token because the patch has no {"Target"} field.");
				}
			}
			PatchManager.Add(patch);
			return patch;
			bool TryParseFields(IContext context, PatchConfig rawFields, out List<EditDataPatchRecord> entries, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out List<IManagedTokenString> targetField2, [NotNullWhen(false)] out string? error12)
			{
				return TryParseEditDataFields(rawFields, tokenParser, immutableRequiredModIds, path, out entries, out fields, out moveEntries, out targetField2, out error12);
			}
		}
		catch (Exception value2)
		{
			return TrackSkip($"error reading info. Technical details:\n{value2}");
		}
		IPatch? TrackSkip(string reason, bool warn = true)
		{
			reason = reason.TrimEnd(new char[2] { '.', ' ' });
			PatchManager.AddPermanentlyDisabled(new DisabledPatch(path, entry.Action, action, entry.Target, entry.TargetLocale, pack, parentPatch, reason));
			if (warn)
			{
				logSkip(reason + ".");
			}
			return null;
		}
	}

	private string? PreValidateFromFileOrTargetToken(string fieldName, LexTokenToken lexToken, HashSet<string>? prohibitedTokens)
	{
		return (prohibitedTokens?.Contains(lexToken.Name) ?? false) ? $"'{lexToken.Name}' is a non-inherited local token, which can't be used in the {fieldName} field." : null;
	}

	private bool TryParsePriority<TPriority>(PatchConfig patch, TPriority defaultValue, out TPriority priority, [NotNullWhen(false)] out string? error) where TPriority : struct
	{
		if (string.IsNullOrWhiteSpace(patch.Priority))
		{
			priority = defaultValue;
			error = null;
			return true;
		}
		TPriority val = default;
		if (Utility.TryParseEnum<TPriority>(patch.Priority, ref val))
		{
			priority = val;
			error = null;
			return true;
		}
		int num = patch.Priority.IndexOfAny(new char[2] { '-', '+' });
		if (num > 0)
		{
			string text = patch.Priority.Substring(0, num);
			char c = patch.Priority[num];
			string priority2 = patch.Priority;
			int num2 = num + 1;
			string s = priority2.Substring(num2, priority2.Length - num2);
			if (Utility.TryParseEnum<TPriority>(text, ref val) && int.TryParse(s, out var result))
			{
				if (c == '-')
				{
					result *= -1;
				}
				priority = (TPriority)(object)((int)(object)val + result);
				error = null;
				return true;
			}
		}
		priority = defaultValue;
		error = $"the {"Priority"} value '{patch.Priority}' is invalid: expected one of [{string.Join(", ", Enum.GetNames(typeof(TPriority)))}] or a simple offset like '{Activator.CreateInstance(typeof(TPriority))} + 10'";
		return false;
	}

	private bool TryParseTextOperations(PatchConfig patch, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder path, out IList<ITextOperation> textOperations, [NotNullWhen(false)] out string? error)
	{
		textOperations = new List<ITextOperation>();
		if (!patch.TextOperations.Any())
		{
			error = null;
			return true;
		}
		int value = 0;
		foreach (TextOperationConfig textOperation3 in patch.TextOperations)
		{
			if (textOperation3 == null)
			{
				continue;
			}
			LogPathBuilder logPathBuilder = path.With(value++.ToString());
			string text = $"{"TextOperations"} > {value} is invalid";
			if (!Enum.TryParse<TextOperationType>(textOperation3.Operation, ignoreCase: true, out var result))
			{
				return Fail(string.IsNullOrWhiteSpace(textOperation3.Operation) ? (text + ": the Operation must be set") : $"{text}: invalid {"Operation"} value '{textOperation3.Operation}', expected one of: {string.Join(", ", Enum.GetNames(typeof(TextOperationType)))}", out error);
			}
			List<IManagedTokenString> list = new List<IManagedTokenString>();
			string[] target = textOperation3.Target;
			foreach (string text2 in target)
			{
				if (!tokenParser.TryParseString(text2, assumeModIds, logPathBuilder.With("Target", value.ToString()), out string error2, out IManagedTokenString parsed))
				{
					return Fail($"{text}: the {"Target"} value '{text2}' couldn't be parsed: {error2}", out error);
				}
				list.Add(parsed);
			}
			if (list.Count == 0)
			{
				return Fail(text + ": the Target value must be set.", out error);
			}
			if (list.Count == 1)
			{
				return Fail(text + ": the Target value must specify at least two segments.", out error);
			}
			if (!tokenParser.TryParseString(textOperation3.Value, assumeModIds, logPathBuilder.With("Value"), out string error3, out IManagedTokenString parsed2))
			{
				return Fail($"{text}: the {"Value"} value '{textOperation3.Value}' couldn't be parsed: {error3}", out error);
			}
			if (!tokenParser.TryParseString(textOperation3.Search, assumeModIds, logPathBuilder.With("Search"), out string error4, out IManagedTokenString parsed3))
			{
				return Fail($"{text}: the {"Search"} value '{textOperation3.Search}' couldn't be parsed: {error4}", out error);
			}
			TextOperationReplaceMode result2;
			if (textOperation3.ReplaceMode == null)
			{
				result2 = TextOperationReplaceMode.All;
			}
			else if (!Enum.TryParse<TextOperationReplaceMode>(textOperation3.ReplaceMode, ignoreCase: true, out result2))
			{
				return Fail($"{text}: invalid {"ReplaceMode"} value '{textOperation3.ReplaceMode}', expected one of: {string.Join(", ", Enum.GetNames(typeof(TextOperationReplaceMode)))}", out error);
			}
			ITextOperation item;
			switch (result)
			{
			case TextOperationType.Append:
			case TextOperationType.Prepend:
				item = new AppendOrPrependTextOperation(result, list, parsed2, textOperation3.Delimiter);
				break;
			case TextOperationType.RemoveDelimited:
			case TextOperationType.ReplaceDelimited:
			{
				if (string.IsNullOrEmpty(textOperation3.Delimiter))
				{
					return Fail($"{text}: the {"Delimiter"} value must be set for a {result} text operation.", out error);
				}
				if (string.IsNullOrWhiteSpace(parsed3.Raw))
				{
					return Fail($"{text}: the {"Search"} value must be set for a {result} text operation.", out error);
				}
				ITextOperation textOperation2;
				if (result != TextOperationType.RemoveDelimited)
				{
					ITextOperation textOperation = new ReplaceDelimitedTextOperation(list, parsed3, parsed2, textOperation3.Delimiter, result2);
					textOperation2 = textOperation;
				}
				else
				{
					ITextOperation textOperation = new RemoveDelimitedTextOperation(list, parsed3, textOperation3.Delimiter, result2);
					textOperation2 = textOperation;
				}
				item = textOperation2;
				break;
			}
			default:
				return Fail($"{text}: unsupported text operation type '{result}'", out error);
			}
			textOperations.Add(item);
		}
		error = null;
		return true;
		static bool Fail(string reason, out string outReason)
		{
			outReason = reason;
			return false;
		}
	}

	private bool TryParseWarps(List<string?> rawWarps, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder path, out List<IManagedTokenString> warps, [NotNullWhen(false)] out string? error)
	{
		if (rawWarps.Count == 0)
		{
			warps = new List<IManagedTokenString>();
			error = null;
			return true;
		}
		warps = new List<IManagedTokenString>(rawWarps.Count);
		for (int i = 0; i < rawWarps.Count; i++)
		{
			LogPathBuilder path2 = path.With(i.ToString());
			if (!tokenParser.TryParseString(rawWarps[i], assumeModIds, path2, out string error2, out IManagedTokenString parsed))
			{
				warps.Clear();
				error = "'" + rawWarps[i] + "' is invalid: " + error2;
				return false;
			}
			warps.Add(parsed);
		}
		error = null;
		return true;
	}

	private bool TryParseEditDataFields(PatchConfig entry, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder path, out List<EditDataPatchRecord> entries, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out List<IManagedTokenString> targetField, [NotNullWhen(false)] out string? error)
	{
		entries = new List<EditDataPatchRecord>();
		fields = new List<EditDataPatchField>();
		moveEntries = new List<EditDataPatchMoveRecord>();
		targetField = new List<IManagedTokenString>();
		string key;
		JToken value;
		foreach (KeyValuePair<string, JToken> entry2 in entry.Entries)
		{
			entry2.Deconstruct(out key, out value);
			string text = key;
			JToken rawJson = value;
			LogPathBuilder logPathBuilder = path.With("Entries", text);
			if (!tokenParser.TryParseString(text, assumeModIds, logPathBuilder.With("key"), out string error2, out IManagedTokenString parsed))
			{
				return Fail($"{"Entries"} > '{text}' key is invalid: {error2}", out error);
			}
			if (!tokenParser.TryParseJson(rawJson, assumeModIds, logPathBuilder.With("value"), out string error3, out TokenizableJToken parsed2))
			{
				return Fail($"{"Entries"} > '{text}' value is invalid: {error3}", out error);
			}
			entries.Add(new EditDataPatchRecord(parsed, parsed2));
		}
		foreach (KeyValuePair<string, InvariantDictionary<JToken>> field in entry.Fields)
		{
			field.Deconstruct(out key, out var value2);
			InvariantDictionary<JToken> invariantDictionary = value2;
			string text2 = key;
			Dictionary<string, JToken> dictionary = invariantDictionary;
			if (dictionary == null)
			{
				continue;
			}
			LogPathBuilder logPathBuilder2 = path.With("Fields", text2);
			if (!tokenParser.TryParseString(text2, assumeModIds, logPathBuilder2.With("key"), out string error4, out IManagedTokenString parsed3))
			{
				return Fail($"{"Fields"} > entry {text2} is invalid: {error4}", out error);
			}
			foreach (KeyValuePair<string, JToken> item in dictionary)
			{
				item.Deconstruct(out key, out value);
				string text3 = key;
				JToken rawJson2 = value;
				if (!tokenParser.TryParseString(text3, assumeModIds, logPathBuilder2.With(text3, "key"), out string error5, out IManagedTokenString parsed4))
				{
					return Fail($"{"Fields"} > entry {text2} > field {text3} key is invalid: {error5}", out error);
				}
				if (!tokenParser.TryParseJson(rawJson2, assumeModIds, logPathBuilder2.With(text3, "value"), out string error6, out TokenizableJToken parsed5))
				{
					return Fail($"{"Fields"} > entry {text2} > field {parsed4} is invalid: {error6}", out error);
				}
				if (parsed5 != null && parsed5.IsString && parsed5.GetTokenStrings().SelectMany((IManagedTokenString p) => p.LexTokens).OfType<LexTokenLiteral>()
					.Any((LexTokenLiteral p) => p.Text.Contains("/")))
				{
					return Fail($"{"Fields"} > entry {text2} > field {parsed4} is invalid: value can't contain field delimiter character '/'", out error);
				}
				fields.Add(new EditDataPatchField(parsed3, parsed4, parsed5));
			}
		}
		int num = 0;
		foreach (PatchMoveEntryConfig moveEntry in entry.MoveEntries)
		{
			if (moveEntry != null)
			{
				LogPathBuilder logPathBuilder3 = path.With("MoveEntries", num++.ToString());
				string[] source = new string[3] { moveEntry.BeforeId, moveEntry.AfterId, moveEntry.ToPosition };
				if (string.IsNullOrWhiteSpace(moveEntry.ID))
				{
					return Fail("MoveEntries > move entry is invalid: must specify an ID value", out error);
				}
				int num2 = source.Count((string p) => !string.IsNullOrWhiteSpace(p));
				int num3 = num2;
				if (num3 > 1)
				{
					return Fail($"{"MoveEntries"} > entry '{moveEntry.ID}' is invalid: must specify only one of {"ToPosition"}, {"BeforeId"}, and {"AfterId"}", out error);
				}
				if (num3 == 0)
				{
					return Fail($"{"MoveEntries"} > entry '{moveEntry.ID}' is invalid: must specify one of {"ToPosition"}, {"BeforeId"}, or {"AfterId"}", out error);
				}
				if (!tokenParser.TryParseString(moveEntry.ID, assumeModIds, logPathBuilder3.With("ID"), out string error7, out IManagedTokenString parsed6))
				{
					return Fail($"{"MoveEntries"} > entry '{moveEntry.ID}' > {"ID"} is invalid: {error7}", out error);
				}
				if (!tokenParser.TryParseString(moveEntry.BeforeId, assumeModIds, logPathBuilder3.With("BeforeId"), out string error8, out IManagedTokenString parsed7))
				{
					return Fail($"{"MoveEntries"} > entry '{moveEntry.ID}' > {"BeforeId"} is invalid: {error8}", out error);
				}
				if (!tokenParser.TryParseString(moveEntry.AfterId, assumeModIds, logPathBuilder3.With("AfterId"), out string error9, out IManagedTokenString parsed8))
				{
					return Fail($"{"MoveEntries"} > entry '{moveEntry.ID}' > {"AfterId"} is invalid: {error9}", out error);
				}
				MoveEntryPosition result = MoveEntryPosition.None;
				if (!string.IsNullOrWhiteSpace(moveEntry.ToPosition) && (!Enum.TryParse<MoveEntryPosition>(moveEntry.ToPosition, ignoreCase: true, out result) || result == MoveEntryPosition.None))
				{
					return Fail($"{"MoveEntries"} > entry '{moveEntry.ID}' > {"ToPosition"} is invalid: must be one of {"Bottom"} or {"Top"}", out error);
				}
				moveEntries.Add(new EditDataPatchMoveRecord(parsed6, parsed7, parsed8, result));
			}
		}
		int num4 = 0;
		foreach (string item2 in entry.TargetField)
		{
			LogPathBuilder path2 = path.With("TargetField", num4++.ToString());
			if (!tokenParser.TryParseString(item2, assumeModIds, path2, out string error10, out IManagedTokenString parsed9))
			{
				return Fail($"{"Fields"} > target path {num4 - 1} is invalid: {error10}", out error);
			}
			targetField.Add(parsed9);
		}
		error = null;
		return true;
		static bool Fail(string reason, out string outReason)
		{
			outReason = reason;
			return false;
		}
	}

	private bool TryParseCondition(string? name, string? value, TokenParser tokenParser, LogPathBuilder path, [NotNullWhen(true)] out Condition? condition, ref MutableInvariantSet immutableRequiredModIds, [NotNullWhen(false)] out string? error)
	{
		ILexToken[] array = Lexer.ParseBits(name, impliedBraces: true).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!tokenParser.Migrator.TryMigrate(ref array[i], out error))
			{
				return Fail(error, out error, out condition);
			}
		}
		if (array.Length != 1 || !(array[0] is LexTokenToken lexTokenToken))
		{
			return Fail("'" + name + "' isn't a valid token name", out error, out condition);
		}
		LexTokenToken lexTokenToken2 = lexTokenToken;
		IManagedTokenString managedTokenString = tokenParser.CreateTokenStringOrNull(lexTokenToken2.InputArgs?.Parts, tokenParser.Context, path.With("key"));
		IInputArguments input = tokenParser.CreateInputArgs(managedTokenString);
		if (!tokenParser.TryValidateToken(lexTokenToken2, immutableRequiredModIds.GetImmutable(), out error))
		{
			return Fail(error, out error, out condition);
		}
		IToken token = tokenParser.Context.GetToken(lexTokenToken2.Name, enforceContext: false);
		if (token != null && !token.TryValidateInput(input, out error))
		{
			return Fail(error, out error, out condition);
		}
		if (string.IsNullOrWhiteSpace(value))
		{
			return Fail("can't parse condition " + name + ": value can't be empty", out error, out condition);
		}
		if (!tokenParser.TryParseString(value, immutableRequiredModIds.GetImmutable(), path.With("value"), out error, out IManagedTokenString parsed))
		{
			return Fail("can't parse condition " + name + ": " + error, out error, out condition);
		}
		if (parsed != null && !parsed.IsMutable && parsed.IsReady && token != null && !token.TryValidateValues(input, parsed.SplitValuesUnique(token.NormalizeValue), tokenParser.Context, out string error2))
		{
			return Fail("invalid " + lexTokenToken2.Name + " condition: " + error2, out error, out condition);
		}
		condition = new Condition(token?.Name ?? lexTokenToken2.Name, managedTokenString, parsed, token?.IsMutable ?? false);
		if (!tokenParser.Migrator.TryMigrate(condition, out error))
		{
			return Fail(error, out error, out condition);
		}
		Condition condition2 = condition;
		if (condition2 != null && condition2.IsReady && !condition2.IsMutable && condition.Is(ConditionType.HasMod))
		{
			if (condition.Input.ReservedArgs.TryGetValue("contains", out IInputArgumentValue value2))
			{
				if (bool.TryParse(condition.Values.Value, out var result) & result)
				{
					CollectionExtensions.AddRange<string>((ISet<string>)immutableRequiredModIds, (IEnumerable<string>)value2.Parsed);
				}
			}
			else
			{
				CollectionExtensions.AddRange<string>((ISet<string>)immutableRequiredModIds, (IEnumerable<string>)condition.CurrentValues);
			}
		}
		return true;
		static bool Fail(string reason, out string setError, out Condition? setCondition)
		{
			setCondition = null;
			setError = reason;
			return false;
		}
	}

	private bool TryParseBoolean(string? rawValue, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out IManagedTokenString? parsed)
	{
		if (!tokenParser.TryParseString(rawValue, assumeModIds, path, out error, out parsed))
		{
			return false;
		}
		if (parsed.HasAnyTokens)
		{
			if (!parsed.IsSingleTokenOnly)
			{
				error = "can't be treated as a true/false value because it contains multiple tokens.";
				return false;
			}
			LexTokenToken lexTokenToken = parsed.GetTokenPlaceholders(recursive: false).Single();
			IToken token = tokenParser.Context.GetToken(lexTokenToken.Name, enforceContext: false);
			IInputArguments input = tokenParser.CreateInputArgs(tokenParser.CreateTokenStringOrNull(lexTokenToken.InputArgs?.Parts, tokenParser.Context, path.With("input")));
			if (token == null)
			{
				error = "unknown token '" + lexTokenToken.Name + "'.";
				return false;
			}
			if (!token.BypassesContextValidation)
			{
				if (!token.HasBoundedValues(input, out IInvariantSet allowedValues) || !allowedValues.All((string p) => bool.TryParse(p, out var _)))
				{
					error = "that token isn't restricted to 'true' or 'false'.";
					return false;
				}
				if (token.CanHaveMultipleValues(input))
				{
					error = "can't be treated as a true/false value because that token can have multiple values.";
					return false;
				}
			}
		}
		return true;
	}

	private bool TryParseEnabled(string? rawValue, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, out bool parsed)
	{
		parsed = false;
		if (!TryParseBoolean(rawValue, tokenParser, assumeModIds, path, out error, out IManagedTokenString parsed2))
		{
			return false;
		}
		if (parsed2.HasAnyTokens)
		{
			error = "cannot contain tokens.";
			return false;
		}
		if (!bool.TryParse(rawValue, out parsed))
		{
			error = "can't parse " + parsed2.Raw + " as a true/false value.";
			return false;
		}
		return true;
	}

	private bool TryParsePosition(PatchPositionConfig? raw, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out TokenPosition? parsed)
	{
		if (raw == null)
		{
			error = "the tile position is required";
			parsed = null;
			return false;
		}
		if (!TryParseField(raw.X, "X", out var result, out error) || !TryParseField(raw.Y, "Y", out var result2, out error))
		{
			parsed = null;
			return false;
		}
		parsed = new TokenPosition(result, result2);
		return true;
		bool TryParseField(string? rawField, string name, [NotNullWhen(true)] out IManagedTokenString? parsed2, [NotNullWhen(false)] out string? parseError)
		{
			if (!TryParseInt(rawField, tokenParser, assumeModIds, path.With(name), out parseError, out parsed2))
			{
				parseError = "invalid " + name + ": " + parseError;
				return false;
			}
			return true;
		}
	}

	private bool TryParseRectangle(PatchRectangleConfig raw, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out TokenRectangle? parsed)
	{
		if (!TryParseField(raw.X, "X", out var result, out error) || !TryParseField(raw.Y, "Y", out var result2, out error) || !TryParseField(raw.Width, "Width", out var result3, out error) || !TryParseField(raw.Height, "Height", out var result4, out error))
		{
			parsed = null;
			return false;
		}
		parsed = new TokenRectangle(result, result2, result3, result4);
		return true;
		bool TryParseField(string? rawField, string name, [NotNullWhen(true)] out IManagedTokenString? parsed2, [NotNullWhen(false)] out string? parseError)
		{
			if (!TryParseInt(rawField, tokenParser, assumeModIds, path.With(name), out parseError, out parsed2))
			{
				parseError = "invalid " + name + ": " + parseError;
				return false;
			}
			return true;
		}
	}

	private bool TryParseInt(string? rawString, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out IManagedTokenString? parsed)
	{
		parsed = null;
		if (!tokenParser.TryParseString(rawString, assumeModIds, path, out error, out IManagedTokenString parsed2))
		{
			return false;
		}
		if (parsed2.HasAnyTokens)
		{
			if (!parsed2.IsSingleTokenOnly)
			{
				error = "can't be treated as a number because it contains multiple tokens.";
				return false;
			}
			LexTokenToken lexTokenToken = parsed2.GetTokenPlaceholders(recursive: false).Single();
			IToken token = tokenParser.Context.GetToken(lexTokenToken.Name, enforceContext: false);
			if (token == null)
			{
				error = "unknown token " + lexTokenToken.Name;
				return false;
			}
			IInputArguments input = tokenParser.CreateInputArgs(tokenParser.CreateTokenStringOrNull(lexTokenToken.InputArgs?.Parts, tokenParser.Context, path.With("input")));
			if (!token.BypassesContextValidation)
			{
				if (!token.HasBoundedRangeValues(input, out var _, out var _) && (!token.HasBoundedValues(input, out IInvariantSet allowedValues) || !allowedValues.All((string p) => int.TryParse(p, out var _))))
				{
					error = "that token isn't restricted to integers.";
					return false;
				}
				if (token.CanHaveMultipleValues(input))
				{
					error = "can't be treated as a number because that token can have multiple values.";
					return false;
				}
			}
		}
		parsed = parsed2;
		return true;
	}

	private bool TryPrepareLocalAsset(string? path, TokenParser tokenParser, IInvariantSet assumeModIds, LogPathBuilder logPath, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out IManagedTokenString? tokenedPath, Func<LexTokenToken, string?>? preValidate = null)
	{
		path = path?.Trim();
		if (string.IsNullOrWhiteSpace(path))
		{
			error = "must set the FromFile field for this action type.";
			tokenedPath = null;
			return false;
		}
		if (!tokenParser.TryParseString(path, assumeModIds, logPath, out string error2, out tokenedPath, preValidate))
		{
			error = "the FromFile is invalid: " + error2;
			tokenedPath = null;
			return false;
		}
		error = null;
		return true;
	}

	private bool IsDescendant(IPatch parent, IPatch child)
	{
		for (IPatch parentPatch = child.ParentPatch; parentPatch != null; parentPatch = parentPatch.ParentPatch)
		{
			if (parentPatch == parent)
			{
				return true;
			}
		}
		return false;
	}

	private int GetConditionParseOrder(string key, string? value)
	{
		if (key.Contains("HasMod"))
		{
			return (!Lexer.MightContainTokens(key) && !Lexer.MightContainTokens(value)) ? 1 : 2;
		}
		return 3;
	}
}
