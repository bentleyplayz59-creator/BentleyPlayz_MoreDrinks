using System;

namespace ContentPatcher.Framework.Conditions;

[Flags]
internal enum UpdateRate
{
	OnDayStart = 1,
	OnLocationChange = 2,
	OnTimeChange = 4
}
