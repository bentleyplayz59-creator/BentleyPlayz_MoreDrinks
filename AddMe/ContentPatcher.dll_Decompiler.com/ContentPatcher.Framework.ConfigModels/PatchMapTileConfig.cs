using Newtonsoft.Json;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels;

internal class PatchMapTileConfig
{
	public string? Layer { get; }

	public PatchPositionConfig? Position { get; }

	public string? SetTilesheet { get; }

	public string? SetIndex { get; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public InvariantDictionary<string?> SetProperties { get; }

	public string? Remove { get; }

	[JsonConstructor]
	public PatchMapTileConfig(string? layer, PatchPositionConfig? position, string? setTilesheet, string? setIndex, InvariantDictionary<string?>? setProperties, string? remove)
	{
		Layer = layer;
		Position = position;
		SetTilesheet = setTilesheet;
		SetIndex = setIndex;
		SetProperties = setProperties ?? new InvariantDictionary<string>();
		Remove = remove;
	}

	public PatchMapTileConfig(PatchMapTileConfig other)
		: this(other.Layer, other.Position, other.SetTilesheet, other.SetIndex, other.SetProperties, other.Remove)
	{
	}
}
