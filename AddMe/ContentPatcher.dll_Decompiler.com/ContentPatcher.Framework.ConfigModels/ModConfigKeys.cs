using System;
using System.Runtime.Serialization;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.ConfigModels;

internal class ModConfigKeys
{
	public KeybindList ToggleDebug { get; set; } = new KeybindList((SButton)114);

	public KeybindList DebugPrevTexture { get; set; } = new KeybindList((SButton)162);

	public KeybindList DebugNextTexture { get; set; } = new KeybindList((SButton)163);

	[OnDeserialized]
	public void OnDeserialized(StreamingContext context)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected Obj, but got Unknown
		//IL_001b: Expected Obj, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected Obj, but got Unknown
		//IL_0036: Expected Obj, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected Obj, but got Unknown
		//IL_0051: Expected Obj, but got Unknown
		if (ToggleDebug == null)
		{
			KeybindList val = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val;
			ToggleDebug = val;
		}
		if (DebugPrevTexture == null)
		{
			KeybindList val3 = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val3;
			DebugPrevTexture = val3;
		}
		if (DebugNextTexture == null)
		{
			KeybindList val4 = new KeybindList(Array.Empty<Keybind>());
			KeybindList val2 = val4;
			DebugNextTexture = val4;
		}
	}

	public ModConfigKeys()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected Obj, but got Unknown
	}
}
