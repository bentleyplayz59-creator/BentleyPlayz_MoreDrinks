using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ContentPatcher.Framework;

internal class DebugOverlay : BaseOverlay
{
	private readonly int Margin = 30;

	private readonly int Padding = 5;

	private readonly IGameContentHelper Content;

	private readonly IAssetName[] TextureNames;

	private IAssetName? CurrentName;

	private Texture2D? CurrentTexture;

	public DebugOverlay(IModEvents events, IInputHelper inputHelper, IGameContentHelper contentHelper, IReflectionHelper reflection)
		: base(events, inputHelper, reflection)
	{
		Content = contentHelper;
		TextureNames = GetTextureNames(contentHelper).OrderByHuman((IAssetName p) => p.Name).ToArray();
		NextTexture();
	}

	public void NextTexture()
	{
		int num = Array.IndexOf(TextureNames, CurrentName) + 1;
		if (num >= TextureNames.Length)
		{
			num = 0;
		}
		CurrentName = TextureNames[num];
		CurrentTexture = Content.Load<Texture2D>(CurrentName);
	}

	public void PrevTexture()
	{
		int num = Array.IndexOf(TextureNames, CurrentName) - 1;
		if (num < 0)
		{
			num = TextureNames.Length - 1;
		}
		CurrentName = TextureNames[num];
		CurrentTexture = Content.Load<Texture2D>(CurrentName);
	}

	protected override void DrawUi(SpriteBatch spriteBatch)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		IAssetName? currentName = CurrentName;
		string text = ((currentName != null) ? currentName.Name : null) ?? "???";
		Vector2 val = Game1.smallFont.MeasureString(text);
		float x = val.X;
		Texture2D? currentTexture = CurrentTexture;
		int num = (int)Math.Max(x, (currentTexture != null) ? currentTexture.Width : 0);
		Vector2 position = new Vector2((float)Margin);
		float num2 = num;
		float num3 = val.Y + (float)Padding;
		Texture2D? currentTexture2 = CurrentTexture;
		CommonHelper.DrawScroll(spriteBatch, in position, new Vector2(num2, num3 + (float)((currentTexture2 != null) ? currentTexture2.Height : ((int)val.Y))), out var contentPos, out var _, Padding);
		spriteBatch.DrawString(Game1.smallFont, text, new Vector2(contentPos.X + ((float)num - val.X) / 2f, contentPos.Y), Color.Black);
		if (CurrentTexture != null)
		{
			spriteBatch.Draw(CurrentTexture, contentPos + new Vector2(0f, val.Y + (float)Padding), Color.White);
		}
		else
		{
			spriteBatch.DrawString(Game1.smallFont, "(null)", contentPos + new Vector2(0f, val.Y + (float)Padding), Color.Black);
		}
	}

	private IEnumerable<IAssetName> GetTextureNames(IGameContentHelper contentHelper)
	{
		List<IAssetName> textureKeys = new List<IAssetName>();
		contentHelper.InvalidateCache((Func<IAssetInfo, bool>)((IAssetInfo asset) =>
		{
			IAssetName name = asset.Name;
			if (typeof(Texture2D).IsAssignableFrom(asset.DataType) && !name.Name.Contains("..") && !name.StartsWith(Constants.GamePath, true, true))
			{
				textureKeys.Add(name);
			}
			return false;
		}));
		return textureKeys;
	}
}
