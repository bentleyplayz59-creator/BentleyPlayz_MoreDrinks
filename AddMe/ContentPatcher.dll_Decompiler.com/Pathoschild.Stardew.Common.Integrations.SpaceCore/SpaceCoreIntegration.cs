using System;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.SpaceCore;

internal class SpaceCoreIntegration : BaseIntegration<ISpaceCoreApi>
{
	public SpaceCoreIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("SpaceCore", "spacechase0.SpaceCore", "1.25.0", modRegistry, monitor)
	{
	}

	public string[] GetCustomSkills()
	{
		return SafelyCallApi((ISpaceCoreApi api) => api.GetCustomSkills(), "getting custom skills") ?? Array.Empty<string>();
	}

	public int GetExperienceForCustomSkill(Farmer farmer, string skill)
	{
		return SafelyCallApi((ISpaceCoreApi api) => api.GetExperienceForCustomSkill(farmer, skill), "getting skill XP for skill '" + skill + "'", 0);
	}

	public string GetDisplayNameOfCustomSkill(string skill)
	{
		return SafelyCallApi((ISpaceCoreApi api) => api.GetDisplayNameOfCustomSkill(skill), "getting display name for skill '" + skill + "'") ?? skill;
	}
}
