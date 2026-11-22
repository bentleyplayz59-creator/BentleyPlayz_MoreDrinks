using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Patches;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands;

internal class DumpCommand : BaseCommand
{
	private readonly Func<PatchManager> GetPatchManager;

	public DumpCommand(IMonitor monitor, Func<PatchManager> getPatchManager)
		: base(monitor, "dump")
	{
		GetPatchManager = getPatchManager;
	}

	public override string GetDescription()
	{
		return "patch dump\n   Usage: patch dump applied\n   Lists active patches grouped by their target value.\n\n   Usage: patch dump order\n   Lists every loaded patch in their apply order. When two patches edit the same asset, they'll be applied in the apply order.";
	}

	public override void Handle(string[] args)
	{
		PatchManager patchManager = GetPatchManager();
		if (args.Length != 1)
		{
			Monitor.Log("The 'patch dump' command requires an argument which indicates what to dump. See 'patch help dump' for more info.", (LogLevel)4);
			return;
		}
		string text = args[0].ToLower();
		string text2 = text;
		if (!(text2 == "applied"))
		{
			if (text2 == "order")
			{
				var array = patchManager.GetPatches().Select((IPatch patch, int globalIndex) => new
				{
					globalPosition = (globalIndex + 1).ToString(CultureInfo.InvariantCulture),
					indexPath = string.Join(" > ", patch.IndexPath),
					name = patch.Path.ToString()
				}).ToArray();
				int totalWidth = Math.Max("order".Length, array.Max(p => p.globalPosition.Length));
				int totalWidth2 = Math.Max("index path".Length, array.Max(p => p.indexPath.Length));
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Here's the global patch definition order across all loaded content packs, which affects the order that patches are applied. The 'order' column is the patch's global position in the order; the 'index path' column is Content Patcher's internal hierarchical definition order.");
				stringBuilder.AppendLine();
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(14, 2, stringBuilder2);
				handler.AppendLiteral("   ");
				handler.AppendFormatted("order".PadRight(totalWidth, ' '));
				handler.AppendLiteral("   ");
				handler.AppendFormatted("index path".PadRight(totalWidth2, ' '));
				handler.AppendLiteral("   patch");
				stringBuilder3.AppendLine(ref handler);
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder4 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(14, 2, stringBuilder2);
				handler.AppendLiteral("   ");
				handler.AppendFormatted("".PadRight(totalWidth, '-'));
				handler.AppendLiteral("   ");
				handler.AppendFormatted("".PadRight(totalWidth2, '-'));
				handler.AppendLiteral("   -----");
				stringBuilder4.AppendLine(ref handler);
				var array2 = array;
				foreach (var anon in array2)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder5 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(9, 3, stringBuilder2);
					handler.AppendLiteral("   ");
					handler.AppendFormatted(anon.globalPosition.PadRight(totalWidth, ' '));
					handler.AppendLiteral("   ");
					handler.AppendFormatted(anon.indexPath.PadRight(totalWidth2, ' '));
					handler.AppendLiteral("   ");
					handler.AppendFormatted(anon.name);
					stringBuilder5.AppendLine(ref handler);
				}
				Monitor.Log(stringBuilder.ToString(), (LogLevel)2);
			}
			else
			{
				Monitor.Log("Invalid 'patch dump' argument. See 'patch help dump' for more info.", (LogLevel)4);
			}
			return;
		}
		StringBuilder stringBuilder6 = new StringBuilder();
		stringBuilder6.AppendLine("Here are the active patches grouped by their current target value. Within each group, patches are listed in the expected apply order and the checkbox indicates whether each patch is currently applied. See `patch summary` for more info about each patch, including reasons it may not be applied.");
		foreach (var (val2, source) in patchManager.GetPatchesByTarget().OrderByHuman((KeyValuePair<IAssetName, IEnumerable<IPatch>> p) => p.Key.Name))
		{
			stringBuilder6.AppendLine();
			stringBuilder6.AppendLine(val2.Name);
			stringBuilder6.AppendLine("".PadRight(val2.Name.Length, '-'));
			IOrderedEnumerable<IPatch> orderedEnumerable = source.OrderByDescending((IPatch p) => p.Type == PatchType.Load).ThenBy((IPatch p) => p, PatchIndexComparer.Instance);
			foreach (IPatch item in orderedEnumerable)
			{
				StringBuilder stringBuilder2 = stringBuilder6;
				StringBuilder stringBuilder7 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(7, 3, stringBuilder2);
				handler.AppendLiteral("   [");
				handler.AppendFormatted(item.IsApplied ? "X" : " ");
				handler.AppendLiteral("] ");
				handler.AppendFormatted(item.Type);
				handler.AppendLiteral(" ");
				handler.AppendFormatted(item.Path);
				stringBuilder7.AppendLine(ref handler);
			}
		}
		Monitor.Log(stringBuilder6.ToString(), (LogLevel)2);
	}
}
