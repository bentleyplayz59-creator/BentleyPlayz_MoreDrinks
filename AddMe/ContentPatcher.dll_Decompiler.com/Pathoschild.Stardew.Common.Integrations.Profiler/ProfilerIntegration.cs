using System;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.Profiler;

internal class ProfilerIntegration : BaseIntegration<IProfilerApi>
{
	public ProfilerIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Profiler", "SinZ.Profiler", "2.0.0", modRegistry, monitor)
	{
	}

	public IDisposable? RecordSection(string modId, string eventType, string details)
	{
		AssertLoaded();
		return SafelyCallApi((IProfilerApi api) => api.RecordSection(modId, eventType, details), "Failed recording section with {0}.");
	}
}
