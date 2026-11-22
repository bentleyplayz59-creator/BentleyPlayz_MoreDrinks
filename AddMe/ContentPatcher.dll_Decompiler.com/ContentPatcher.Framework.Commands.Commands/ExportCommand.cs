using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Pathoschild.Stardew.Common.Commands;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Framework.ContentManagers;
using StardewModdingAPI.Toolkit.Serialization;
using StardewValley;
using TMXTile;
using xTile;

namespace ContentPatcher.Framework.Commands.Commands;

internal class ExportCommand : BaseCommand
{
	private readonly IGameContentHelper ContentHelper;

	private readonly Lazy<JsonSerializerSettings> JsonSettings = new Lazy<JsonSerializerSettings>((Func<JsonSerializerSettings>)JsonHelper.CreateDefaultSettings);

	public ExportCommand(IMonitor monitor, IGameContentHelper contentHelper)
		: base(monitor, "export")
	{
		ContentHelper = contentHelper;
	}

	public override string GetDescription()
	{
		return "patch export\n   Usage: patch export \"<asset name>\"\n   Saves a copy of an asset (including any changes from mods like Content Patcher) to the game folder. The asset name should be the target without the locale or extension, like \"Characters/Abigail\" if you want to export the value of 'Content/Characters/Abigail.xnb'.\n\n   Usage: patch export \"<asset name>\" \"<C# type name>\"\n   Same as the previous, but manually specify the C# data type for the asset. This must be the C# full type name. Example types:\n      - A string/string dictionary: System.Collections.Generic.Dictionary`2[[System.String],[System.String]]\n      - A number/string dictionary: System.Collections.Generic.Dictionary`2[[System.Int32],[System.String]]\n      - Movie reactions: System.Collections.Generic.List<StardewValley.GameData.Movies.MovieReaction>\n\n  You can also specify 'image' as the type for a Texture2D value, or 'map' for a xTile.Map.";
	}

	public override void Handle(string[] args)
	{
		int num = args.Length;
		if ((num < 1 || num > 2) ? true : false)
		{
			Monitor.Log("The 'patch export' command expects one argument containing the target asset name, and an optional second argument for the data type. See 'patch help' for more info.", (LogLevel)4);
			return;
		}
		string text = args[0];
		string text2 = ((args.Length > 1) ? args[1] : null);
		List<Type> list = TryGetTypes(text2).ToList();
		switch (list.Count)
		{
		case 0:
			Monitor.Log("Couldn't find type '" + text2 + "'. Type `patch help export` for usage.", (LogLevel)4);
			break;
		default:
			Monitor.Log($"Found multiple types matching '{text2}'. Please enter one of these exact values:\n    - \"{string.Join("\n    - \"", list.Select((Type possibleType) => possibleType.AssemblyQualifiedName))}\"", (LogLevel)4);
			break;
		case 1:
		{
			if (list[0] == typeof(object))
			{
				List<Type> likelyTypes = GetLikelyTypes(text);
				if (likelyTypes != null)
				{
					list.InsertRange(0, likelyTypes);
				}
			}
			object obj = null;
			Dictionary<Type, Exception> dictionary = null;
			try
			{
				Exception innerException = default;
				foreach (Type item in list)
				{
					try
					{
						obj = LoadAsset(text, item);
						if (obj != null)
						{
							break;
						}
					}
					catch (Exception ex) when (((Func<bool>)delegate
					{
						// Could not convert BlockContainer to single expression
						innerException = ex.InnerException;
						return (innerException is ContentLoadException || innerException is InvalidCastException) ? true : false;
					}).Invoke())
					{
						if (dictionary == null)
						{
							dictionary = new Dictionary<Type, Exception>();
						}
						dictionary.Add(item, ex.InnerException);
					}
				}
			}
			catch (Exception ex2)
			{
				Monitor.Log("Can't load asset '" + text + "': " + ex2.Message, (LogLevel)4);
			}
			if (obj == null)
			{
				Monitor.Log($"Couldn't load asset '{text}' using a likely type (tried: {string.Join(", ", list.Select((Type p) => p.FullName))}). Try either specifying the type, or waiting until the game loads it before exporting it. See the SMAPI log for details.", (LogLevel)4);
				if (dictionary == null)
				{
					break;
				}
				{
					foreach (var (type2, value) in dictionary)
					{
						Monitor.Log($"Failed to load using type '{type2.FullName}': {value}", (LogLevel)0);
					}
					break;
				}
			}
			string path = Path.Combine(Constants.GamePath, "patch export", GetSanitizedFileName(text));
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			if (TryExportRaw(obj, ref path, out string error))
			{
				Monitor.Log($"Exported asset '{text}' to '{path}'.", (LogLevel)2);
			}
			else
			{
				Monitor.Log($"Failed exporting '{text}': {error}.", (LogLevel)4);
			}
			break;
		}
		}
	}

	private bool TryExportRaw(object? asset, ref string path, [NotNullWhen(false)] out string? error)
	{
		error = null;
		if (asset != null)
		{
			Map val = (Map)((asset is Map) ? asset : null);
			if (val == null)
			{
				Texture2D val2 = (Texture2D)((asset is Texture2D) ? asset : null);
				if (val2 == null)
				{
					IRawTextureData val3 = (IRawTextureData)((asset is IRawTextureData) ? asset : null);
					if (val3 != null)
					{
						path += ".png";
						ExportRawTexture(val3, path);
						return true;
					}
					path += ".json";
					ExportData(asset, path);
					return true;
				}
				path += ".png";
				ExportTexture(val2, path);
				return true;
			}
			path += ".tmx";
			ExportMap(val, path);
			return true;
		}
		error = "the asset could not be loaded";
		return false;
	}

	private void ExportMap(Map rawMap, string path)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		//IL_00f4: Expected Obj, but got Unknown
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Expected Obj, but got Unknown
		TMXFormat val = new TMXFormat(16, 16, 4, 4);
		TMXMap val2 = val.Store(rawMap);
		foreach (TMXObjectgroup objectgroup in val2.Objectgroups)
		{
			for (int i = 0; i < objectgroup.Objects.Count; i++)
			{
				if (objectgroup.Objects[i].Properties.Length == 0)
				{
					objectgroup.Objects[i] = null;
				}
			}
		}
		string directoryName = Path.GetDirectoryName(path);
		foreach (TMXTileset tileset in val2.Tilesets)
		{
			string source = tileset.Image.Source;
			tileset.Image.Source = Path.GetFileName(source);
			string text = null;
			Texture2D val3;
			try
			{
				val3 = LoadAssetImpl<Texture2D>(source);
			}
			catch (ContentLoadException ex)
			{
				ContentLoadException ex2 = ex;
				val3 = null;
				text = ((Exception)(object)ex2).Message;
			}
			if (val3 == null)
			{
				Monitor.Log($"Failed while attempting to export tilesheets for map: Can't load asset '{source}' with type '{typeof(Texture2D).FullName}'{((text != null) ? (": " + text) : ".")}", (LogLevel)4);
			}
			else
			{
				val3 = UnPremultiplyTransparency(val3);
				string sanitizedFileName = GetSanitizedFileName(Path.GetRelativePath(Constants.GamePath, source));
				string text2 = Path.Combine(directoryName, sanitizedFileName) + ".png";
				ExportTexture(val3, text2);
				tileset.Image.Source = sanitizedFileName;
				Monitor.Log($"Exported asset '{source}' to '{text2}'.", (LogLevel)2);
			}
		}
		TMXParser val4 = new TMXParser();
		val4.Export(val2, path, (DataEncodingType)0);
	}

	private void ExportRawTexture(IRawTextureData image, string path)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
		Texture2D val = new Texture2D(Game1.graphics.GraphicsDevice, image.Width, image.Height);
		try
		{
			val.SetData<Color>(image.Data);
			ExportTexture(val, path);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ExportTexture(Texture2D image, string path)
	{
		Texture2D val = UnPremultiplyTransparency(image);
		try
		{
			using Stream stream = File.Create(path);
			val.SaveAsPng(stream, val.Width, val.Height);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ExportData(object data, string path)
	{
		File.WriteAllText(path, JsonConvert.SerializeObject(data, JsonSettings.Value));
	}

	private string GetSanitizedFileName(string assetName)
	{
		return string.Join('_', assetName.Split(Path.GetInvalidFileNameChars()));
	}

	private Type[] TryGetTypes(string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return new Type[1] { typeof(object) };
		}
		if (string.Equals(name, "image", StringComparison.OrdinalIgnoreCase))
		{
			return new Type[1] { typeof(Texture2D) };
		}
		if (string.Equals(name, "map", StringComparison.OrdinalIgnoreCase))
		{
			return new Type[1] { typeof(Map) };
		}
		Type type = Type.GetType(name);
		if (type != null)
		{
			return new Type[1] { type };
		}
		HashSet<Type> hashSet = new HashSet<Type>();
		HashSet<Type> hashSet2 = new HashSet<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.IsDynamic)
			{
				continue;
			}
			Type[] exportedTypes = assembly.GetExportedTypes();
			foreach (Type type2 in exportedTypes)
			{
				try
				{
					if (string.Equals(type2.FullName, name, StringComparison.OrdinalIgnoreCase))
					{
						hashSet2.Add(type2);
					}
					if (string.Equals(type2.Name, name, StringComparison.OrdinalIgnoreCase))
					{
						hashSet.Add(type2);
					}
				}
				catch
				{
				}
			}
		}
		HashSet<Type> source = (hashSet2.Any() ? hashSet2 : hashSet);
		return source.OrderBy((Type p) => p.FullName, HumanSortComparer.DefaultIgnoreCase).ToArray();
	}

	private List<Type>? GetLikelyTypes(string asset)
	{
		IAssetName val = ContentHelper.ParseAssetName(asset);
		if (val.IsDirectlyUnderPath("Maps"))
		{
			List<Type> list = new List<Type>(2);
			list.Add(typeof(Map));
			list.Add(typeof(Texture2D));
			return list;
		}
		if (val.IsDirectlyUnderPath("Animals") || val.IsDirectlyUnderPath("Buildings") || val.IsDirectlyUnderPath("Characters") || val.IsDirectlyUnderPath("Portraits") || val.IsDirectlyUnderPath("Minigames") || val.IsDirectlyUnderPath("TerrainFeatures") || val.IsDirectlyUnderPath("TileSheets"))
		{
			List<Type> list = new List<Type>(1);
			list.Add(typeof(Texture2D));
			return list;
		}
		if (val.IsDirectlyUnderPath("Characters/Dialogue") || val.IsDirectlyUnderPath("Characters/schedules") || val.IsDirectlyUnderPath("Data/Events") || val.IsDirectlyUnderPath("Data/Festivals"))
		{
			List<Type> list = new List<Type>(1);
			list.Add(typeof(Dictionary<string, string>));
			return list;
		}
		if (val.IsDirectlyUnderPath("Data"))
		{
			string baseName = val.BaseName;
			int length = "Data/".Length;
			string text = baseName.Substring(length, baseName.Length - length);
			if (text.Contains('_'))
			{
				return null;
			}
			MethodInfo method = typeof(DataLoader).GetMethod(text, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public);
			if (method != null)
			{
				return new List<Type>(1) { method.ReturnType };
			}
		}
		return null;
	}

	private Texture2D UnPremultiplyTransparency(Texture2D texture)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected Obj, but got Unknown
		Color[] array = new Color[texture.Width * texture.Height];
		texture.GetData<Color>(array);
		for (int i = 0; i < array.Length; i++)
		{
			Color val = array[i];
			if (val.A != 0)
			{
				array[i] = new Color((byte)(val.R * 255 / val.A), (byte)(val.G * 255 / val.A), (byte)(val.B * 255 / val.A), val.A);
			}
		}
		Texture2D val2 = new Texture2D(((GraphicsResource)texture).GraphicsDevice ?? Game1.graphics.GraphicsDevice, texture.Width, texture.Height);
		val2.SetData<Color>(array);
		return val2;
	}

	private object? LoadAsset(string assetName, Type type)
	{
		return GetType().GetMethod("LoadAssetImpl", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(type).Invoke(this, new object[1] { assetName });
	}

	private TAsset? LoadAssetImpl<TAsset>(string assetName)
	{
		if (IsAssetLoaded((ContentManager)(object)Game1.content, assetName))
		{
			return Game1.content.Load<TAsset>(assetName);
		}
		ContentManager val = (ContentManager)(object)Game1.content.CreateTemporary();
		try
		{
			return val.Load<TAsset>(assetName);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool IsAssetLoaded(ContentManager contentManager, string assetName)
	{
		IAssetName val = ContentHelper.ParseAssetName(assetName);
		IContentManager val2 = (IContentManager)(object)((contentManager is IContentManager) ? contentManager : null);
		if (val2 == null)
		{
			throw new InvalidOperationException($"Can't access internals for content manager with type {((object)contentManager)?.GetType().FullName ?? "null"}, expected implementation of {typeof(IContentManager).FullName}.");
		}
		return val2.IsLoaded(val);
	}
}
