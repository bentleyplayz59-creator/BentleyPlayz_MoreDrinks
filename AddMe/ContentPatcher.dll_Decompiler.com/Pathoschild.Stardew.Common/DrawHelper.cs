using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.Common;

internal static class DrawHelper
{
	public static float GetSpaceWidth(SpriteFont font)
	{
		return CommonHelper.GetSpaceWidth(font);
	}

	public static void DrawSprite(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Vector2 errorSize, Nullable color = null, float scale = 1f)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			spriteBatch.Draw(sheet, new Vector2(x, y), (Nullable)sprite, color ?? Color.White, 0f, Vector2.Zero, scale, (SpriteEffects)0, 0f);
		}
		catch
		{
			Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, (int)errorSize.X, (int)errorSize.Y), 0f);
		}
	}

	public static void DrawSprite(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Point errorSize, Nullable color = null, float scale = 1f)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			spriteBatch.Draw(sheet, new Vector2(x, y), (Nullable)sprite, color ?? Color.White, 0f, Vector2.Zero, scale, (SpriteEffects)0, 0f);
		}
		catch
		{
			Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, errorSize.X, errorSize.Y), 0f);
		}
	}

	public static void DrawSpriteWithin(this SpriteBatch spriteBatch, SpriteInfo? sprite, float x, float y, Vector2 size, Nullable color = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			sprite?.Draw(spriteBatch, (int)x, (int)y, size, color);
		}
		catch
		{
			Utility.DrawErrorTexture(spriteBatch, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), 0f);
		}
	}

	public static void DrawSpriteWithin(this SpriteBatch spriteBatch, Texture2D sheet, Rectangle sprite, float x, float y, Vector2 size, Nullable color = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Max(sprite.Width, sprite.Height);
		float num2 = size.X / num;
		float num3 = Math.Max((size.X - (float)sprite.Width * num2) / 2f, 0f);
		float num4 = Math.Max((size.Y - (float)sprite.Height * num2) / 2f, 0f);
		spriteBatch.DrawSprite(sheet, sprite, x + num3, y + num4, size, color ?? Color.White, num2);
	}

	public static void DrawLine(this SpriteBatch batch, float x, float y, Vector2 size, Nullable color = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		batch.Draw(CommonHelper.Pixel, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y), color ?? Color.White);
	}

	public static Texture2D RenderToTexture(Action<SpriteBatch> draw, int pixelWidth, int pixelHeight, SpriteBatch? useSpriteBatch = null)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected Obj, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected Obj, but got Unknown
		//IL_006f: Expected Obj, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		RenderTargetBinding[] renderTargets = Game1.graphics.GraphicsDevice.GetRenderTargets();
		object obj;
		if (renderTargets.Length == 0)
		{
			obj = null;
		}
		else
		{
			Texture renderTarget = renderTargets[0].RenderTarget;
			obj = ((renderTarget is RenderTarget2D) ? renderTarget : null);
		}
		RenderTarget2D renderTarget2 = (RenderTarget2D)obj;
		SpriteBatch val = null;
		RenderTarget2D val2 = new RenderTarget2D(Game1.graphics.GraphicsDevice, pixelWidth, pixelHeight, false, (SurfaceFormat)0, (DepthFormat)0, 0, (RenderTargetUsage)0);
		Game1.SetRenderTarget(val2);
		try
		{
			SpriteBatch val3;
			if (useSpriteBatch != null)
			{
				val3 = useSpriteBatch;
			}
			else
			{
				SpriteBatch val4 = new SpriteBatch(Game1.graphics.GraphicsDevice);
				val = val4;
				val3 = val4;
			}
			val3.Begin((SpriteSortMode)1, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null, (Effect)null, (Nullable)null);
			Game1.graphics.GraphicsDevice.Clear(Color.Transparent);
			draw(val3);
			val3.End();
			return (Texture2D)(object)val2;
		}
		finally
		{
			Game1.SetRenderTarget(renderTarget2);
			if (val != null)
			{
				((GraphicsResource)val).Dispose();
			}
		}
	}
}
