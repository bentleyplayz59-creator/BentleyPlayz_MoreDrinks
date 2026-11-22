using StardewValley.Characters;

namespace Pathoschild.Stardew.Common.Integrations.HaveMoreKids;

public interface IHaveMoreKidsAPI
{
	int GetDaysToNextChildGrowth(Child kid);

	string GetChildBirthdayString(Child kid);
}
