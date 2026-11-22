using System;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations;

internal abstract class BaseIntegration : IModIntegration
{
	protected string ModId { get; }

	protected IModRegistry ModRegistry { get; }

	protected IMonitor Monitor { get; }

	public string Label { get; }

	public virtual bool IsLoaded { get; }

	protected BaseIntegration(string label, string modId, string minVersion, IModRegistry modRegistry, IMonitor monitor)
	{
		Label = label;
		ModId = modId;
		ModRegistry = modRegistry;
		Monitor = monitor;
		IModInfo val = modRegistry.Get(ModId);
		IManifest val2 = ((val != null) ? val.Manifest : null);
		if (val2 != null)
		{
			if (val2.Version.IsOlderThan(minVersion))
			{
				monitor.Log($"Detected {label} {val2.Version}, but need {minVersion} or later. Disabled integration with this mod.", (LogLevel)3);
			}
			else
			{
				IsLoaded = true;
			}
		}
	}

	protected TApi? GetValidatedApi<TApi>() where TApi : class
	{
		TApi api = ModRegistry.GetApi<TApi>(ModId);
		if (api == null)
		{
			Monitor.Log("Detected " + Label + ", but couldn't fetch its API. Disabled integration with this mod.", (LogLevel)3);
			return null;
		}
		return api;
	}

	protected virtual void AssertLoaded()
	{
		if (!IsLoaded)
		{
			throw new InvalidOperationException("The " + Label + " integration isn't loaded.");
		}
	}
}
internal abstract class BaseIntegration<TApi> : BaseIntegration where TApi : class
{
	public TApi? ModApi { get; }

	[MemberNotNullWhen(true, "ModApi")]
	public override bool IsLoaded
	{
		[MemberNotNullWhen(true, "ModApi")]
		get
		{
			return ModApi != null;
		}
	}

	protected BaseIntegration(string label, string modId, string minVersion, IModRegistry modRegistry, IMonitor monitor)
		: base(label, modId, minVersion, modRegistry, monitor)
	{
		if (base.IsLoaded)
		{
			ModApi = GetValidatedApi<TApi>();
		}
	}

	[MemberNotNull("ModApi")]
	protected override void AssertLoaded()
	{
		if (!IsLoaded)
		{
			throw new InvalidOperationException("The " + Label + " integration isn't loaded.");
		}
	}

	protected void SafelyCallApi(Action<TApi> callApi, string error)
	{
		if (IsLoaded)
		{
			try
			{
				callApi(ModApi);
			}
			catch (Exception value)
			{
				error = string.Format(error, Label);
				Monitor.LogOnce($"{error}\n\nTechnical info:\n{value}", (LogLevel)4);
			}
		}
	}

	[return: NotNullIfNotNull("defaultValue")]
	protected TReturn? SafelyCallApi<TReturn>(Func<TApi, TReturn> callApi, string error, TReturn? defaultValue = default(TReturn?))
	{
		if (IsLoaded)
		{
			try
			{
				return callApi(ModApi);
			}
			catch (Exception value)
			{
				error = string.Format(error, Label);
				Monitor.LogOnce($"{error}\n\nTechnical info:\n{value}", (LogLevel)4);
			}
		}
		return defaultValue;
	}
}
