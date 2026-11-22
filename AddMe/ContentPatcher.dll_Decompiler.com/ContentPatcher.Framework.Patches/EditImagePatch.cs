using System;
using System.Collections.Generic;
using System.IO;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Internal;

namespace ContentPatcher.Framework.Patches;

internal class EditImagePatch : Patch
{
	private readonly IMonitor Monitor;

	private readonly TokenRectangle? FromArea;

	private readonly TokenRectangle? ToArea;

	private readonly PatchImageMode PatchMode;

	private bool ResizedLastImage;

	internal static bool EnablePyTkLegacyMode;

	public EditImagePatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IManagedTokenString? assetLocale, AssetEditPriority priority, IEnumerable<Condition> conditions, IManagedTokenString fromAsset, TokenRectangle? fromArea, TokenRectangle? toArea, PatchImageMode patchMode, UpdateRate updateRate, InvariantDictionary<IManagedTokenString>? inheritedLocalTokens, InvariantDictionary<IManagedTokenString>? localTokens, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, IMonitor monitor, Func<string, IAssetName> parseAssetName)
		: base(indexPath, path, PatchType.EditImage, assetName, assetLocale, (int)priority, updateRate, inheritedLocalTokens, localTokens, conditions, contentPack, migrator, parentPatch, parseAssetName, fromAsset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected I4, but got Unknown
		FromArea = fromArea;
		ToArea = toArea;
		PatchMode = patchMode;
		Monitor = monitor;
		Contextuals.Add(fromArea).Add(toArea);
	}

	public override void Edit<T>(IAssetData asset)
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		if (typeof(T) != typeof(Texture2D))
		{
			Warn($"this file isn't an image file (found {typeof(T)}).", (LogLevel)3);
			return;
		}
		if (!FromAssetExists())
		{
			Warn($"the {"FromFile"} file '{FromAsset}' doesn't exist.", (LogLevel)3);
			return;
		}
		IAssetDataForImage val = asset.AsImage();
		LoadSourceImage(FromAsset, out int width, out int height, out IRawTextureData rawData, out Texture2D fullTexture);
		Rectangle area2;
		if (!TryReadArea(FromArea, 0, 0, width, height, out Rectangle area, out string error))
		{
			Warn("the source area is invalid: " + error + ".", (LogLevel)3);
		}
		else if (!TryReadArea(ToArea, 0, 0, area.Width, area.Height, out area2, out error))
		{
			Warn("the target area is invalid: " + error + ".", (LogLevel)3);
		}
		else if (area.X < 0 || area.Y < 0 || area.Width < 0 || area.Height < 0)
		{
			Warn($"source area (X:{area.X}, Y:{area.Y}, Width:{area.Width}, Height:{area.Height}) has negative values, which isn't valid.", (LogLevel)4);
		}
		else if (area2.X < 0 || area2.Y < 0 || area2.Width < 0 || area2.Height < 0)
		{
			Warn($"target area (X:{area2.X}, Y:{area2.Y}, Width:{area2.Width}, Height:{area2.Height}) has negative values, which isn't valid.", (LogLevel)4);
		}
		else if (area2.Right > ((IAssetData<Texture2D>)(object)val).Data.Width)
		{
			Warn($"target area (X:{area2.X}, Y:{area2.Y}, Width:{area2.Width}, Height:{area2.Height}) extends past the right edge of the image (Width:{((IAssetData<Texture2D>)(object)val).Data.Width}), which isn't allowed. Patches can only extend the tilesheet downwards.", (LogLevel)4);
		}
		else if (area.Width != area2.Width || area.Height != area2.Height)
		{
			string value = ((FromArea != null) ? "FromArea" : "source image");
			string value2 = ((ToArea != null) ? "ToArea" : "target image");
			Warn($"{value} size (Width:{area.Width}, Height:{area.Height}) doesn't match {value2} size (Width:{area2.Width}, Height:{area2.Height}).", (LogLevel)4);
		}
		else
		{
			try
			{
				ResizedLastImage = val.ExtendImage(((IAssetData<Texture2D>)(object)val).Data.Width, area2.Bottom);
			}
			catch (InvalidCastException ex) when (ex.Message.Contains("SpritesInDetail.ReplacedTexture"))
			{
				Monitor.Log($"Failed applying image patch {Path}: image has been edited by Sprites In Detail, so it can no longer be resized by content packs.", (LogLevel)4);
				Monitor.Log(ExceptionHelper.GetLogSummary((Exception)ex), (LogLevel)0);
				return;
			}
			if (rawData != null)
			{
				val.PatchImage(rawData, (Nullable)area, (Nullable)area2, (PatchMode)PatchMode);
			}
			else
			{
				val.PatchImage(fullTexture, (Nullable)area, (Nullable)area2, (PatchMode)PatchMode);
			}
		}
	}

	public override IEnumerable<string> GetChangeLabels()
	{
		if (ResizedLastImage)
		{
			yield return "resized image";
		}
		yield return "edited image";
	}

	private void LoadSourceImage(string fromAsset, out int width, out int height, out IRawTextureData? rawData, out Texture2D? fullTexture)
	{
		bool flag = !string.Equals(System.IO.Path.GetExtension(fromAsset), ".xnb", StringComparison.OrdinalIgnoreCase);
		if (flag && EnablePyTkLegacyMode)
		{
			flag = false;
			Monitor.LogOnce("Enabled compatibility mode for PyTK 1.23.* or earlier. This won't cause any issues, but may impact performance.", (LogLevel)3);
		}
		if (flag)
		{
			rawData = ContentPack.ModContent.Load<IRawTextureData>(fromAsset);
			fullTexture = null;
			width = rawData.Width;
			height = rawData.Height;
		}
		else
		{
			rawData = null;
			fullTexture = ContentPack.ModContent.Load<Texture2D>(fromAsset);
			width = fullTexture.Width;
			height = fullTexture.Height;
		}
	}

	private void Warn(string message, LogLevel level = (LogLevel)3)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Monitor.Log($"Can't apply image patch \"{Path}\" to {TargetAsset}: {message}", level);
	}
}
