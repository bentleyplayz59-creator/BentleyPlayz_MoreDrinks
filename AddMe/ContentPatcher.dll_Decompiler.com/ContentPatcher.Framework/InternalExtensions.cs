using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;

namespace ContentPatcher.Framework;

internal static class InternalExtensions
{
	public static IEnumerable<string> ExceptIgnoreCase(this IEnumerable<string> source, IEnumerable<string> other)
	{
		return source.Except(other, StringComparer.OrdinalIgnoreCase);
	}

	public static IEnumerable<IGrouping<string, TSource>> GroupByIgnoreCase<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
	{
		return source.GroupBy(keySelector, StringComparer.OrdinalIgnoreCase);
	}

	public static IOrderedEnumerable<string> OrderByHuman(this IEnumerable<string> source)
	{
		return source.OrderBy((string p) => p, HumanSortComparer.DefaultIgnoreCase);
	}

	public static IOrderedEnumerable<TSource> OrderByHuman<TSource>(this IEnumerable<TSource> source, Func<TSource, string?> keySelector)
	{
		return source.OrderBy(keySelector, HumanSortComparer.DefaultIgnoreCase);
	}

	public static bool IsMeaningful([NotNullWhen(true)] this ITokenString? str)
	{
		return !string.IsNullOrWhiteSpace(str?.Value);
	}

	public static IInvariantSet SplitValuesUnique(this ITokenString? tokenStr, Func<string, string>? normalize = null)
	{
		if ((!(tokenStr?.IsReady)) ?? false)
		{
			throw new InvalidOperationException("Can't get values from a non-ready token string (raw value: " + tokenStr.Raw + ").");
		}
		if (string.IsNullOrWhiteSpace(tokenStr?.Value))
		{
			return InvariantSets.Empty;
		}
		string[] array = tokenStr.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (normalize != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = normalize(array[i]);
			}
		}
		return InvariantSets.From(array);
	}

	public static string GetFullPath(this IContentPack contentPack, string relativePath)
	{
		return Path.Combine(contentPack.DirectoryPath, relativePath);
	}

	public static bool HasDependency(this IManifest manifest, string modId, bool canBeOptional = true)
	{
		ISemanticVersion minVersion;
		return manifest.HasDependency(modId, out minVersion, canBeOptional);
	}

	public static bool HasDependency(this IManifest? manifest, string modId, out ISemanticVersion? minVersion, bool canBeOptional = true)
	{
		minVersion = null;
		if (manifest == null)
		{
			return false;
		}
		if (StringExtensions.EqualsIgnoreCase(manifest.UniqueID, modId))
		{
			return true;
		}
		IManifestContentPackFor contentPackFor = manifest.ContentPackFor;
		if (contentPackFor != null && StringExtensions.EqualsIgnoreCase(contentPackFor.UniqueID, modId))
		{
			return true;
		}
		IManifestDependency val = manifest.Dependencies.FirstOrDefault((IManifestDependency p) => StringExtensions.EqualsIgnoreCase(p.UniqueID, modId));
		minVersion = ((val != null) ? val.MinimumVersion : null);
		return val != null && (canBeOptional || val.IsRequired);
	}
}
