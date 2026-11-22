using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.UI;

internal class Dropdown<TItem> : ClickableComponent
{
	private readonly SpriteFont Font;

	private readonly DropdownList<TItem> List;

	private readonly int BorderWidth = CommonSprites.Tab.TopLeft.Width * 2 * 4;

	private readonly int? MaxLabelWidth;

	private string? DisplayLabel;

	private bool IsAndroid
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Invalid comparison between Unknown and I4
			return (int)Constants.TargetPlatform == 0;
		}
	}

	public bool IsExpanded
	{
		get;
		set
		{
			field = value;
			base.downNeighborID = (value ? List.TopComponentId : DefaultDownNeighborId);
		}
	}

	public TItem Selected => List.SelectedValue;

	public int DefaultDownNeighborId { get; set; } = -99999;

	public Dropdown(int x, int y, SpriteFont font, TItem? selectedItem, TItem[] items, Func<TItem, string> getLabel, int? maxLabelWidth = null)
		: base(Rectangle.Empty, (selectedItem != null) ? getLabel(selectedItem) : string.Empty)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Font = font;
		List = new DropdownList<TItem>(selectedItem, items, getLabel, x, y, font);
		base.bounds.X = x;
		base.bounds.Y = y;
		MaxLabelWidth = maxLabelWidth;
		ReinitializeComponents();
		OnValueSelected();
	}

	public override bool containsPoint(int x, int y)
	{
		return ((ClickableComponent)this).containsPoint(x, y) || (IsExpanded && ((ClickableComponent)List).containsPoint(x, y));
	}

	public bool TryClick(int x, int y)
	{
		bool itemClicked;
		bool dropdownToggled;
		return TryClick(x, y, out itemClicked, out dropdownToggled);
	}

	public bool TryClick(int x, int y, out bool itemClicked, out bool dropdownToggled)
	{
		itemClicked = false;
		dropdownToggled = false;
		if (IsExpanded && List.TryClick(x, y, out itemClicked))
		{
			if (itemClicked)
			{
				IsExpanded = false;
				dropdownToggled = true;
				OnValueSelected();
			}
			return true;
		}
		if (base.bounds.Contains(x, y) || IsExpanded)
		{
			IsExpanded = !IsExpanded;
			dropdownToggled = true;
			return true;
		}
		return false;
	}

	public bool TrySelect(TItem value)
	{
		return List.TrySelect(value);
	}

	public void ReceiveScrollWheelAction(int direction)
	{
		if (IsExpanded)
		{
			List.ReceiveScrollWheelAction(direction);
		}
	}

	public void Draw(SpriteBatch sprites, float opacity = 1f)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		CommonHelper.DrawTab(sprites, base.bounds.X, base.bounds.Y, base.bounds.Width, List.MaxLabelHeight, out var innerDrawPosition, 0, 1f, forIcon: false, IsAndroid);
		sprites.DrawString(Font, DisplayLabel, innerDrawPosition, Color.Black * opacity);
		if (IsExpanded)
		{
			List.Draw(sprites, opacity);
		}
	}

	public void ReinitializeComponents()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.bounds.Height = (int)Font.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZ").Y - 10 + BorderWidth;
		base.bounds.Width = List.MaxLabelWidth + BorderWidth;
		if (base.bounds.Width > MaxLabelWidth)
		{
			base.bounds.Width = MaxLabelWidth.Value;
		}
		((ClickableComponent)List).bounds.X = base.bounds.X;
		((ClickableComponent)List).bounds.Y = base.bounds.Bottom;
		List.ReinitializeComponents();
		ReinitializeControllerFlow();
	}

	public void ReinitializeControllerFlow()
	{
		List.ReinitializeControllerFlow();
		IsExpanded = IsExpanded;
	}

	public IEnumerable<ClickableComponent> GetChildComponents()
	{
		return List.GetChildComponents();
	}

	private void OnValueSelected()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		string text = List.SelectedLabel;
		if (MaxLabelWidth.HasValue && Font.MeasureString(text).X > (float?)MaxLabelWidth)
		{
			float x = Font.MeasureString("...").X;
			int num = MaxLabelWidth.Value - (int)x;
			bool flag = false;
			while (text.Length > 10 && Font.MeasureString(text).X > (float)num)
			{
				string text2 = text;
				text = text2.Substring(0, text2.Length - 1);
				flag = true;
			}
			if (flag)
			{
				text += "...";
			}
		}
		DisplayLabel = text;
	}
}
