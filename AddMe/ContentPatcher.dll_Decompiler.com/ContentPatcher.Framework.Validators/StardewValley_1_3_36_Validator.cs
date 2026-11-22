using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;
using xTile;
using xTile.Tiles;

namespace ContentPatcher.Framework.Validators;

internal class StardewValley_1_3_36_Validator : BaseValidator
{
	private readonly Dictionary<string, string> ObsoleteTilesheets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["mine"] = "Mines/mine",
		["mine_dark"] = "Mines/mine_dark",
		["mine_lava"] = "Mines/mine_lava"
	};

	public override bool TryValidate<T>(IAssetName assetName, T data, IPatch patch, [NotNullWhen(false)] out string? error)
	{
		Map val = (Map)((((object)data) is Map) ? ((object)data) : null);
		if (val != null)
		{
			string directoryName = Path.GetDirectoryName(patch.FromAsset);
			foreach (TileSheet tileSheet in val.TileSheets)
			{
				string imageSource = tileSheet.ImageSource;
				string text = ((directoryName != null) ? Path.Combine(directoryName, imageSource) : imageSource);
				if (patch.ContentPack.HasFile(text) || !IsObsoleteTilesheet(imageSource, out string newKey))
				{
					continue;
				}
				error = $"references vanilla tilesheet '{imageSource}' removed in Stardew Valley 1.3.36, should use '{newKey}' instead";
				return false;
			}
		}
		error = null;
		return true;
	}

	private bool IsObsoleteTilesheet(string? curKey, [NotNullWhen(true)] out string? newKey)
	{
		if (curKey == null)
		{
			newKey = null;
			return false;
		}
		if (ObsoleteTilesheets.TryGetValue(curKey, out newKey))
		{
			return true;
		}
		if (curKey.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && ObsoleteTilesheets.TryGetValue(curKey.Substring(0, curKey.Length - 4), out newKey))
		{
			return true;
		}
		return false;
	}
}
