using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels;

internal class PatchRectangleConfig
{
	public string? X { get; }

	public string? Y { get; }

	public string? Width { get; }

	public string? Height { get; }

	[JsonConstructor]
	public PatchRectangleConfig(string? x, string? y, string? width, string? height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public PatchRectangleConfig(PatchRectangleConfig other)
		: this(other.X, other.Y, other.Width, other.Height)
	{
	}
}
