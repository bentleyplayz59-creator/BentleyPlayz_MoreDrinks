using System;
using System.Diagnostics.CodeAnalysis;
using Netcode;
using StardewValley;
using StardewValley.Mods;
using StardewValley.Network;

namespace Pathoschild.Stardew.Common;

internal static class ModDataDictionaryExtensions
{
	[return: NotNullIfNotNull("defaultValue")]
	public static T? ReadField<T>(this ModDataDictionary data, string key, Func<string, T> parse, T? defaultValue = default(T?))
	{
		string arg = default;
		if (((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)data).TryGetValue(key, ref arg))
		{
			try
			{
				return parse(arg);
			}
			catch
			{
			}
		}
		return defaultValue;
	}

	[return: NotNullIfNotNull("defaultValue")]
	public static string? ReadField(this ModDataDictionary data, string key, string? defaultValue = null)
	{
		string text = default;
		return ((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)data).TryGetValue(key, ref text) ? text : defaultValue;
	}

	public static ModDataDictionary WriteField(this ModDataDictionary data, string key, string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)data).Remove(key);
		}
		else
		{
			((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)data)[key] = value;
		}
		return data;
	}
}
