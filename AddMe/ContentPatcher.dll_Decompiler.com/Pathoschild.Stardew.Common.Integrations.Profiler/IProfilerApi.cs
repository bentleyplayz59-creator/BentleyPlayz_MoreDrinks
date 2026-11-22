using System;

namespace Pathoschild.Stardew.Common.Integrations.Profiler;

public interface IProfilerApi
{
	IDisposable RecordSection(string modId, string eventType, string details);
}
