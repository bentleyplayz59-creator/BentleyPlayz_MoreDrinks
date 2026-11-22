using System;
using System.Collections.Generic;

namespace ContentPatcher.Framework.Patches;

internal class PatchIndexComparer : IComparer<IPatch?>
{
	public static readonly PatchIndexComparer Instance = new PatchIndexComparer();

	public int Compare(IPatch? leftPatch, IPatch? rightPatch)
	{
		int[] array = leftPatch?.IndexPath;
		int[] array2 = rightPatch?.IndexPath;
		if (array == null || array2 == null)
		{
			return (array == null).CompareTo(array2 == null);
		}
		int num = Math.Min(array.Length, array2.Length);
		for (int i = 0; i < num; i++)
		{
			int num2 = array[i].CompareTo(array2[i]);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return array.Length.CompareTo(array2.Length);
	}
}
