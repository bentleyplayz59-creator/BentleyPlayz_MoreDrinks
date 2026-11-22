using System;
using xTile;
using xTile.Dimensions;
using xTile.Layers;

namespace Pathoschild.Stardew.Common;

internal static class MapExtensions
{
	public static Size GetSizeInTiles(this Map map)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		int num = 1;
		int num2 = 1;
		foreach (Layer layer in map.Layers)
		{
			num = Math.Max(num, layer.LayerWidth);
			num2 = Math.Max(num2, layer.LayerHeight);
		}
		return new Size(num, num2);
	}
}
