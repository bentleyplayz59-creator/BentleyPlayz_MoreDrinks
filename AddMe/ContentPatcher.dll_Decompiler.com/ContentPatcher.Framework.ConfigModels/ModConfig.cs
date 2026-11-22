using System.Runtime.Serialization;

namespace ContentPatcher.Framework.ConfigModels;

internal class ModConfig
{
	public bool EnableDebugFeatures { get; set; }

	public ModConfigKeys Controls { get; set; } = new ModConfigKeys();

	[OnDeserialized]
	public void OnDeserialized(StreamingContext context)
	{
		if (Controls == null)
		{
			ModConfigKeys modConfigKeys = (Controls = new ModConfigKeys());
		}
	}
}
