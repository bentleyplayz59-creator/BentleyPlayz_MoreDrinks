using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.ConfigModels;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal class Migration_2_7 : BaseMigration
{
	public Migration_2_7()
		: base((ISemanticVersion)new SemanticVersion(2, 7, 0, (string)null, (string)null))
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected Obj, but got Unknown
	}

	public override bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
	{
		if (!base.TryMigrate(ref patches, out error))
		{
			return false;
		}
		PatchConfig[] array = patches;
		foreach (PatchConfig patchConfig in array)
		{
			if (patchConfig.Entries.Count <= 0 || !string.Equals(patchConfig.Target, "Data/Characters", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			foreach (JToken value in patchConfig.Entries.Values)
			{
				JObject val = (JObject)(object)((value is JObject) ? value : null);
				if (val == null)
				{
					continue;
				}
				JProperty val2 = val.Property("EndSlideShow", StringComparison.OrdinalIgnoreCase);
				if (val2 != null)
				{
					string a = Extensions.Value<string>((IEnumerable<JToken>)val2.Value);
					if (string.Equals(a, bool.TrueString, StringComparison.OrdinalIgnoreCase))
					{
						val2.Value = JToken.op_Implicit("MainGroup");
					}
					else if (string.Equals(a, bool.FalseString, StringComparison.OrdinalIgnoreCase))
					{
						val2.Value = JToken.op_Implicit("Hidden");
					}
				}
			}
		}
		return true;
	}
}
