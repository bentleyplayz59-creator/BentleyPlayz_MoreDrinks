using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile;
using xTile.Layers;

namespace Pathoschild.Stardew.Common;

internal static class TileHelper
{
	public static IEnumerable<Vector2> GetTiles(this GameLocation? location)
	{
		object obj;
		if (location == null)
		{
			obj = null;
		}
		else
		{
			Map map = location.Map;
			obj = ((map != null) ? map.Layers : null);
		}
		if (obj == null)
		{
			return Array.Empty<Vector2>();
		}
		Layer val = location.Map.Layers[0];
		return GetTiles(0, 0, val.LayerWidth, val.LayerHeight);
	}

	public static IEnumerable<Vector2> GetTiles(this Rectangle area)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return GetTiles(area.X, area.Y, area.Width, area.Height);
	}

	public static Rectangle Expand(this Rectangle area, int distance)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return new Rectangle(area.X - distance, area.Y - distance, area.Width + distance * 2, area.Height + distance * 2);
	}

	public static IEnumerable<Vector2> GetSurroundingTiles(this Vector2 tile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Utility.getSurroundingTileLocationsArray(tile);
	}

	public static IEnumerable<Vector2> GetSurroundingTiles(this Rectangle area)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		for (int x = area.X - 1; x <= area.X + area.Width; x++)
		{
			for (int y = area.Y - 1; y <= area.Y + area.Height; y++)
			{
				if (!area.Contains(x, y))
				{
					yield return new Vector2((float)x, (float)y);
				}
			}
		}
	}

	public static IEnumerable<Vector2> GetAdjacentTiles(this Vector2 tile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Utility.getAdjacentTileLocationsArray(tile);
	}

	public static IEnumerable<Vector2> GetTiles(int x, int y, int width, int height)
	{
		int curX = x;
		for (int maxX = x + width - 1; curX <= maxX; curX++)
		{
			int curY = y;
			for (int maxY = y + height - 1; curY <= maxY; curY++)
			{
				yield return new Vector2((float)curX, (float)curY);
			}
		}
	}

	public static IEnumerable<Vector2> GetVisibleTiles(int expand = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetVisibleArea(expand).GetTiles();
	}

	public static Rectangle GetVisibleArea(int expand = 0)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		return new Rectangle(Game1.viewport.X / 64 - expand, Game1.viewport.Y / 64 - expand, (int)Math.Ceiling((decimal)Game1.viewport.Width / 64m) + expand * 2, (int)Math.Ceiling((decimal)Game1.viewport.Height / 64m) + expand * 2);
	}

	public static Vector2 GetTileFromCursor()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return GetTileFromScreenPosition(Game1.getMouseXRaw(), Game1.getMouseYRaw());
	}

	public static Vector2 GetTileFromScreenPosition(float x, float y)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Game1.viewport.X + x / Game1.options.zoomLevel;
		float num2 = (float)Game1.viewport.Y + y / Game1.options.zoomLevel;
		int num3 = (int)Math.Floor(num / 64f);
		int num4 = (int)Math.Floor(num2 / 64f);
		return new Vector2((float)num3, (float)num4);
	}
}
