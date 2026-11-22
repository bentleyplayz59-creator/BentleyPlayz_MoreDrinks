using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.TextOperations;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Extensions;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace ContentPatcher.Framework.Patches;

internal class EditMapPatch : Patch
{
	private readonly IMonitor Monitor;

	private readonly TokenRectangle? FromArea;

	private readonly TokenRectangle? ToArea;

	private readonly PatchMapMode PatchMode;

	private readonly EditMapPatchProperty[] MapProperties;

	private readonly EditMapPatchTile[] MapTiles;

	public readonly IManagedTokenString[] AddNpcWarps;

	public readonly IManagedTokenString[] AddWarps;

	private readonly ITextOperation[] TextOperations;

	private bool AppliesMapPatch => RawFromAsset != null;

	private bool AppliesTilePatches => MapTiles.Length != 0;

	public EditMapPatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IManagedTokenString? assetLocale, AssetEditPriority priority, IEnumerable<Condition> conditions, IManagedTokenString? fromAsset, TokenRectangle? fromArea, TokenRectangle? toArea, PatchMapMode patchMode, IEnumerable<EditMapPatchProperty>? mapProperties, IEnumerable<EditMapPatchTile>? mapTiles, IEnumerable<IManagedTokenString>? addNpcWarps, IEnumerable<IManagedTokenString>? addWarps, IEnumerable<ITextOperation>? textOperations, UpdateRate updateRate, InvariantDictionary<IManagedTokenString>? inheritedLocalTokens, InvariantDictionary<IManagedTokenString>? localTokens, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, IMonitor monitor, Func<string, IAssetName> parseAssetName)
		: base(indexPath, path, PatchType.EditMap, assetName, assetLocale, (int)priority, updateRate, inheritedLocalTokens, localTokens, conditions, contentPack, migrator, parentPatch, parseAssetName, fromAsset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected I4, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		FromArea = fromArea;
		ToArea = toArea;
		PatchMode = patchMode;
		MapProperties = mapProperties?.ToArray() ?? Array.Empty<EditMapPatchProperty>();
		MapTiles = mapTiles?.ToArray() ?? Array.Empty<EditMapPatchTile>();
		AddNpcWarps = addNpcWarps?.Reverse().ToArray() ?? Array.Empty<IManagedTokenString>();
		AddWarps = addWarps?.Reverse().ToArray() ?? Array.Empty<IManagedTokenString>();
		TextOperations = textOperations?.ToArray() ?? Array.Empty<ITextOperation>();
		Monitor = monitor;
		Contextuals.Add(FromArea).Add(ToArea).Add(MapProperties)
			.Add(MapTiles)
			.Add(AddNpcWarps)
			.Add(AddWarps)
			.Add(TextOperations);
	}

	public override void Edit<T>(IAssetData asset)
	{
		if (typeof(T) != typeof(Map))
		{
			WarnForPatch($"this file isn't a map file (found {typeof(T)}).");
			return;
		}
		if (AppliesMapPatch && !FromAssetExists())
		{
			WarnForPatch($"the {"FromFile"} file '{FromAsset}' doesn't exist.");
			return;
		}
		IAssetDataForMap val = asset.AsMap();
		Map data = ((IAssetData<Map>)(object)val).Data;
		if (AppliesMapPatch)
		{
			Map source = ContentPack.ModContent.Load<Map>(FromAsset);
			if (!TryApplyMapPatch(source, val, out string error))
			{
				WarnForPatch("map patch couldn't be applied: " + error);
			}
		}
		if (AppliesTilePatches)
		{
			int num = 0;
			EditMapPatchTile[] mapTiles = MapTiles;
			foreach (EditMapPatchTile tilePatch in mapTiles)
			{
				num++;
				if (!TryApplyTile(data, tilePatch, out string error2))
				{
					WarnForPatch($"{"MapTiles"} > entry {num} couldn't be applied: {error2}");
				}
			}
		}
		EditMapPatchProperty[] mapProperties = MapProperties;
		foreach (EditMapPatchProperty editMapPatchProperty in mapProperties)
		{
			string value = editMapPatchProperty.Key.Value;
			string text = editMapPatchProperty.Value?.Value;
			if (text == null)
			{
				((IDictionary<string, PropertyValue>)((Component)data).Properties).Remove(value);
			}
			else
			{
				((IDictionary<string, PropertyValue>)((Component)data).Properties)[value] = PropertyValue.op_Implicit(text);
			}
		}
		string value2;
		string key;
		if (AddNpcWarps.Length != 0)
		{
			ApplyWarps(data, AddNpcWarps, out IDictionary<string, string> errors, "NPCWarp");
			foreach (KeyValuePair<string, string> item in errors)
			{
				item.Deconstruct(out value2, out key);
				string value3 = value2;
				string value4 = key;
				WarnForPatch($"{"AddNpcWarps"} > warp '{value3}' couldn't be applied: {value4}");
			}
		}
		if (AddWarps.Length != 0)
		{
			ApplyWarps(data, AddWarps, out IDictionary<string, string> errors2, "Warp");
			foreach (KeyValuePair<string, string> item2 in errors2)
			{
				item2.Deconstruct(out key, out value2);
				string value5 = key;
				string value6 = value2;
				WarnForPatch($"{"AddWarps"} > warp '{value5}' couldn't be applied: {value6}");
			}
		}
		for (int k = 0; k < TextOperations.Length; k++)
		{
			if (!TryApplyTextOperation(data, TextOperations[k], out string error3))
			{
				WarnForPatch($"{"TextOperations"} > entry {k} couldn't be applied: {error3}");
			}
		}
	}

	public override IEnumerable<string> GetChangeLabels()
	{
		if (AppliesMapPatch || AppliesTilePatches)
		{
			yield return "patched map tiles";
		}
		if (MapProperties.Length != 0 || AddNpcWarps.Length != 0 || AddWarps.Length != 0)
		{
			yield return "changed map properties";
		}
		if (TextOperations.Length != 0)
		{
			yield return "applied text operations";
		}
	}

	private bool TryApplyMapPatch(Map source, IAssetDataForMap targetAsset, [NotNullWhen(false)] out string? error)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		Rectangle mapArea = GetMapArea(source);
		if (!TryReadArea(FromArea, 0, 0, mapArea.Width, mapArea.Height, out var area, out error))
		{
			return Fail("the source area is invalid: " + error + ".", out error);
		}
		if (!TryReadArea(ToArea, 0, 0, area.Width, area.Height, out var area2, out error))
		{
			return Fail("the target area is invalid: " + error + ".", out error);
		}
		string value = ((FromArea != null) ? "FromArea" : "source map");
		string value2 = ((ToArea != null) ? "ToArea" : "target map");
		Point value3 = new Point(source.Layers.Max((Layer p) => p.LayerWidth), source.Layers.Max((Layer p) => p.LayerHeight));
		if (!TryValidateArea(area, value3, "source", out error))
		{
			return Fail(error, out error);
		}
		if (!TryValidateArea(area2, null, "target", out error))
		{
			return Fail(error, out error);
		}
		if (area.Width != area2.Width || area.Height != area2.Height)
		{
			return Fail($"{value} size (Width:{area.Width}, Height:{area.Height}) doesn't match {value2} size (Width:{area2.Width}, Height:{area2.Height}).", out error);
		}
		targetAsset.ExtendMap(area2.Right, area2.Bottom);
		targetAsset.PatchMap(source, (Nullable)area, (Nullable)area2, PatchMode);
		error = null;
		return true;
	}

	private bool TryApplyTile(Map map, EditMapPatchTile tilePatch, [NotNullWhen(false)] out string? error)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Expected Obj, but got Unknown
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		if (!TryReadTile(tilePatch, out string layerName, out Location position, out int? setIndex, out string setTilesheetId, out Dictionary<string, string> properties, out bool remove, out error))
		{
			return Fail(error, out error);
		}
		bool flag = setIndex.HasValue || setTilesheetId != null || properties.Count > 0;
		Layer layer = map.GetLayer(layerName);
		if (layer == null)
		{
			return Fail("Layer specifies a '" + layerName + "' layer which doesn't exist.", out error);
		}
		TileSheet val = null;
		if (setTilesheetId != null)
		{
			val = map.GetTileSheet(setTilesheetId);
			if (val == null)
			{
				return Fail("SetTilesheet specifies a '" + setTilesheetId + "' tilesheet which doesn't exist.", out error);
			}
		}
		if (!layer.IsValidTileLocation(position))
		{
			return Fail($"{"Position"} specifies a tile position '{position.X}, {position.Y}' which is outside the map area.", out error);
		}
		Tile val2 = layer.Tiles[position];
		if (flag && (remove || val2 == null) && (val == null || !setIndex.HasValue))
		{
			return Fail($"the map has no tile at {layerName} ({position.X}, {position.Y}). To add a tile, the {"SetTilesheet"} and {"SetIndex"} fields must be set.", out error);
		}
		if (remove)
		{
			layer.Tiles[position] = null;
		}
		if (val != null || setIndex.HasValue || properties.Count > 0)
		{
			StaticTile val3 = new StaticTile(layer, val ?? val2.TileSheet, (BlendMode)((val2 != null) ? ((int)val2.BlendMode) : 0), setIndex ?? val2.TileIndex);
			string key;
			if (val2 != null && ((ICollection<KeyValuePair<string, PropertyValue>>)((Component)val2).Properties).Count > 0)
			{
				foreach (KeyValuePair<string, PropertyValue> item in (IEnumerable<KeyValuePair<string, PropertyValue>>)((Component)val2).Properties)
				{
					item.Deconstruct(out key, out var value);
					string key2 = key;
					PropertyValue value2 = value;
					((IDictionary<string, PropertyValue>)((Component)val3).Properties)[key2] = value2;
				}
			}
			foreach (KeyValuePair<string, string> item2 in properties)
			{
				item2.Deconstruct(out key, out var value3);
				string key3 = key;
				string text = value3;
				if (text == null)
				{
					((IDictionary<string, PropertyValue>)((Component)val3).Properties).Remove(key3);
				}
				else
				{
					((IDictionary<string, PropertyValue>)((Component)val3).Properties)[key3] = PropertyValue.op_Implicit(text);
				}
			}
			layer.Tiles[position] = (Tile)(object)val3;
		}
		error = null;
		return true;
	}

	private void ApplyWarps(Map target, IManagedTokenString[] addWarps, out IDictionary<string, string> errors, string propertyName)
	{
		errors = new InvariantDictionary<string>();
		List<string> list = new List<string>(addWarps.Length);
		foreach (string item in addWarps.Select((IManagedTokenString p) => p.Value))
		{
			if (!ValidateWarp(item, out string error))
			{
				errors[item ?? "<null>"] = error;
			}
			else
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			string text2 = default;
			string text = (FrameworkExtensions.TryGetValue(((Component)target).Properties, propertyName, ref text2) ? text2 : "");
			string text3 = string.Join(" ", list);
			((IDictionary<string, PropertyValue>)((Component)target).Properties)[propertyName] = PropertyValue.op_Implicit((text3 + " " + text).Trim());
		}
	}

	private bool ValidateWarp([NotNullWhen(true)] string? warp, [NotNullWhen(false)] out string? error)
	{
		if (warp == null)
		{
			return Fail("warp cannot be null", out error);
		}
		string[] array = warp.Split(' ');
		if (array.Length != 5)
		{
			return Fail("must have exactly five fields in the form `fromX fromY toMap toX toY`.", out error);
		}
		string[] array2 = new string[4]
		{
			array[0],
			array[1],
			array[3],
			array[4]
		};
		foreach (string text in array2)
		{
			if (!int.TryParse(text, out var _) || text.Any((char ch) => !char.IsDigit(ch) && ch != '-'))
			{
				return Fail("can't parse '" + text + "' as a tile coordinate number.", out error);
			}
		}
		if (array[2].Trim().Length == 0)
		{
			return Fail("the map value can't be blank.", out error);
		}
		error = null;
		return true;
	}

	private bool TryApplyTextOperation(Map target, ITextOperation operation, [NotNullWhen(false)] out string? error)
	{
		TextOperationTargetRoot? targetRoot = operation.GetTargetRoot();
		TextOperationTargetRoot? textOperationTargetRoot = targetRoot;
		TextOperationTargetRoot? textOperationTargetRoot2 = textOperationTargetRoot;
		if (textOperationTargetRoot2.HasValue)
		{
			TextOperationTargetRoot valueOrDefault = textOperationTargetRoot2.GetValueOrDefault();
			if (valueOrDefault == TextOperationTargetRoot.MapProperties)
			{
				if (operation.Target.Length > 2)
				{
					return Fail($"a '{TextOperationTargetRoot.MapProperties}' path must only have one other segment for the property name.", out error);
				}
				string value = operation.Target[1].Value;
				string text2 = default;
				string text = (FrameworkExtensions.TryGetValue(((Component)target).Properties, value, ref text2) ? text2 : null);
				string text3 = operation.Apply(text);
				if (text3 != null)
				{
					((IDictionary<string, PropertyValue>)((Component)target).Properties)[value] = PropertyValue.op_Implicit(text3);
				}
				error = null;
				return true;
			}
		}
		return Fail((!targetRoot.HasValue) ? $"unknown path root '{operation.Target[0]}'." : $"path root '{targetRoot}' isn't valid for an {"EditMap"} patch", out error);
	}

	private bool TryReadTile(EditMapPatchTile tile, out string? layerName, out Location position, out int? setIndex, out string? setTilesheetId, out Dictionary<string, string?> properties, out bool remove, [NotNullWhen(false)] out string? error)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		setIndex = null;
		position = Location.Origin;
		properties = new Dictionary<string, string>();
		remove = false;
		layerName = tile.Layer?.Value;
		setTilesheetId = tile.SetTilesheet?.Value;
		if (tile.SetIndex.IsMeaningful())
		{
			if (!int.TryParse(tile.SetIndex.Value, out var result))
			{
				return Fail($"{"SetIndex"} specifies '{tile.SetIndex}', which isn't a valid number.", out error);
			}
			setIndex = result;
		}
		if (!tile.Position.TryGetLocation(out position, out error))
		{
			return Fail($"{"Position"} specifies '{tile.Position.X}, {tile.Position.Y}', which isn't a valid position.", out error);
		}
		foreach (var (tokenString3, tokenString4) in tile.SetProperties)
		{
			properties[tokenString3.Value] = tokenString4?.Value;
		}
		if (tile.Remove.IsMeaningful() && !bool.TryParse(tile.Remove.Value, out remove))
		{
			return Fail($"{"Remove"} specifies '{tile.Remove}', which isn't a valid boolean value (must be 'true' or 'false').", out error);
		}
		error = null;
		return true;
	}

	private bool TryValidateArea(Rectangle area, Nullable maxSize, string name, [NotNullWhen(false)] out string? error)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (area.X < 0 || area.Y < 0 || area.Width < 0 || area.Height < 0)
		{
			return FailArea(area, name, "has negative values, which isn't valid.", out error);
		}
		if (maxSize.HasValue && (area.Right > maxSize.Value.X || area.Bottom > maxSize.Value.Y))
		{
			return FailArea(area, name, "extends past the edges of the " + name + " map, which isn't allowed.", out error);
		}
		error = null;
		return true;
	}

	private Rectangle GetMapArea(Map map)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		foreach (Layer layer in map.Layers)
		{
			if (layer.LayerWidth > num)
			{
				num = layer.LayerWidth;
			}
			if (layer.LayerHeight > num2)
			{
				num2 = layer.LayerHeight;
			}
		}
		return new Rectangle(0, 0, num, num2);
	}

	private bool FailArea(Rectangle area, string name, string inError, out string outError)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		return Fail($"{name} area (X:{area.X}, Y:{area.Y}, Width:{area.Width}, Height:{area.Height}) {inError}", out outError);
	}

	private void WarnForPatch(string message)
	{
		Monitor.Log($"Can't apply map patch \"{Path}\" to {TargetAsset}: {message}", (LogLevel)3);
	}
}
