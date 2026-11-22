using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels;

internal class PatchPositionConfig
{
	public string? X { get; }

	public string? Y { get; }

	[JsonConstructor]
	public PatchPositionConfig(string? x, string? y)
	{
		X = x;
		Y = y;
	}

	public PatchPositionConfig(PatchRectangleConfig other)
		: this(other.X, other.Y)
	{
	}
}
