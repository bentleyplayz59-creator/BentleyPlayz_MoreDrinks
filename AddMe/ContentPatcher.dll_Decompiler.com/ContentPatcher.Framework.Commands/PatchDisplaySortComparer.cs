using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Commands;

internal class PatchDisplaySortComparer : IComparer<PatchInfo>
{
	public int Compare(PatchInfo? left, PatchInfo? right)
	{
		if (left == right || left?.Path == null || right?.Path == null)
		{
			return 0;
		}
		string[] comparablePathSegments = GetComparablePathSegments(left);
		string[] comparablePathSegments2 = GetComparablePathSegments(right);
		for (int i = 0; i < comparablePathSegments.Length && i < comparablePathSegments2.Length; i++)
		{
			int num = CompareHuman(comparablePathSegments[i], comparablePathSegments2[i]);
			if (num != 0)
			{
				return num;
			}
		}
		return comparablePathSegments.Length.CompareTo(comparablePathSegments2.Length);
	}

	private string[] GetComparablePathSegments(PatchInfo patch)
	{
		LogPathBuilder logPathBuilder = patch.Path;
		if (patch.ParsedType == PatchType.Include)
		{
			string text = patch.ParsedFromAsset?.Value ?? patch.RawFromAsset;
			if (text != null)
			{
				logPathBuilder = new LogPathBuilder(logPathBuilder.Segments.Take(logPathBuilder.Segments.Length - 1)).With(PathUtilities.NormalizeAssetName(text));
			}
		}
		return logPathBuilder.Segments;
	}

	private int CompareHuman(string left, string right)
	{
		return HumanSortComparer.DefaultIgnoreCase.Compare(left, right);
	}
}
