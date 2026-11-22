using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Patches;

internal class DisabledPatch
{
	public LogPathBuilder Path { get; }

	public string? RawType { get; }

	public PatchType? ParsedType { get; }

	public string? AssetName { get; }

	public string? AssetNameLocale { get; }

	public IContentPack ContentPack { get; }

	public string ReasonDisabled { get; }

	public Patch? ParentPatch { get; }

	public DisabledPatch(LogPathBuilder path, string? rawType, PatchType? parsedType, string? assetName, string? assetNameLocale, IContentPack contentPack, Patch? parentPatch, string reasonDisabled)
	{
		Path = path;
		RawType = rawType;
		ParsedType = parsedType;
		ContentPack = contentPack;
		AssetName = assetName;
		AssetNameLocale = assetNameLocale;
		ParentPatch = parentPatch;
		ReasonDisabled = reasonDisabled;
	}
}
