using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common;

internal static class CommonHelper
{
	private static readonly Lazy<Texture2D> LazyPixel = new Lazy<Texture2D>(() =>
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected Obj, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
		val.SetData<Color>(new Color[1] { Color.White });
		return val;
	});

	public const int ButtonBorderWidth = 16;

	public static readonly Vector2 ScrollEdgeSize = new Vector2((float)(CommonSprites.Scroll.TopLeft.Width * 4), (float)(CommonSprites.Scroll.TopLeft.Height * 4));

	public static Texture2D Pixel => LazyPixel.Value;

	public static IEnumerable<TValue> GetEnumValues<TValue>() where TValue : struct
	{
		return Enum.GetValues(typeof(TValue)).Cast<TValue>();
	}

	public static IEnumerable<GameLocation> GetLocations(bool includeTempLevels = false)
	{
		IEnumerable<GameLocation> enumerable = Game1.locations.Concat(from location in Game1.locations
			from indoors in location.GetInstancedBuildingInteriors()
			select indoors);
		if (includeTempLevels)
		{
			enumerable = enumerable.Concat((IEnumerable<GameLocation>)MineShaft.activeMines).Concat((IEnumerable<GameLocation>)VolcanoDungeon.activeLevels);
		}
		return enumerable;
	}

	public static Vector2 GetPlayerTile(Farmer? player)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((player != null) ? ((Character)player).Position : Vector2.Zero);
		return new Vector2((float)(int)(val.X / 64f), (float)(int)(val.Y / 64f));
	}

	public static bool IsItemId(string itemId, bool allowZero = true)
	{
		int result;
		return !string.IsNullOrWhiteSpace(itemId) && (!int.TryParse(itemId, out result) || result >= ((!allowZero) ? 1 : 0));
	}

	public static string FormatTime(int time)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		string text = Game1.getTimeOfDayString(time);
		LanguageCode currentLanguageCode = LocalizedContentManager.CurrentLanguageCode;
		if (currentLanguageCode - 9 <= 1)
		{
			LocalizedContentManager content = Game1.content;
			bool flag = ((time < 1200 || time >= 2400) ? true : false);
			string text2 = content.LoadString(flag ? "Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370" : "Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371");
			text += (((int)LocalizedContentManager.CurrentLanguageCode == 9) ? text2 : (" " + text2));
		}
		return text;
	}

	public static float GetSpaceWidth(SpriteFont font)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return font.MeasureString("A B").X - font.MeasureString("AB").X;
	}

	public static string GetFormattedPercentageNumber(float chance)
	{
		float num = chance * 100f;
		float num2 = num;
		float num3 = num2;
		if (!(num3 <= 0f))
		{
			if (!(num3 >= 100f))
			{
				if (!(num3 < 1f))
				{
					if (num3 < 1.95f)
					{
						return num.ToString("#.#");
					}
					return num.ToString("#");
				}
				for (int i = 3; i < 28; i++)
				{
					decimal num4 = Math.Round((decimal)num, i);
					if (num4 > 0m)
					{
						return num4.ToString("0.".PadRight(i + 2, '#'));
					}
				}
				return num.ToString(CultureInfo.InvariantCulture);
			}
			return "100";
		}
		return "0";
	}

	public static void Draw(this SpriteBatch batch, Texture2D sheet, Rectangle sprite, int x, int y, int width, int height, Nullable color = null)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		batch.Draw(sheet, new Rectangle(x, y, width, height), (Nullable)sprite, color ?? Color.White);
	}

	public static Vector2 DrawHoverBox(SpriteBatch spriteBatch, string label, in Vector2 position, float wrapWidth)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = spriteBatch.DrawTextBlock(Game1.smallFont, label, position + new Vector2(20f), wrapWidth, (Nullable)null);
		IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)position.X, (int)position.Y, (int)val.X + 27 + 20, (int)val.Y + 27, Color.White, 1f, true, -1f);
		spriteBatch.DrawTextBlock(Game1.smallFont, label, position + new Vector2(20f), wrapWidth, (Nullable)null);
		return val + new Vector2(27f);
	}

	public static void DrawTab(SpriteBatch spriteBatch, int x, int y, int innerWidth, int innerHeight, out Vector2 innerDrawPosition, int align = 0, float alpha = 1f, bool forIcon = false, bool drawShadow = true)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		int num = innerWidth + 32;
		int num2 = innerHeight + 21;
		if (1 == 0)
		{
		}
		int num3 = align switch
		{
			1 => -num / 2, 
			2 => -num, 
			_ => 0, 
		};
		if (1 == 0)
		{
		}
		int num4 = num3;
		int num5 = (forIcon ? (-4) : 0);
		int num6 = (forIcon ? (-8) : 0);
		innerDrawPosition = new Vector2((float)(x + 16 + num4 + num5), (float)(y + 16 + num6));
		IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x + num4, y, num, num2 + 4, Color.White * alpha, 1f, drawShadow, -1f);
	}

	public static void DrawButton(SpriteBatch spriteBatch, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding = 0)
	{
		DrawContentBox(spriteBatch, CommonSprites.Button.Sheet, in CommonSprites.Button.Background, in CommonSprites.Button.Top, in CommonSprites.Button.Right, in CommonSprites.Button.Bottom, in CommonSprites.Button.Left, in CommonSprites.Button.TopLeft, in CommonSprites.Button.TopRight, in CommonSprites.Button.BottomRight, in CommonSprites.Button.BottomLeft, in position, in contentSize, out contentPos, out bounds, padding);
	}

	public static void DrawScroll(SpriteBatch spriteBatch, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding = 5, float alpha = 1f)
	{
		DrawContentBox(spriteBatch, CommonSprites.Scroll.Sheet, in CommonSprites.Scroll.Background, in CommonSprites.Scroll.Top, in CommonSprites.Scroll.Right, in CommonSprites.Scroll.Bottom, in CommonSprites.Scroll.Left, in CommonSprites.Scroll.TopLeft, in CommonSprites.Scroll.TopRight, in CommonSprites.Scroll.BottomRight, in CommonSprites.Scroll.BottomLeft, in position, in contentSize, out contentPos, out bounds, padding, alpha);
	}

	public static void DrawContentBox(SpriteBatch spriteBatch, Texture2D texture, in Rectangle background, in Rectangle top, in Rectangle right, in Rectangle bottom, in Rectangle left, in Rectangle topLeft, in Rectangle topRight, in Rectangle bottomRight, in Rectangle bottomLeft, in Vector2 position, in Vector2 contentSize, out Vector2 contentPos, out Rectangle bounds, int padding, float alpha = 1f)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		GetContentBoxDimensions(topLeft, contentSize, padding, out var innerWidth, out var innerHeight, out var outerWidth, out var outerHeight, out var borderWidth, out var borderHeight);
		int num = (int)position.X;
		int num2 = (int)position.Y;
		Color val = ((alpha < 1f) ? (Color.White * alpha) : Color.White);
		spriteBatch.Draw(texture, new Rectangle(num + borderWidth, num2 + borderHeight, innerWidth, innerHeight), (Nullable)background, val);
		spriteBatch.Draw(texture, new Rectangle(num + borderWidth, num2, innerWidth, borderHeight), (Nullable)top, val);
		spriteBatch.Draw(texture, new Rectangle(num + borderWidth, num2 + borderHeight + innerHeight, innerWidth, borderHeight), (Nullable)bottom, val);
		spriteBatch.Draw(texture, new Rectangle(num, num2 + borderHeight, borderWidth, innerHeight), (Nullable)left, val);
		spriteBatch.Draw(texture, new Rectangle(num + borderWidth + innerWidth, num2 + borderHeight, borderWidth, innerHeight), (Nullable)right, val);
		spriteBatch.Draw(texture, new Rectangle(num, num2, borderWidth, borderHeight), (Nullable)topLeft, val);
		spriteBatch.Draw(texture, new Rectangle(num, num2 + borderHeight + innerHeight, borderWidth, borderHeight), (Nullable)bottomLeft, val);
		spriteBatch.Draw(texture, new Rectangle(num + borderWidth + innerWidth, num2, borderWidth, borderHeight), (Nullable)topRight, val);
		spriteBatch.Draw(texture, new Rectangle(num + borderWidth + innerWidth, num2 + borderHeight + innerHeight, borderWidth, borderHeight), (Nullable)bottomRight, val);
		contentPos = new Vector2((float)(num + borderWidth + padding), (float)(num2 + borderHeight + padding));
		bounds = new Rectangle(num, num2, outerWidth, outerHeight);
	}

	public static void ShowInfoMessage(string message, int? duration = null, int number = -1)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected Obj, but got Unknown
		Game1.addHUDMessage(new HUDMessage(message, 3)
		{
			noIcon = true,
			timeLeft = (((float?)duration) ?? 3500f),
			number = number
		});
	}

	public static void ShowErrorMessage(string message)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		Game1.addHUDMessage(new HUDMessage(message, 3));
	}

	public static void GetScrollDimensions(Vector2 contentSize, int padding, out int innerWidth, out int innerHeight, out int labelOuterWidth, out int outerHeight, out int borderWidth, out int borderHeight)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		GetContentBoxDimensions(CommonSprites.Scroll.TopLeft, contentSize, padding, out innerWidth, out innerHeight, out labelOuterWidth, out outerHeight, out borderWidth, out borderHeight);
	}

	public static void GetContentBoxDimensions(Rectangle topLeft, Vector2 contentSize, int padding, out int innerWidth, out int innerHeight, out int outerWidth, out int outerHeight, out int borderWidth, out int borderHeight)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		borderWidth = topLeft.Width * 4;
		borderHeight = topLeft.Height * 4;
		innerWidth = (int)(contentSize.X + (float)(padding * 2));
		innerHeight = (int)(contentSize.Y + (float)(padding * 2));
		outerWidth = innerWidth + borderWidth * 2;
		outerHeight = innerHeight + borderHeight * 2;
	}

	public static void DrawLine(this SpriteBatch batch, float x, float y, in Vector2 size, in Nullable color = null)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		batch.Draw(Pixel, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), color ?? Color.White);
	}

	public static Vector2 DrawTextBlock(this SpriteBatch batch, SpriteFont font, string? text, in Vector2 position, float wrapWidth, in Nullable color = null, bool bold = false, float scale = 1f)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		if (text == null)
		{
			return new Vector2(0f, 0f);
		}
		List<string> list = new List<string>();
		string[] array = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		foreach (string text2 in array)
		{
			string text3 = text2;
			int num;
			while ((num = text3.IndexOf(Environment.NewLine, StringComparison.Ordinal)) >= 0)
			{
				if (num == 0)
				{
					list.Add(Environment.NewLine);
					text3 = text3.Substring(Environment.NewLine.Length);
				}
				else if (num > 0)
				{
					list.Add(text3.Substring(0, num));
					list.Add(Environment.NewLine);
					text3 = text3.Substring(num + Environment.NewLine.Length);
				}
			}
			if (text3.Length > 0)
			{
				list.Add(text3);
			}
		}
		float num2 = 0f;
		float num3 = 0f;
		float num4 = font.MeasureString("ABC").Y * scale;
		float num5 = GetSpaceWidth(font) * scale;
		float num6 = 0f;
		float num7 = num4;
		foreach (string item in list)
		{
			float num8 = font.MeasureString(item).X * scale;
			if (item == Environment.NewLine || (num8 + num2 > wrapWidth && (int)num2 != 0))
			{
				num2 = 0f;
				num3 += num4;
				num7 += num4;
			}
			if (!(item == Environment.NewLine))
			{
				Vector2 val = new Vector2(position.X + num2, position.Y + num3);
				if (bold)
				{
					Utility.drawBoldText(batch, item, font, val, color ?? Color.Black, scale, -1f, 1);
				}
				else
				{
					batch.DrawString(font, item, val, color ?? Color.Black, 0f, Vector2.Zero, scale, (SpriteEffects)0, 1f);
				}
				if (num2 + num8 > num6)
				{
					num6 = num2 + num8;
				}
				num2 += num8 + num5;
			}
		}
		return new Vector2(num6, num7);
	}

	public static void InterceptErrors(this IMonitor monitor, string verb, Action action, Action<Exception>? onError = null)
	{
		monitor.InterceptErrors(verb, null, action, onError);
	}

	public static void InterceptErrors(this IMonitor monitor, string verb, string? detailedVerb, Action action, Action<Exception>? onError = null)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			monitor.InterceptError(ex, verb, detailedVerb);
			onError?.Invoke(ex);
		}
	}

	public static void InterceptError(this IMonitor monitor, Exception ex, string verb, string? detailedVerb = null)
	{
		if (detailedVerb == null)
		{
			detailedVerb = verb;
		}
		monitor.Log($"Something went wrong {detailedVerb}:\n{ex}", (LogLevel)4);
		ShowErrorMessage("Huh. Something went wrong " + verb + ". The error log has the technical details.");
	}

	public static void RemoveObsoleteFiles(IMod mod, params string[] relativePaths)
	{
		string directoryPath = mod.Helper.DirectoryPath;
		foreach (string text in relativePaths)
		{
			string path = Path.Combine(directoryPath, text);
			if (File.Exists(path))
			{
				try
				{
					File.Delete(path);
					mod.Monitor.Log("Removed obsolete file '" + text + "'.", (LogLevel)0);
				}
				catch (Exception value)
				{
					mod.Monitor.Log($"Failed deleting obsolete file '{text}':\n{value}", (LogLevel)0);
				}
			}
		}
	}

	public static string GetFileHash(string absolutePath)
	{
		using FileStream inputStream = File.OpenRead(absolutePath);
		using MD5 mD = MD5.Create();
		byte[] array = mD.ComputeHash(inputStream);
		return BitConverter.ToString(array).Replace("-", "").ToLowerInvariant();
	}

	static CommonHelper()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
	}
}
