using System;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Commands.Commands;

internal class ReloadCommand : BaseCommand
{
	private readonly Func<PatchLoader> GetPatchLoader;

	private readonly Func<PatchManager> GetPatchManager;

	private readonly LoadedContentPack[] ContentPacks;

	private readonly Action UpdateContext;

	public ReloadCommand(IMonitor monitor, Func<PatchLoader> getPatchLoader, Func<PatchManager> getPatchManager, LoadedContentPack[] contentPacks, Action updateContext)
		: base(monitor, "reload")
	{
		GetPatchLoader = getPatchLoader;
		GetPatchManager = getPatchManager;
		ContentPacks = contentPacks;
		UpdateContext = updateContext;
	}

	public override string GetDescription()
	{
		return $"patch reload\n   Usage: patch reload \"<content pack ID>\"\n   Reloads every patch for the content pack with this mod ID. Non-patch content (like config schema and dynamic tokens) aren't reloaded.\n\n   Usage: patch reload \"<content pack ID>\" \"<include file>\"\n   Reloads patches for the content pack with this mod ID, but only those loaded through an {PatchType.Include} patch with this relative path. Non-patch content (like config schema and dynamic tokens) aren't reloaded.";
	}

	public override void Handle(string[] args)
	{
		PatchLoader patchLoader = GetPatchLoader();
		PatchManager patchManager = GetPatchManager();
		string packId = ArgUtility.Get(args, 0, (string)null, false);
		string includePath = ArgUtility.Get(args, 1, (string)null, false);
		if (packId == null)
		{
			Monitor.Log("The 'patch reload' command expects the first argument to be the target content pack's mod ID. See 'patch help' for more info.", (LogLevel)4);
			return;
		}
		RawContentPack rawContentPack = ContentPacks.SingleOrDefault((LoadedContentPack p) => p.Manifest.UniqueID == packId);
		if (rawContentPack == null)
		{
			Monitor.Log("No Content Patcher content pack with the unique ID \"" + packId + "\".", (LogLevel)4);
			return;
		}
		IncludePatch includePatch = null;
		if (includePath != null)
		{
			includePath = PathUtilities.NormalizePath(includePath);
			includePatch = patchManager.GetPatches().FirstOrDefault((IPatch p) => p.FromAsset == includePath) as IncludePatch;
			if (includePatch == null)
			{
				Monitor.Log($"There's no {PatchType.Include} patch which reads from the path \"{includePath}\".", (LogLevel)4);
				return;
			}
			if (!includePatch.IsReady || !includePatch.Conditions.All((Condition p) => p.IsMatch))
			{
				Monitor.Log("The specified patch isn't currently enabled.", (LogLevel)4);
				return;
			}
		}
		if (includePatch != null)
		{
			patchLoader.UnloadPatchesLoadedBy(includePatch);
			includePatch.AttemptLoad();
		}
		else
		{
			patchLoader.UnloadPatchesLoadedBy(rawContentPack);
			if (!rawContentPack.TryReloadContent(out string error))
			{
				Monitor.Log($"Failed to reload content pack '{rawContentPack.Manifest.Name}' for configuration changes: {error}. The content pack may not be in a valid state.", (LogLevel)4);
				return;
			}
			patchLoader.LoadPatches(rawContentPack, rawContentPack.Content.Changes, null, new int[1] { rawContentPack.Index }, new LogPathBuilder(rawContentPack.Manifest.Name), null);
		}
		UpdateContext();
		Monitor.Log("Content pack reloaded.", (LogLevel)2);
	}
}
