using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Patches;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Validators;

internal abstract class BaseValidator : IAssetValidator
{
	public virtual bool TryValidate<T>(IAssetName assetName, T data, IPatch patch, [NotNullWhen(false)] out string? error)
	{
		error = null;
		return true;
	}
}
