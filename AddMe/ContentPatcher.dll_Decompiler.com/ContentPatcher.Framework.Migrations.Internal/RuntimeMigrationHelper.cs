using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;

namespace ContentPatcher.Framework.Migrations.Internal;

internal static class RuntimeMigrationHelper
{
	private static readonly Dictionary<string, string?> ParseObjectIdCache = new Dictionary<string, string>();

	public static string? ParseObjectId(string? rawItemId)
	{
		if (rawItemId == null)
		{
			return null;
		}
		Dictionary<string, string> parseObjectIdCache = ParseObjectIdCache;
		if (parseObjectIdCache.TryGetValue(rawItemId, out var value))
		{
			return value;
		}
		ItemMetadata metadata = ItemRegistry.GetMetadata(rawItemId);
		bool? flag = ((metadata != null) ? new bool?(metadata.Exists()) : ((bool?)null));
		if (!flag.HasValue || flag != true || metadata.TypeIdentifier != "(O)")
		{
			parseObjectIdCache[rawItemId] = null;
			return null;
		}
		parseObjectIdCache[rawItemId] = metadata.LocalItemId;
		return metadata.LocalItemId;
	}

	public static int CountFields(string row, char delimiter = '/')
	{
		int num = 1;
		int num2 = -1;
		while (true)
		{
			num2 = row.IndexOf('/', num2 + 1);
			if (num2 == -1)
			{
				break;
			}
			num++;
		}
		return num;
	}

	public static string? MigrateLiteralTextToTokenizableField(string? newValue, string? prevValue, string? assetValue)
	{
		return (!string.IsNullOrWhiteSpace(newValue) && newValue != prevValue && newValue != TokenParser.ParseText(assetValue, (Random)null, (TokenParserDelegate)null, (Farmer)null)) ? newValue : assetValue;
	}
}
