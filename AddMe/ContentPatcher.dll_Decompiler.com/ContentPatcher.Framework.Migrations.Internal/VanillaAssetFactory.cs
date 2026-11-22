using System;
using System.Diagnostics.CodeAnalysis;
using Force.DeepCloner;
using Microsoft.Xna.Framework.Content;
using StardewValley;

namespace ContentPatcher.Framework.Migrations.Internal;

internal class VanillaAssetFactory<T> where T : class
{
	private readonly Lazy<T> Data;

	public VanillaAssetFactory(Func<LocalizedContentManager, T> load)
	{
		Data = new Lazy<T>(() => LoadVanillaData(load));
	}

	[return: NotNull]
	public T GetFreshCopy()
	{
		return DeepClonerExtensions.DeepClone<T>(Data.Value);
	}

	public static T LoadVanillaData(Func<LocalizedContentManager, T> load)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		LocalizedContentManager val = new LocalizedContentManager(((ContentManager)Game1.content).ServiceProvider, ((ContentManager)Game1.content).RootDirectory);
		try
		{
			return load(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
