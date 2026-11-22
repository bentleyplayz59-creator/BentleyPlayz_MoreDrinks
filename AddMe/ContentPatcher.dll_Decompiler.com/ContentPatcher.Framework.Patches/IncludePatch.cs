using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Patches;

internal class IncludePatch : Patch
{
	private readonly IMonitor Monitor;

	private readonly RawContentPack RawContentPack;

	private readonly PatchLoader PatchLoader;

	private bool AttemptedDataLoad;

	public IEnumerable<IPatch>? PatchesJustLoaded { get; private set; }

	public IncludePatch(int[] indexPath, LogPathBuilder path, IEnumerable<Condition> conditions, IManagedTokenString fromFile, UpdateRate updateRate, InvariantDictionary<IManagedTokenString>? inheritedLocalTokens, InvariantDictionary<IManagedTokenString>? localTokens, RawContentPack contentPack, IPatch? parentPatch, Func<string, IAssetName> parseAssetName, IMonitor monitor, PatchLoader patchLoader)
		: base(indexPath, path, PatchType.Include, null, null, 0, updateRate, inheritedLocalTokens, localTokens, conditions, contentPack.ContentPack, contentPack.Migrator, parentPatch, parseAssetName, fromFile)
	{
		RawContentPack = contentPack;
		Monitor = monitor;
		PatchLoader = patchLoader;
	}

	public override bool UpdateContext(IContext context)
	{
		PatchesJustLoaded = null;
		if (!UpdateContext(context, out string previousFilePath))
		{
			return false;
		}
		if (!string.IsNullOrWhiteSpace(previousFilePath))
		{
			PatchLoader.UnloadPatchesLoadedBy(this);
		}
		AttemptLoad();
		return MarkUpdated();
	}

	public void AttemptLoad()
	{
		AttemptedDataLoad = IsReady && Conditions.All((Condition p) => p.IsMatch);
		IsApplied = false;
		if (!AttemptedDataLoad)
		{
			return;
		}
		try
		{
			if (!FromAssetExists())
			{
				WarnForPatch("file '" + FromAsset + "' doesn't exist.", (LogLevel)3);
				return;
			}
			List<string> list = new List<string>();
			for (IPatch parentPatch = ParentPatch; parentPatch != null; parentPatch = parentPatch.ParentPatch)
			{
				if (parentPatch.Type == PatchType.Include)
				{
					list.Add(parentPatch.FromAsset);
					if (IsSameFilePath(parentPatch.FromAsset, FromAsset))
					{
						list.Reverse();
						list.Add(FromAsset);
						WarnForPatch("patch skipped because it would cause an infinite loop (" + string.Join(" > ", list) + ").", (LogLevel)3);
						return;
					}
				}
			}
			ContentConfig contentConfig = ContentPack.ModContent.Load<ContentConfig>(FromAsset);
			if (!contentConfig.Changes.Any())
			{
				WarnForPatch($"file '{FromAsset}' doesn't have anything in the {"Changes"} field. Is the file formatted correctly?", (LogLevel)3);
			}
			else
			{
				string[] source = GetInvalidFields(contentConfig).ToArray();
				if (source.Any())
				{
					WarnForPatch("file contains fields which aren't allowed for a secondary file (" + string.Join(", ", source.OrderByHuman()) + ").", (LogLevel)3);
					return;
				}
				PatchesJustLoaded = PatchLoader.LoadPatches(RawContentPack, contentConfig.Changes, LocalTokensIncludingInherited, IndexPath, GetIncludedLogPath(FromAsset), this);
				IsApplied = true;
			}
		}
		catch (Exception value)
		{
			WarnForPatch($"an error occurred.\n{value}", (LogLevel)4);
		}
	}

	public override IEnumerable<string> GetChangeLabels()
	{
		return Array.Empty<string>();
	}

	private bool UpdateContext(IContext context, out string? previousFilePath)
	{
		previousFilePath = ((AttemptedDataLoad && IsReady && FromAssetExists()) ? FromAsset : null);
		return base.UpdateContext(context);
	}

	private bool IsSameFilePath(string? left, string? right)
	{
		if (left == right)
		{
			return true;
		}
		if (left == null || right == null)
		{
			return false;
		}
		left = PathUtilities.NormalizeAssetName(left);
		right = PathUtilities.NormalizeAssetName(right);
		return StringExtensions.EqualsIgnoreCase(left, right);
	}

	private IEnumerable<string> GetInvalidFields(ContentConfig content)
	{
		PropertyInfo[] properties = typeof(ContentConfig).GetProperties();
		foreach (PropertyInfo property in properties)
		{
			if (!(property.Name == "Changes"))
			{
				object value = property.GetValue(content);
				if ((value is IEnumerable list) ? list.Cast<object>().Any() : (value != null))
				{
					yield return property.Name;
				}
			}
		}
	}

	private LogPathBuilder GetIncludedLogPath(string filePath)
	{
		filePath = PathUtilities.NormalizeAssetName(filePath);
		string text = PathUtilities.NormalizeAssetName(Path.Segments.Last());
		return StringExtensions.ContainsIgnoreCase(text, filePath) ? Path : Path.With(filePath);
	}

	private void WarnForPatch(string message, LogLevel level = (LogLevel)3)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Monitor.Log($"Error loading patch '{Path}': {message}.", level);
	}
}
