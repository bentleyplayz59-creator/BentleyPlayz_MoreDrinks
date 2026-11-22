using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands;

internal class PatchInfo : PatchBaseInfo
{
	public LogPathBuilder Path { get; }

	public LogPathBuilder PathWithoutContentPackPrefix { get; }

	public string? RawType { get; }

	public PatchType? ParsedType { get; }

	public string? RawFromAsset { get; set; }

	public ITokenString? ParsedFromAsset { get; set; }

	public string? RawTargetAsset { get; }

	public ITokenString? ParsedTargetAsset { get; }

	public string? TargetAssetLocale { get; }

	public int? Priority { get; }

	public IContentPack ContentPack { get; }

	public bool IsLoaded { get; }

	public bool IsApplied { get; }

	public IPatch? Patch { get; }

	public PatchInfo(DisabledPatch patch)
		: this(patch.Path, patch.RawType, patch.ParsedType, null, Array.Empty<Condition>(), state: new ContextualState().AddError(patch.ReasonDisabled), contentPack: patch.ContentPack, matchesContext: false)
	{
		RawTargetAsset = patch.AssetName;
		TargetAssetLocale = patch.AssetNameLocale;
	}

	public PatchInfo(IPatch patch)
		: this(patch.Path, patch.Type.ToString(), patch.Type, patch.Priority, patch.Conditions, matchesContext: patch.IsReady, state: patch.GetDiagnosticState(), contentPack: patch.ContentPack)
	{
		ParsedType = patch.Type;
		RawFromAsset = patch.RawFromAsset?.Raw;
		ParsedFromAsset = patch.RawTargetAsset;
		RawTargetAsset = patch.RawTargetAsset?.Raw;
		ParsedTargetAsset = patch.RawTargetAsset;
		TargetAssetLocale = patch.TargetLocale;
		IsLoaded = true;
		IsApplied = patch.IsApplied;
		Patch = patch;
	}

	public IEnumerable<string> GetChangeLabels()
	{
		return Patch?.GetChangeLabels() ?? Array.Empty<string>();
	}

	public override string? GetReasonNotLoaded()
	{
		return (!IsApplied) ? base.GetReasonNotLoaded() : null;
	}

	public string? GetDisplayTarget()
	{
		string text = ParsedTargetAsset?.Value;
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		if (!string.IsNullOrWhiteSpace(TargetAssetLocale))
		{
			text = text + "." + TargetAssetLocale;
		}
		return text;
	}

	private PatchInfo(LogPathBuilder path, string? rawType, PatchType? parsedType, int? priority, Condition[] conditions, IContentPack contentPack, bool matchesContext, IContextualState state)
		: base(conditions, matchesContext, state)
	{
		Path = path;
		RawType = rawType;
		ParsedType = parsedType;
		Priority = priority;
		ContentPack = contentPack;
		PathWithoutContentPackPrefix = new LogPathBuilder(path.Segments.Skip(1));
	}
}
