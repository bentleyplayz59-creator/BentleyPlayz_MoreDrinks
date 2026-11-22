using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley.GameData.Machines;

namespace Pathoschild.Stardew.Common.Integrations.ExtraMachineConfig;

internal class ExtraMachineConfigIntegration : BaseIntegration<IExtraMachineConfigApi>
{
	public ExtraMachineConfigIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("ExtraMachineConfig", "selph.ExtraMachineConfig", "1.4.0", modRegistry, monitor)
	{
	}

	public IList<(string, int)> GetExtraRequirements(string machineId, MachineItemOutput outputData)
	{
		return SafelyCallApi((IExtraMachineConfigApi api) => api.GetExtraRequirements(outputData), "Failed fetching extra requirements from {0}.") ?? new List<(string, int)>();
	}

	public IList<(string, int)> GetExtraTagsRequirements(string machineId, MachineItemOutput outputData)
	{
		return SafelyCallApi((IExtraMachineConfigApi api) => api.GetExtraTagsRequirements(outputData), "Failed fetching extra tag requirements from {0}.") ?? new List<(string, int)>();
	}

	public IList<MachineItemOutput> GetExtraOutputs(string machineId, MachineItemOutput outputData, MachineData machine)
	{
		return SafelyCallApi((IExtraMachineConfigApi api) => api.GetExtraOutputs(outputData, machine), "Failed fetching extra outputs from {0}.") ?? new List<MachineItemOutput>();
	}
}
