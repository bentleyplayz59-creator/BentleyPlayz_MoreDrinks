using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using ContentPatcher.Framework.Validators;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.Profiler;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using xTile;

namespace ContentPatcher.Framework;

internal class PatchManager
{
	private readonly TokenManager TokenManager;

	private readonly IMonitor Monitor;

	private readonly IAssetValidator[] AssetValidators;

	private readonly List<DisabledPatch> PermanentlyDisabledPatches = new List<DisabledPatch>();

	private readonly SortedSet<IPatch> Patches = new SortedSet<IPatch>(PatchIndexComparer.Instance);

	private readonly InvariantDictionary<HashSet<IPatch>> PatchesAffectedByToken = new InvariantDictionary<HashSet<IPatch>>();

	private readonly Dictionary<IAssetName, SortedSet<IPatch>> PatchesByCurrentTarget = new Dictionary<IAssetName, SortedSet<IPatch>>();

	private readonly Dictionary<IPatch, IndexedPatchValues> IndexedPatchValues = new Dictionary<IPatch, IndexedPatchValues>(new ObjectReferenceComparer<IPatch>());

	private readonly HashSet<IPatch> PendingPatches = new HashSet<IPatch>();

	private readonly HashSet<IAssetName> AssetsWithRemovedPatches = new HashSet<IAssetName>();

	private readonly Dictionary<ContextUpdateType, MutableInvariantSet> QueuedTokenChanges = new Dictionary<ContextUpdateType, MutableInvariantSet>
	{
		[ContextUpdateType.OnTimeChange] = new MutableInvariantSet(),
		[ContextUpdateType.OnLocationChange] = new MutableInvariantSet(),
		[ContextUpdateType.All] = new MutableInvariantSet()
	};

	private readonly ProfilerIntegration? Profiler;

	private readonly LocalizedContentManager RawFileContentManager;

	public PatchManager(IMonitor monitor, TokenManager tokenManager, IAssetValidator[] assetValidators, ProfilerIntegration profiler)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected Obj, but got Unknown
		Monitor = monitor;
		TokenManager = tokenManager;
		AssetValidators = assetValidators;
		RawFileContentManager = new LocalizedContentManager(((ContentManager)Game1.content).ServiceProvider, ((ContentManager)Game1.content).RootDirectory);
		Profiler = (profiler.IsLoaded ? profiler : null);
	}

	public void OnAssetRequested(AssetRequestedEventArgs e, bool ignoreLoadPatches)
	{
		LoadPatch[] array = ((!ignoreLoadPatches) ? GetCurrentLoaders(e).ToArray() : Array.Empty<LoadPatch>());
		IPatch[] array2 = GetCurrentEditors(e).ToArray();
		if (array.Any() || array2.Any())
		{
			MethodInfo methodInfo = GetType().GetMethod("ApplyPatchesToAsset", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(e.DataType);
			methodInfo.Invoke(this, new object[3] { e, array, array2 });
		}
	}

	public void UpdateContext(IGameContentHelper contentHelper, IInvariantSet globalChangedTokens, ContextUpdateType updateType)
	{
		bool verbose = Monitor.IsVerbose;
		if (verbose)
		{
			Monitor.Log($"Updating context for {updateType} tick...", (LogLevel)0);
		}
		MutableInvariantSet mutableInvariantSet = new MutableInvariantSet(globalChangedTokens);
		if (updateType == ContextUpdateType.All)
		{
			foreach (MutableInvariantSet value in QueuedTokenChanges.Values)
			{
				mutableInvariantSet.UnionWith(value);
				value.Clear();
			}
		}
		else
		{
			foreach (var (contextUpdateType2, mutableInvariantSet3) in QueuedTokenChanges)
			{
				if (contextUpdateType2 != updateType)
				{
					CollectionExtensions.AddRange<string>((ISet<string>)mutableInvariantSet3, (IEnumerable<string>)globalChangedTokens);
				}
			}
			CollectionExtensions.AddRange<string>((ISet<string>)mutableInvariantSet, (IEnumerable<string>)QueuedTokenChanges[updateType]);
			QueuedTokenChanges[updateType].Clear();
		}
		globalChangedTokens = mutableInvariantSet.Lock();
		Queue<IPatch> queue = new Queue<IPatch>(GetPatchesToUpdate(globalChangedTokens, updateType));
		HashSet<IAssetName> reloadAssetNames = new HashSet<IAssetName>(AssetsWithRemovedPatches);
		if (!queue.Any() && !reloadAssetNames.Any())
		{
			return;
		}
		IPatch[] source = PendingPatches.ToArray();
		PendingPatches.Clear();
		AssetsWithRemovedPatches.Clear();
		List<PatchAuditChange> list = (verbose ? new List<PatchAuditChange>() : null);
		IAssetName val = null;
		HashSet<IPatch> hashSet = new HashSet<IPatch>(new ObjectReferenceComparer<IPatch>());
		while (queue.Any())
		{
			IPatch patch = queue.Dequeue();
			if (verbose && val != patch.TargetAsset)
			{
				Monitor.Log($"   {patch.TargetAsset}:", (LogLevel)0);
				val = patch.TargetAsset;
			}
			string fromAsset = patch.FromAsset;
			IAssetName targetAsset = patch.TargetAsset;
			bool flag = patch.IsReady && !Enumerable.Contains(source, patch);
			ModTokenContext modTokenContext = TokenManager.TrackLocalTokens(patch.ContentPack);
			bool flag2;
			try
			{
				flag2 = patch.UpdateContext(modTokenContext);
			}
			catch (Exception ex)
			{
				Monitor.Log($"Patch error: {patch.Path} failed on context update (see log file for details).\n{ex.Message}", (LogLevel)4);
				Monitor.Log(ex.ToString(), (LogLevel)0);
				flag2 = false;
			}
			bool isReady = patch.IsReady;
			switch (patch.Type)
			{
			case PatchType.EditData:
				if (patch.FromAsset != fromAsset)
				{
					IndexPatch(patch, TokenManager.TrackLocalTokens(patch.ContentPack));
				}
				break;
			case PatchType.Include:
				if (!(patch is IncludePatch { PatchesJustLoaded: not null } includePatch))
				{
					break;
				}
				foreach (IPatch item in includePatch.PatchesJustLoaded)
				{
					hashSet.Add(item);
					queue.Enqueue(item);
				}
				break;
			case PatchType.Load:
				if (patch is LoadPatch loadPatch && patch.IsReady && !patch.FromAssetExists())
				{
					Monitor.Log($"Patch error: {patch.Path} has a {"FromFile"} which matches non-existent file '{loadPatch.FromAsset}'.", (LogLevel)4);
				}
				break;
			}
			flag2 = flag2 || hashSet.Contains(patch);
			bool flag3 = isReady != flag || (isReady & flag2);
			if (flag3)
			{
				patch.IsApplied = false;
				if (flag && targetAsset != null)
				{
					reloadAssetNames.Add(targetAsset);
				}
				if (isReady && patch.TargetAsset != null)
				{
					reloadAssetNames.Add(patch.TargetAsset);
				}
			}
			if ((targetAsset != null) ? (!targetAsset.IsEquivalentTo(patch.TargetAsset, false)) : (patch.TargetAsset != null))
			{
				IndexPatch(patch, modTokenContext);
			}
			if (verbose)
			{
				list.Add(new PatchAuditChange(patch, flag, fromAsset, targetAsset, flag3));
				List<string> list2 = new List<string>();
				if (flag != isReady)
				{
					list2.Add(isReady ? "enabled" : "disabled");
				}
				if (targetAsset != patch.TargetAsset)
				{
					list2.Add($"target: {targetAsset} => {patch.TargetAsset}");
				}
				string text = string.Join(", ", list2);
				Monitor.Log($"      [{(isReady ? "X" : " ")}] {patch.Path}: {(list2.Any() ? text : "OK")}", (LogLevel)0);
			}
		}
		if (list != null && list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(34, 2, stringBuilder2);
			handler.AppendFormatted(list.Count);
			handler.AppendLiteral(" patches were rechecked for ");
			handler.AppendFormatted(updateType);
			handler.AppendLiteral(" tick.");
			stringBuilder3.AppendLine(ref handler);
			foreach (PatchAuditChange item2 in list.OrderByHuman((PatchAuditChange p) => p.Patch.Path.ToString()))
			{
				IPatch patch2 = item2.Patch;
				List<string> list3 = new List<string>();
				if (item2.WillInvalidate)
				{
					IEnumerable<IAssetName> source2 = ((IEnumerable<IAssetName?>)new IAssetName[2] { item2.WasTargetAsset, patch2.TargetAsset }).WhereNotNull().Distinct();
					list3.Add("invalidates " + string.Join(", ", source2.Select((IAssetName p) => p.Name).OrderByHuman()));
				}
				if (item2.WasReady != patch2.IsReady)
				{
					list3.Add(patch2.IsReady ? "=> ready" : "=> not ready");
				}
				if (item2.WasFromAsset != patch2.FromAsset)
				{
					list3.Add($"{"FromAsset"} '{item2.WasFromAsset}' => '{patch2.FromAsset}'");
				}
				if (item2.WasTargetAsset != patch2.TargetAsset)
				{
					list3.Add($"{"TargetAsset"} '{item2.WasTargetAsset}' => '{patch2.TargetAsset}'");
				}
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
				handler.AppendLiteral("   - ");
				handler.AppendFormatted(patch2.Type);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(patch2.Path);
				stringBuilder4.AppendLine(ref handler);
				foreach (string item3 in list3)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder2);
					handler.AppendLiteral("      - ");
					handler.AppendFormatted(item3);
					stringBuilder5.AppendLine(ref handler);
				}
			}
			Monitor.Log(stringBuilder.ToString(), (LogLevel)0);
		}
		if (!reloadAssetNames.Any())
		{
			return;
		}
		if (verbose)
		{
			Monitor.Log($"   reloading {reloadAssetNames.Count} assets: {string.Join(", ", reloadAssetNames.OrderByHuman((IAssetName p) => p.Name))}", (LogLevel)0);
		}
		contentHelper.InvalidateCache((Func<IAssetInfo, bool>)((IAssetInfo asset) =>
		{
			bool flag4 = reloadAssetNames.Contains(asset.NameWithoutLocale);
			if (verbose)
			{
				Monitor.Log($"      [{(flag4 ? "X" : " ")}] reload {asset.Name}", (LogLevel)0);
			}
			return flag4;
		}));
	}

	public void Add(IPatch patch)
	{
		ModTokenContext modTokenContext = TokenManager.TrackLocalTokens(patch.ContentPack);
		patch.UpdateContext(modTokenContext);
		if (Monitor.IsVerbose)
		{
			Monitor.Log($"      added {patch.Type} {patch.TargetAsset}.", (LogLevel)0);
		}
		Patches.Add(patch);
		PendingPatches.Add(patch);
		IndexPatch(patch, modTokenContext);
	}

	public void Remove(IPatch patch)
	{
		if (Monitor.IsVerbose)
		{
			Monitor.Log($"      removed {patch.Path}.", (LogLevel)0);
		}
		if (Patches.Remove(patch))
		{
			if (patch != null && patch.IsApplied && patch.TargetAsset != null)
			{
				AssetsWithRemovedPatches.Add(patch.TargetAsset);
			}
			DeIndexPatch(patch);
		}
	}

	public void AddPermanentlyDisabled(DisabledPatch patch)
	{
		PermanentlyDisabledPatches.Add(patch);
	}

	public IEnumerable<IPatch> GetPatches()
	{
		return Patches;
	}

	public IEnumerable<IPatch> GetPatches(IAssetName assetName)
	{
		if (PatchesByCurrentTarget.TryGetValue(assetName, out SortedSet<IPatch> value))
		{
			return value;
		}
		return Array.Empty<IPatch>();
	}

	public IEnumerable<KeyValuePair<IAssetName, IEnumerable<IPatch>>> GetPatchesByTarget()
	{
		foreach (KeyValuePair<IAssetName, SortedSet<IPatch>> item in PatchesByCurrentTarget)
		{
			item.Deconstruct(out var key, out var value);
			ISet<IPatch> set = value;
			IAssetName assetName = key;
			ISet<IPatch> list = set;
			yield return new KeyValuePair<IAssetName, IEnumerable<IPatch>>(assetName, list);
		}
	}

	public IEnumerable<DisabledPatch> GetPermanentlyDisabledPatches()
	{
		return PermanentlyDisabledPatches;
	}

	public IEnumerable<LoadPatch> GetCurrentLoaders(AssetRequestedEventArgs request)
	{
		IAssetName assetName = request.Name;
		IAssetName nameWithoutLocale = request.NameWithoutLocale;
		bool skipLocalizedAssetByDefault = assetName.LocaleCode != null && (!RawFileContentManager.DoesAssetExist<object>($"{nameWithoutLocale}.fr-FR") || !RawFileContentManager.DoesAssetExist<object>($"{nameWithoutLocale}"));
		return (from patch in GetPatches(nameWithoutLocale)
			where patch.IsReady && ((patch.TargetLocale != null) ? string.Equals(patch.TargetLocale, assetName.LocaleCode, StringComparison.InvariantCultureIgnoreCase) : (!skipLocalizedAssetByDefault))
			select patch).OfType<LoadPatch>();
	}

	public IEnumerable<IPatch> GetCurrentEditors(AssetRequestedEventArgs request)
	{
		PatchType? patchType = GetEditType(request.DataType);
		if (!patchType.HasValue)
		{
			return Array.Empty<IPatch>();
		}
		return from patch in GetPatches(request.NameWithoutLocale)
			where patch.Type == patchType && patch.IsReady && (patch.TargetLocale == null || string.Equals(patch.TargetLocale, request.Name.LocaleCode, StringComparison.InvariantCultureIgnoreCase))
			select patch;
	}

	private void ApplyPatchesToAsset<T>(AssetRequestedEventArgs e, LoadPatch[] loaders, IPatch[] editors) where T : notnull
	{
		IAssetName assetName = e.NameWithoutLocale;
		LoadPatch loader = null;
		foreach (LoadPatch loadPatch in loaders)
		{
			if (!loadPatch.FromAssetExists())
			{
				Monitor.Log($"Can't apply load \"{loadPatch.Path}\" to {loadPatch.TargetAsset}: the {"FromFile"} file '{loadPatch.FromAsset}' doesn't exist.", (LogLevel)3);
			}
			else
			{
				if (loader?.Priority > loadPatch.Priority)
				{
					continue;
				}
				if (loadPatch.Priority == int.MaxValue)
				{
					LoadPatch loadPatch2 = loader;
					if (loadPatch2 != null && loadPatch2.Priority == int.MaxValue)
					{
						IPatch[] source = ((IEnumerable<IPatch>)loaders.Where((LoadPatch p) => p.Priority == int.MaxValue)).ToArray();
						string[] array = source.Select((IPatch p) => p.ContentPack.Manifest.Name).Distinct().OrderByHuman()
							.ToArray();
						string[] value = source.Select((IPatch p) => p.Path.ToString()).OrderByHuman().ToArray();
						switch (array.Length)
						{
						case 1:
							Monitor.Log($"'{array[0]}' has multiple patches with the '{"Exclusive"}' priority which load the '{assetName}' asset at the same time ({string.Join(", ", value)}). None will be applied. You should report this to the content pack author.", (LogLevel)4);
							break;
						case 2:
							Monitor.Log($"Two content packs want to load the '{assetName}' asset with the '{"Exclusive"}' priority ({string.Join(" and ", array)}). Neither will be applied. You should remove one of the content packs, or ask the authors about compatibility.", (LogLevel)4);
							Monitor.Log("Affected patches: " + string.Join(", ", value), (LogLevel)0);
							break;
						default:
							Monitor.Log($"Multiple content packs want to load the '{assetName}' asset with the '{"Exclusive"}' priority ({string.Join(", ", array)}). None will be applied. You should remove some of the content packs, or ask the authors about compatibility.", (LogLevel)4);
							Monitor.Log("Affected patches: " + string.Join(", ", value), (LogLevel)0);
							break;
						}
						loader = null;
						break;
					}
				}
				loader = loadPatch;
			}
		}
		if (loader != null)
		{
			e.LoadFrom((Func<object>)(() => ApplyLoad<T>(loader, assetName)), (AssetLoadPriority)loader.Priority, loader.ContentPack.Manifest.UniqueID);
		}
		if (editors.Any())
		{
			List<List<IPatch>> list = SortAndGroupEditPatches(editors);
			foreach (List<IPatch> item in list)
			{
				List<IPatch> patches = item;
				IPatch patch = patches[0];
				e.Edit((Action<IAssetData>)((IAssetData data) =>
				{
					ApplyEdits<T>(patches, data);
				}), (AssetEditPriority)patch.Priority, patch.ContentPack.Manifest.UniqueID);
			}
		}
		if (Monitor.IsVerbose)
		{
			Monitor.Log($"asset requested: can [{(loaders.Any() ? "X" : " ")}] load [{(editors.Any() ? "X" : " ")}] edit {assetName}", (LogLevel)0);
		}
	}

	private T? ApplyLoad<T>(LoadPatch patch, IAssetName assetName) where T : notnull
	{
		using (Profiler?.RecordSection(patch.ContentPack.Manifest.UniqueID, "ApplyLoad", patch.Path.ToString()))
		{
			if (Monitor.IsVerbose)
			{
				Monitor.Log($"Patch \"{patch.Path}\" loaded {assetName}.", (LogLevel)0);
			}
			try
			{
				T asset = default;
				if (patch.Migrator.TryApplyLoadPatch(patch, assetName, ref asset, out string error))
				{
					patch.IsApplied = true;
					return asset;
				}
				if (error != null)
				{
					Monitor.Log($"Can't apply patch {patch.Path} to {assetName}: {error}.", (LogLevel)4);
					return default;
				}
				T val = patch.Load<T>(assetName);
				IAssetValidator[] assetValidators = AssetValidators;
				foreach (IAssetValidator assetValidator in assetValidators)
				{
					if (!assetValidator.TryValidate(assetName, val, patch, out string error2))
					{
						Monitor.Log($"Can't apply patch {patch.Path} to {assetName}: {error2}.", (LogLevel)4);
						return default;
					}
				}
				patch.IsApplied = true;
				return val;
			}
			catch (Exception value)
			{
				Monitor.Log($"Can't apply patch {patch.Path} to {assetName}:\n{value}.", (LogLevel)4);
				return default;
			}
		}
	}

	private void ApplyEdits<T>(ICollection<IPatch> patches, IAssetData asset) where T : notnull
	{
		foreach (IPatch patch in patches)
		{
			using (Profiler?.RecordSection(patch.ContentPack.Manifest.UniqueID, "ApplyEdit", patch.Path.ToString()))
			{
				if (Monitor.IsVerbose)
				{
					Monitor.Log($"Applied patch \"{patch.Path}\" to {((IAssetInfo)asset).Name}.", (LogLevel)0);
				}
				try
				{
					if (patch.Migrator.TryApplyEditPatch<T>(patch, asset, out string error))
					{
						patch.IsApplied = true;
					}
					else if (error != null)
					{
						Monitor.Log($"Can't apply patch {patch.Path} to {((IAssetInfo)asset).Name}: {error}.", (LogLevel)4);
					}
					else
					{
						patch.Edit<T>(asset);
						patch.IsApplied = true;
					}
				}
				catch (Exception value)
				{
					Monitor.Log($"Unhandled exception applying patch: {patch.Path}.\n{value}", (LogLevel)4);
					patch.IsApplied = false;
				}
			}
		}
	}

	private PatchType? GetEditType(Type assetType)
	{
		if (typeof(Texture2D).IsAssignableFrom(assetType))
		{
			return PatchType.EditImage;
		}
		if (typeof(Map).IsAssignableFrom(assetType))
		{
			return PatchType.EditMap;
		}
		return PatchType.EditData;
	}

	private HashSet<IPatch> GetPatchesToUpdate(IInvariantSet globalChangedTokens, ContextUpdateType updateType)
	{
		HashSet<IPatch> hashSet = new HashSet<IPatch>(new ObjectReferenceComparer<IPatch>());
		foreach (string globalChangedToken in globalChangedTokens)
		{
			if (!PatchesAffectedByToken.TryGetValue(globalChangedToken, out HashSet<IPatch> value))
			{
				continue;
			}
			foreach (IPatch item in value)
			{
				if (updateType == ContextUpdateType.All || item.UpdateRate.HasFlag((UpdateRate)updateType))
				{
					hashSet.Add(item);
				}
			}
		}
		CollectionExtensions.AddRange<IPatch>((ISet<IPatch>)hashSet, (IEnumerable<IPatch>)PendingPatches);
		return hashSet;
	}

	private List<List<IPatch>> SortAndGroupEditPatches(IEnumerable<IPatch> patches)
	{
		List<List<IPatch>> list = new List<List<IPatch>>();
		string text = null;
		List<IPatch> list2 = new List<IPatch>();
		foreach (IPatch item in patches.OrderBy((IPatch p) => p.Priority))
		{
			string uniqueID = item.ContentPack.Manifest.UniqueID;
			if (uniqueID != text)
			{
				text = uniqueID;
				if (list2.Count > 0)
				{
					list.Add(list2);
					list2 = new List<IPatch>();
				}
			}
			list2.Add(item);
		}
		if (list2.Any())
		{
			list.Add(list2);
		}
		return list;
	}

	private void IndexPatch(IPatch patch, ModTokenContext modContext)
	{
		bool flag = false;
		if (!IndexedPatchValues.TryGetValue(patch, out IndexedPatchValues value))
		{
			flag = true;
			value = (IndexedPatchValues[patch] = new IndexedPatchValues());
		}
		IAssetName targetAsset = patch.TargetAsset;
		IAssetName target = value.Target;
		if (flag || ((target != null) ? (!target.IsEquivalentTo(targetAsset, false)) : (targetAsset != null)))
		{
			if (Monitor.IsVerbose)
			{
				Monitor.Log($"[patch index: by target] {patch.Path}: {((target != null) ? $"{target} > " : "")}{patch.TargetAsset}", (LogLevel)0);
			}
			if (!flag && target != null)
			{
				PatchesByCurrentTarget[target].Remove(patch);
			}
			if (targetAsset != null)
			{
				GetPatchesByTarget(targetAsset).Add(patch);
			}
		}
		value.Target = targetAsset;
		IInvariantSet tokensUsed = patch.GetTokensUsed();
		IInvariantSet rawTokens = value.RawTokens;
		if (!flag && tokensUsed.SetEquals(rawTokens))
		{
			return;
		}
		IInvariantSet invariantSet = ResolveTokensUsed(tokensUsed, modContext);
		if (Monitor.IsVerbose)
		{
			Monitor.Log($"[patch index: by tokens] {patch.Path}: ({string.Join(", ", value.ResolvedTokens)}) > ({string.Join(", ", invariantSet)})", (LogLevel)0);
		}
		if (!flag)
		{
			foreach (string resolvedToken in value.ResolvedTokens)
			{
				if (!invariantSet.Contains(resolvedToken))
				{
					PatchesAffectedByToken[resolvedToken].Remove(patch);
				}
			}
		}
		foreach (string item in invariantSet)
		{
			GetPatchesAffectedByToken(item).Add(patch);
		}
		value.RawTokens = tokensUsed;
		value.ResolvedTokens = invariantSet;
	}

	private void DeIndexPatch(IPatch patch)
	{
		if (!IndexedPatchValues.Remove(patch, out IndexedPatchValues value))
		{
			return;
		}
		if (value.Target != null)
		{
			PatchesByCurrentTarget[value.Target].Remove(patch);
		}
		foreach (string resolvedToken in value.ResolvedTokens)
		{
			PatchesAffectedByToken[resolvedToken].Remove(patch);
		}
	}

	private IInvariantSet ResolveTokensUsed(IInvariantSet rawTokens, ModTokenContext modContext)
	{
		MutableInvariantSet mutableInvariantSet = new MutableInvariantSet();
		foreach (string rawToken in rawTokens)
		{
			string text = modContext.ResolveAlias(rawToken);
			mutableInvariantSet.Add(text);
			foreach (string item in modContext.GetTokensWhichAffect(text))
			{
				mutableInvariantSet.Add(item);
			}
		}
		return mutableInvariantSet.GetImmutable();
	}

	private SortedSet<IPatch> GetPatchesByTarget(IAssetName assetName)
	{
		if (PatchesByCurrentTarget.TryGetValue(assetName, out SortedSet<IPatch> value))
		{
			return value;
		}
		value = new SortedSet<IPatch>(PatchIndexComparer.Instance);
		PatchesByCurrentTarget[assetName] = value;
		return value;
	}

	private HashSet<IPatch> GetPatchesAffectedByToken(string tokenName)
	{
		if (PatchesAffectedByToken.TryGetValue(tokenName, out HashSet<IPatch> value))
		{
			return value;
		}
		value = new HashSet<IPatch>();
		PatchesAffectedByToken[tokenName] = value;
		return value;
	}
}
