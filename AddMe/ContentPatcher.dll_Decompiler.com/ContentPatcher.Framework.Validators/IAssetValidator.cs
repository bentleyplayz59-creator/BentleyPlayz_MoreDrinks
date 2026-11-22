using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Validators;

internal interface IAssetValidator
{
	bool TryValidate<T>(IAssetName assetName, T data, IPatch patch, [NotNullWhen(false)] out string? error);
}
