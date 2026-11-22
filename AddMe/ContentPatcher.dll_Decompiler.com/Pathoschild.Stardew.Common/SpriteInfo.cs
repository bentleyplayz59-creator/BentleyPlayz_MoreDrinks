using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.Stardew.Common;

internal class SpriteInfo
{
	public Texture2D Spritesheet { get; }

	public Rectangle SourceRectangle
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
	}

	public SpriteInfo(Texture2D spritesheet, Rectangle sourceRectangle)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Spritesheet = spritesheet;
		SourceRectangle = sourceRectangle;
	}

	public virtual void Draw(SpriteBatch spriteBatch, int x, int y, Vector2 size, Nullable color = null)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		spriteBatch.DrawSpriteWithin(Spritesheet, SourceRectangle, x, y, size, color ?? Color.White);
	}
}
