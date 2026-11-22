using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Locations;

internal class CustomLocationData
{
	public string Name { get; }

	public string FromMapFile { get; }

	public string[] MigrateLegacyNames { get; }

	public string PublicMapPath { get; }

	public IContentPack ContentPack { get; }

	[MemberNotNullWhen(false, "Error")]
	public bool IsEnabled
	{
		[MemberNotNullWhen(false, "Error")]
		get;
		[MemberNotNullWhen(false, "Error")]
		private set;
	}

	public string? Error { get; private set; }

	public bool HasLegacyNames { get; }

	public string ModId => ContentPack.Manifest.UniqueID;

	public string ModName => ContentPack.Manifest.Name;

	public CustomLocationData(string name, string fromMapFile, string[] migrateLegacyNames, IContentPack contentPack)
	{
		Name = name;
		FromMapFile = fromMapFile;
		MigrateLegacyNames = migrateLegacyNames;
		ContentPack = contentPack;
		PublicMapPath = PathUtilities.NormalizeAssetName("Maps/" + name);
		IsEnabled = true;
		HasLegacyNames = MigrateLegacyNames.Any();
	}

	public void Disable(string error)
	{
		IsEnabled = false;
		Error = error;
	}
}
