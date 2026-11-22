using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Locations;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Commands;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Commands.Commands;

internal class SummaryCommand : BaseCommand
{
	private readonly IGameContentHelper ContentHelper;

	private readonly Func<PatchManager> GetPatchManager;

	private readonly Func<TokenManager> GetTokenManager;

	private readonly Func<CustomLocationManager> GetCustomLocationLoader;

	private readonly Regex AssetNameWithContentPattern = new Regex("^Content[/\\\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private readonly Regex AssetNameWithExtensionPattern = new Regex("(\\.\\w+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private readonly Regex AssetNameWithLocalePattern = new Regex("^\\.(?:de-DE|es-ES|ja-JP|pt-BR|ru-RU|zh-CN)(?:\\.xnb)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private readonly HashSet<ConditionType> SortTokens = new HashSet<ConditionType>
	{
		ConditionType.HasActiveQuest,
		ConditionType.HasCaughtFish,
		ConditionType.HasCookingRecipe,
		ConditionType.HasCraftingRecipe,
		ConditionType.HasConversationTopic,
		ConditionType.HasDialogueAnswer,
		ConditionType.HasFlag,
		ConditionType.HasProfession,
		ConditionType.HasReadLetter,
		ConditionType.HasSeenEvent,
		ConditionType.SkillLevel
	};

	public SummaryCommand(IMonitor monitor, IGameContentHelper contentHelper, Func<PatchManager> getPatchManager, Func<TokenManager> getTokenManager, Func<CustomLocationManager> getCustomLocationLoader)
		: base(monitor, "summary")
	{
		ContentHelper = contentHelper;
		GetPatchManager = getPatchManager;
		GetTokenManager = getTokenManager;
		GetCustomLocationLoader = getCustomLocationLoader;
	}

	public override string GetDescription()
	{
		return "patch summary\n   Usage: patch summary\n   Shows a summary of the current conditions and loaded patches.\n\n   Usage: patch summary \"<content pack ID>\"\n   Show a summary of the current conditions, and loaded patches for the given content pack.\n\n   You can specify any number of optional flags (e.g. `patch summary full asset \"Data/Crops\"`):\n      - asset \"<asset name>\":\n           Only list changes to the given asset. This filters by base asset name, so 'Data/furniture' also matches\n           edits to 'Data/furniture.fr-FR'. You can list multiple assets by repeating the flag.\n\n      - full:\n           Don't truncate very long token values.\n\n      - unsorted:\n           Don't sort the values for display. This is mainly useful for checking the real order for `valueAt`.";
	}

	public override void Handle(string[] args)
	{
		PatchManager patchManager = GetPatchManager();
		TokenManager tokenManager = GetTokenManager();
		StringBuilder stringBuilder = new StringBuilder();
		LogPathBuilder path = new LogPathBuilder("console command");
		bool showFull = false;
		bool sort = true;
		MutableInvariantSet forModIds = new MutableInvariantSet();
		HashSet<string> onlyAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < args.Length; i++)
		{
			switch (args[i].ToLower())
			{
			case "asset":
			{
				string text = ArgUtility.Get(args, i + 1, (string)null, false);
				if (text == null)
				{
					stringBuilder.AppendLine("When using the 'assets' argument, you must specify an asset name to filter for.");
					return;
				}
				IAssetName val = ContentHelper.ParseAssetName(text);
				onlyAssets.Add(val.BaseName);
				i++;
				break;
			}
			case "full":
				showFull = true;
				break;
			case "unsorted":
				sort = false;
				break;
			default:
				forModIds.Add(args[i]);
				break;
			}
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("=====================");
		stringBuilder.AppendLine("==  Global tokens  ==");
		stringBuilder.AppendLine("=====================");
		var array = (from _003C_003Eh__TransparentIdentifier2 in (from token in tokenManager.GetTokens(enforceContext: false).OrderByHuman((IToken p) => p.Name)
				let inputArgs = token.GetAllowedInputArguments()
				select new
				{
					_003C_003Eh__TransparentIdentifier0 = _003C_003Eh__TransparentIdentifier0,
					rootValues = ((!token.RequiresInput) ? GetValues(token, InputArguments.Empty, sort).ToArray() : Array.Empty<string>())
				}).Select(_003C_003Eh__TransparentIdentifier1 =>
			{
				IInvariantSet inputArgs = inputArgs;
				int isMultiValue;
				if ((inputArgs == null || inputArgs.Count <= 1) && rootValues.Length <= 1)
				{
					IInvariantSet inputArgs2 = inputArgs;
					isMultiValue = ((inputArgs2 != null && inputArgs2.Count == 1 && GetValues(token, new InputArguments(new LiteralString(inputArgs.First(), path.With(token.Name, "input"))), sort).Count() > 1) ? 1 : 0);
				}
				else
				{
					isMultiValue = 1;
				}
				return new
				{
					_003C_003Eh__TransparentIdentifier1 = _003C_003Eh__TransparentIdentifier1,
					isMultiValue = ((byte)isMultiValue != 0)
				};
			})
			let mod = (token as ModProvidedToken)?.Mod
			orderby isMultiValue
			select new
			{
				Mod = mod,
				Token = token
			}).GroupBy(p =>
		{
			IManifest mod = p.Mod;
			return (mod != null) ? mod.Name.Trim() : null;
		}).OrderByHuman(p => p.Key).ToArray();
		int totalWidth = Math.Max(array.Max(group => group.Max(p => p.Token.Name.Length)), "token name".Length);
		var array2 = array;
		foreach (var grouping in array2)
		{
			if (grouping.Key != null && !MatchesModFilter(grouping.First().Mod.UniqueID))
			{
				continue;
			}
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 1, stringBuilder2);
			handler.AppendLiteral("   ");
			handler.AppendFormatted(grouping.Key ?? "Content Patcher");
			handler.AppendLiteral(":");
			stringBuilder3.AppendLine(ref handler);
			stringBuilder.AppendLine();
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
			handler.AppendLiteral("      ");
			handler.AppendFormatted("token name".PadRight(totalWidth));
			handler.AppendLiteral(" | value");
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
			handler.AppendLiteral("      ");
			handler.AppendFormatted("".PadRight(totalWidth, '-'));
			handler.AppendLiteral(" | -----");
			stringBuilder5.AppendLine(ref handler);
			foreach (IToken item3 in grouping.Select(p => p.Token))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(9, 1, stringBuilder2);
				handler.AppendLiteral("      ");
				handler.AppendFormatted(item3.Name.PadRight(totalWidth));
				handler.AppendLiteral(" | ");
				stringBuilder6.Append(ref handler);
				if (!item3.IsReady)
				{
					stringBuilder.AppendLine("[ ] n/a");
				}
				else if (item3.RequiresInput)
				{
					IInvariantSet allowedInputArguments = item3.GetAllowedInputArguments();
					if (allowedInputArguments != null && allowedInputArguments.Any())
					{
						bool flag = true;
						foreach (string item4 in allowedInputArguments.OrderByHuman())
						{
							if (flag)
							{
								stringBuilder.Append("[X] ");
								flag = false;
							}
							else
							{
								stringBuilder2 = stringBuilder;
								StringBuilder stringBuilder7 = stringBuilder2;
								handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
								handler.AppendLiteral("      ");
								handler.AppendFormatted("".PadRight(totalWidth, ' '));
								handler.AppendLiteral(" |     ");
								stringBuilder7.Append(ref handler);
							}
							stringBuilder2 = stringBuilder;
							StringBuilder stringBuilder8 = stringBuilder2;
							handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder2);
							handler.AppendLiteral(":");
							handler.AppendFormatted(item4);
							handler.AppendLiteral(": ");
							handler.AppendFormatted(GetTruncatedTokenValues(GetValues(item3, new InputArguments(new LiteralString(item4, path.With(item3.Name, "input"))), sort)));
							stringBuilder8.AppendLine(ref handler);
						}
					}
					else
					{
						stringBuilder.AppendLine("[X] (token returns a dynamic value)");
					}
				}
				else
				{
					stringBuilder.AppendLine("[X] " + GetTruncatedTokenValues(GetValues(item3, InputArguments.Empty, sort)));
				}
			}
			stringBuilder.AppendLine();
		}
		if (!onlyAssets.Any())
		{
			IGrouping<string, CustomLocationData>[] array3 = (from p in GetCustomLocationLoader().GetCustomLocationData()
				where MatchesModFilter(p.ModId)
				select p).GroupByIgnoreCase((CustomLocationData p) => p.ModName).OrderByHuman((IGrouping<string, CustomLocationData> p) => p.Key).ToArray();
			if (array3.Any())
			{
				stringBuilder.AppendLine("======================\n== Custom locations ==\n======================\nThe following custom locations were created by content packs.\n" + (forModIds.Any() ? $"\n(Filtered to content pack ID{((forModIds.Count > 1) ? "s" : "")}: {string.Join(", ", forModIds.OrderByHuman())}.)\n" : ""));
				IGrouping<string, CustomLocationData>[] array4 = array3;
				foreach (IGrouping<string, CustomLocationData> grouping2 in array4)
				{
					int totalWidth2 = Math.Max("location name".Length, grouping2.Max((CustomLocationData p) => p.Name.Length));
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder9 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
					handler.AppendFormatted(grouping2.Key);
					handler.AppendLiteral(":");
					stringBuilder9.AppendLine(ref handler);
					stringBuilder.AppendLine("".PadRight(grouping2.Key.Length + 1, '-'));
					stringBuilder.AppendLine();
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder10 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
					handler.AppendLiteral("   ");
					handler.AppendFormatted("location name".PadRight(totalWidth2));
					handler.AppendLiteral("  | status");
					stringBuilder10.AppendLine(ref handler);
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder11 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
					handler.AppendLiteral("   ");
					handler.AppendFormatted("".PadRight(totalWidth2, '-'));
					handler.AppendLiteral("  | ------");
					stringBuilder11.AppendLine(ref handler);
					foreach (CustomLocationData item5 in grouping2.OrderByHuman((CustomLocationData p) => p.Name))
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder12 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(7, 2, stringBuilder2);
						handler.AppendLiteral("   ");
						handler.AppendFormatted(item5.Name.PadRight(totalWidth2));
						handler.AppendLiteral("  | ");
						handler.AppendFormatted(item5.IsEnabled ? "[X] ok" : ("[ ] error: " + item5.Error));
						stringBuilder12.AppendLine(ref handler);
					}
					stringBuilder.AppendLine();
				}
			}
			stringBuilder.AppendLine();
		}
		IGrouping<string, PatchInfo>[] array5 = (from p in GetAllPatches(patchManager)
			where MatchesModFilter(p.ContentPack.Manifest.UniqueID) && MatchesAssetFilter(p.ParsedTargetAsset?.Value)
			select p).GroupByIgnoreCase((PatchInfo p) => p.ContentPack.Manifest.Name).OrderByHuman((IGrouping<string, PatchInfo> p) => p.Key).ToArray();
		stringBuilder.AppendLine("=====================\n== Content patches ==\n=====================\nThe following patches were loaded. For each patch:\n  - 'loaded' shows whether the patch is loaded and enabled (see details for the reason if not).\n  - 'conditions' shows whether the patch matches with the current conditions (see details for the reason if not). If this is unexpectedly false, check (a) the conditions above and (b) your Where field.\n  - 'applied' shows whether the target asset was loaded and patched. If you expected it to be loaded by this point but it's false, double-check (a) that the game has actually loaded the asset yet, and (b) your Targets field is correct.\n" + (forModIds.Any() ? $"\n(Filtered to content pack ID{((forModIds.Count > 1) ? "s" : "")}: {string.Join(", ", forModIds.OrderByHuman())}.)\n" : "") + (onlyAssets.Any() ? $"\n(Filtered to asset name{((onlyAssets.Count > 1) ? "s" : "")}: {string.Join(", ", onlyAssets.OrderByHuman())}.)\n" : "") + "\n");
		IGrouping<string, PatchInfo>[] array6 = array5;
		foreach (IGrouping<string, PatchInfo> patchGroup in array6)
		{
			ModTokenContext modTokenContext = tokenManager.TrackLocalTokens(patchGroup.First().ContentPack);
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder13 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder2);
			handler.AppendFormatted(patchGroup.Key);
			handler.AppendLiteral(":");
			stringBuilder13.AppendLine(ref handler);
			stringBuilder.AppendLine("".PadRight(patchGroup.Key.Length + 1, '-'));
			var array7 = (from _003C_003Eh__TransparentIdentifier0 in (from IToken token in modTokenContext.GetTokens(enforceContext: false)
					where token.Scope != null
					select token).Select((IToken token) =>
				{
					IToken token2 = token;
					object validInputs;
					if (!token.IsReady || !token.RequiresInput)
					{
						IEnumerable<ITokenString> enumerable = new _003C_003Ez__ReadOnlySingleElementList<ITokenString>(null);
						validInputs = enumerable;
					}
					else
					{
						IInvariantSet? allowedInputArguments2 = token.GetAllowedInputArguments();
						validInputs = ((allowedInputArguments2 != null) ? ((IEnumerable<ITokenString>)allowedInputArguments2.Select((string p) => new LiteralString(p, path.With(patchGroup.Key, token.Name, "input '" + p + "'")))).AsEnumerable() : null) ?? Array.Empty<ITokenString>();
					}
					return new
					{
						token = token2,
						validInputs = (IEnumerable<ITokenString>)validInputs
					};
				})
				from input in validInputs.Cast<ITokenString>()
				where !token.RequiresInput || validInputs.Any()
				let result = new
				{
					Name = (token.RequiresInput ? $"{token.Name}:{input}" : token.Name),
					Values = (token.IsReady ? GetValues(token, (input != null) ? new InputArguments(input) : InputArguments.Empty, sort).ToArray() : Array.Empty<string>()),
					IsReady = token.IsReady
				}
				orderby result.Name
				select result).ToArray();
			if (array7.Any())
			{
				int totalWidth3 = Math.Max(array7.Max(p => p.Name.Length), "token name".Length);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("   Local tokens:");
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder14 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
				handler.AppendLiteral("      ");
				handler.AppendFormatted("token name".PadRight(totalWidth3));
				handler.AppendLiteral(" | value");
				stringBuilder14.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder15 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(14, 1, stringBuilder2);
				handler.AppendLiteral("      ");
				handler.AppendFormatted("".PadRight(totalWidth3, '-'));
				handler.AppendLiteral(" | -----");
				stringBuilder15.AppendLine(ref handler);
				var array8 = array7;
				foreach (var anon in array8)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder16 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(12, 3, stringBuilder2);
					handler.AppendLiteral("      ");
					handler.AppendFormatted(anon.Name.PadRight(totalWidth3));
					handler.AppendLiteral(" | [");
					handler.AppendFormatted(anon.IsReady ? "X" : " ");
					handler.AppendLiteral("] ");
					handler.AppendFormatted(GetTruncatedTokenValues(anon.Values));
					stringBuilder16.AppendLine(ref handler);
				}
			}
			int totalWidth4 = Math.Max("priority".Length, patchGroup.Max((PatchInfo p) => GetDisplayPriority(p)?.Length ?? 0));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("   Patches:");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder17 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(56, 1, stringBuilder2);
			handler.AppendLiteral("      loaded  | conditions | applied | ");
			handler.AppendFormatted("priority".PadRight(totalWidth4));
			handler.AppendLiteral(" | name + details");
			stringBuilder17.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder18 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(56, 1, stringBuilder2);
			handler.AppendLiteral("      ------- | ---------- | ------- | ");
			handler.AppendFormatted("".PadRight(totalWidth4, '-'));
			handler.AppendLiteral(" | --------------");
			stringBuilder18.AppendLine(ref handler);
			string key;
			foreach (PatchInfo item6 in patchGroup.OrderBy((PatchInfo p) => p, new PatchDisplaySortComparer()))
			{
				string displayPriority = GetDisplayPriority(item6);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder19 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(39, 5, stringBuilder2);
				handler.AppendLiteral("      [");
				handler.AppendFormatted(item6.IsLoaded ? "X" : " ");
				handler.AppendLiteral("]     | [");
				handler.AppendFormatted(item6.MatchesContext ? "X" : " ");
				handler.AppendLiteral("]        | [");
				handler.AppendFormatted(item6.IsApplied ? "X" : " ");
				handler.AppendLiteral("]     | ");
				handler.AppendFormatted((displayPriority ?? "").PadRight(totalWidth4));
				handler.AppendLiteral(" | ");
				handler.AppendFormatted(item6.PathWithoutContentPackPrefix);
				stringBuilder19.Append(ref handler);
				string text2 = PathUtilities.NormalizeAssetName((item6.ParsedType == PatchType.Include) ? item6.RawFromAsset : item6.RawTargetAsset);
				ITokenString tokenString = ((item6.ParsedType == PatchType.Include) ? item6.ParsedFromAsset : item6.ParsedTargetAsset);
				string text3 = ((text2 != null && !StringExtensions.ContainsIgnoreCase(PathUtilities.NormalizeAssetName(item6.PathWithoutContentPackPrefix.ToString().Replace(" ", "")), text2.Replace(" ", ""))) ? ((item6.ParsedType?.ToString() ?? item6.RawType) + " " + text2) : null);
				string text4 = ((item6.MatchesContext && tokenString != null && tokenString.HasAnyTokens) ? PathUtilities.NormalizeAssetName(tokenString.Value) : null);
				if (text3 != null || text4 != null)
				{
					stringBuilder.Append(" (");
					if (text3 != null)
					{
						stringBuilder.Append(text3);
						if (text4 != null)
						{
							stringBuilder.Append(" ");
						}
					}
					if (text4 != null)
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder20 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(3, 1, stringBuilder2);
						handler.AppendLiteral("=> ");
						handler.AppendFormatted(text4);
						stringBuilder20.Append(ref handler);
					}
					stringBuilder.Append(")");
				}
				if (item6.TargetAssetLocale != null)
				{
					stringBuilder.Append(string.IsNullOrWhiteSpace(item6.TargetAssetLocale) ? " (locale: base only)" : (" (locale: " + item6.TargetAssetLocale + " only)"));
				}
				string reasonNotLoaded = item6.GetReasonNotLoaded();
				if (reasonNotLoaded != null)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder21 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
					handler.AppendLiteral("  // ");
					handler.AppendFormatted(reasonNotLoaded);
					stringBuilder21.Append(ref handler);
				}
				if (reasonNotLoaded == null && item6 != null && item6.IsLoaded && !item6.IsApplied && item6.ParsedTargetAsset.IsMeaningful())
				{
					string value = item6.ParsedTargetAsset.Value;
					List<string> list = new List<string>();
					if (AssetNameWithContentPattern.IsMatch(value))
					{
						list.Add("shouldn't include 'Content/' prefix");
					}
					if (AssetNameWithExtensionPattern.IsMatch(value))
					{
						Match match = AssetNameWithExtensionPattern.Match(value);
						list.Add($"shouldn't include '{match.Captures[0]}' extension");
					}
					if (AssetNameWithLocalePattern.IsMatch(value))
					{
						list.Add("shouldn't include language code (use conditions instead)");
					}
					if (list.Any())
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder22 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(41, 1, stringBuilder2);
						handler.AppendLiteral(" // hint: asset name may be incorrect (");
						handler.AppendFormatted(string.Join("; ", list));
						handler.AppendLiteral(").");
						stringBuilder22.Append(ref handler);
					}
				}
				if (item6.Patch != null)
				{
					Tuple<UpdateRate, string, IInvariantSet>[] tokensWithSpecialUpdateRates = tokenManager.TokensWithSpecialUpdateRates;
					for (int num5 = 0; num5 < tokensWithSpecialUpdateRates.Length; num5++)
					{
						tokensWithSpecialUpdateRates[num5].Deconstruct(out var item, out key, out var item2);
						UpdateRate updateRate = item;
						string value2 = key;
						IInvariantSet source = item2;
						if (!item6.Patch.UpdateRate.HasFlag(updateRate))
						{
							IInvariantSet tokensUsed = item6.Patch.GetTokensUsed();
							string[] source2 = source.Where((string p) => tokensUsed.Contains(p)).ToArray();
							if (source2.Any())
							{
								stringBuilder2 = stringBuilder;
								StringBuilder stringBuilder23 = stringBuilder2;
								handler = new StringBuilder.AppendInterpolatedStringHandler(46, 3, stringBuilder2);
								handler.AppendLiteral(" // hint: patch uses ");
								handler.AppendFormatted(value2);
								handler.AppendLiteral(", but doesn't set \"");
								handler.AppendFormatted("Update");
								handler.AppendLiteral("\": \"");
								handler.AppendFormatted(updateRate);
								handler.AppendLiteral("\".");
								stringBuilder23.Append(ref handler);
							}
						}
					}
				}
				stringBuilder.AppendLine();
			}
			Dictionary<string, MutableInvariantSet> dictionary = new Dictionary<string, MutableInvariantSet>(StringComparer.OrdinalIgnoreCase);
			foreach (PatchInfo item7 in patchGroup)
			{
				if (!item7.IsApplied || item7.Patch == null)
				{
					continue;
				}
				string[] source3 = item7.GetChangeLabels().ToArray();
				if (!source3.Any())
				{
					continue;
				}
				string displayTarget = item7.GetDisplayTarget();
				if (displayTarget != null)
				{
					if (!dictionary.TryGetValue(displayTarget, out var value3))
					{
						value3 = (dictionary[displayTarget] = new MutableInvariantSet());
					}
					CollectionExtensions.AddRange<string>((ISet<string>)value3, item7.GetChangeLabels());
				}
			}
			stringBuilder.AppendLine();
			if (dictionary.Any())
			{
				int num6 = Math.Max("asset name".Length, dictionary.Max((KeyValuePair<string, MutableInvariantSet> p) => p.Key.Length));
				stringBuilder.AppendLine("   Current changes:");
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder24 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
				handler.AppendLiteral("      asset name");
				handler.AppendFormatted("".PadRight(num6 - "asset name".Length));
				handler.AppendLiteral(" | changes");
				stringBuilder24.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder25 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
				handler.AppendLiteral("      ----------");
				handler.AppendFormatted("".PadRight(num6 - "----------".Length, '-'));
				handler.AppendLiteral(" | -------");
				stringBuilder25.AppendLine(ref handler);
				foreach (KeyValuePair<string, MutableInvariantSet> item8 in dictionary.OrderByHuman((KeyValuePair<string, MutableInvariantSet> p) => p.Key))
				{
					item8.Deconstruct(out key, out var value4);
					string text5 = key;
					MutableInvariantSet source4 = value4;
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder26 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(9, 3, stringBuilder2);
					handler.AppendLiteral("      ");
					handler.AppendFormatted(text5);
					handler.AppendFormatted("".PadRight(num6 - text5.Length));
					handler.AppendLiteral(" | ");
					handler.AppendFormatted(string.Join("; ", source4.OrderByHuman()));
					stringBuilder26.AppendLine(ref handler);
				}
			}
			else
			{
				stringBuilder.AppendLine("   No current changes.");
			}
			stringBuilder.AppendLine();
		}
		Monitor.Log(stringBuilder.ToString(), (LogLevel)1);
		string GetTruncatedTokenValues(IEnumerable<string> values)
		{
			string text6 = string.Join(", ", values);
			return (showFull || text6.Length <= 200) ? text6 : (text6.Substring(0, 200 - "... (use `patch summary full` to see other values)".Length) + "... (use `patch summary full` to see other values)");
		}
		bool MatchesAssetFilter(string? assetName)
		{
			if (onlyAssets.Count == 0)
			{
				return true;
			}
			if (string.IsNullOrWhiteSpace(assetName))
			{
				return false;
			}
			IAssetName val2 = ContentHelper.ParseAssetName(assetName);
			return onlyAssets.Contains(val2.BaseName);
		}
		bool MatchesModFilter(string modId)
		{
			return forModIds.Count == 0 || forModIds.Contains(modId);
		}
	}

	private IEnumerable<PatchInfo> GetAllPatches(PatchManager patchManager)
	{
		foreach (IPatch patch in patchManager.GetPatches())
		{
			yield return new PatchInfo(patch);
		}
		foreach (DisabledPatch patch2 in patchManager.GetPermanentlyDisabledPatches())
		{
			yield return new PatchInfo(patch2);
		}
	}

	private IEnumerable<string> GetValues(IToken token, IInputArguments input, bool sort)
	{
		if (!token.IsReady)
		{
			return Array.Empty<string>();
		}
		IEnumerable<string> enumerable = token.GetValues(input);
		if (sort && Enum.TryParse<ConditionType>(token.Name, ignoreCase: true, out var result) && SortTokens.Contains(result))
		{
			enumerable = enumerable.OrderByHuman();
		}
		return enumerable;
	}

	private string? GetDisplayPriority(PatchInfo patch)
	{
		if (patch.Priority.HasValue)
		{
			switch (patch.ParsedType)
			{
			case PatchType.Load:
				return GetDisplayPriority<AssetLoadPriority>(patch.Priority.Value);
			case PatchType.EditImage:
			case PatchType.EditData:
			case PatchType.EditMap:
				return GetDisplayPriority<AssetEditPriority>(patch.Priority.Value);
			}
		}
		return null;
	}

	private string GetDisplayPriority<TPriority>(int value) where TPriority : struct, Enum
	{
		if (Enum.IsDefined(typeof(TPriority), value))
		{
			return ((TPriority)(object)value/*cast due to constrained. prefix*/).ToString();
		}
		int num = 0;
		string text = null;
		TPriority[] values = Enum.GetValues<TPriority>();
		foreach (TPriority val in values)
		{
			int num2 = value - (int)(object)val;
			int num3 = Math.Abs(num2);
			if (text == null || num3 < num)
			{
				text = $"{val} {((num2 > 0) ? "+" : "-")} {num3}";
				num = num3;
			}
		}
		return text ?? value.ToString();
	}
}
