using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Messages;

internal class AutomateUpdateChestMessage
{
	public string? LocationName { get; set; }

	public Vector2 Tile
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}
}
