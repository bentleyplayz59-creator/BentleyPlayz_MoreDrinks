using System;

namespace Pathoschild.Stardew.Common.Integrations.IconicFramework;

public interface IIconicFrameworkApi
{
	void AddToolbarIcon(string texturePath, Nullable sourceRect, Func<string>? getTitle, Func<string>? getDescription, Action onClick, Action? onRightClick = null);
}
