using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.Common.UI;

internal class DropdownList<TValue> : ClickableComponent
{
	private class DropListOption : ClickableComponent
	{
		public int Index { get; }

		public TValue Value { get; }

		public int LabelWidth { get; }

		public int LabelHeight { get; }

		public DropListOption(Rectangle bounds, int index, string label, TValue value, SpriteFont font)
			: base(bounds, index.ToString(), label)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			Index = index;
			Value = value;
			Vector2 val = font.MeasureString(label);
			LabelWidth = (int)val.X;
			LabelHeight = (int)val.Y;
		}
	}

	private const int DropdownPadding = 5;

	private DropListOption SelectedOption;

	private readonly DropListOption[] Options;

	private int FirstVisibleIndex;

	private int MaxItems;

	private readonly SpriteFont Font;

	private ClickableTextureComponent UpArrow;

	private ClickableTextureComponent DownArrow;

	private int LastVisibleIndex => FirstVisibleIndex + MaxItems - 1;

	private int MaxFirstVisibleIndex => Options.Length - MaxItems;

	private bool CanScrollUp => FirstVisibleIndex > 0;

	private bool CanScrollDown => FirstVisibleIndex < MaxFirstVisibleIndex;

	public TValue SelectedValue => SelectedOption.Value;

	public string SelectedLabel => ((ClickableComponent)SelectedOption).label;

	public int MaxLabelHeight { get; }

	public int MaxLabelWidth { get; private set; }

	public int TopComponentId => ((ClickableComponent)Options.First((DropListOption p) => ((ClickableComponent)p).visible)).myID;

	public DropdownList(TValue? selectedValue, TValue[] items, Func<TValue, string> getLabel, int x, int y, SpriteFont font)
		: base(default(Rectangle), "DropdownList")
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Options = items.Select((TValue item, int index) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return new DropListOption(Rectangle.Empty, index, getLabel(item), item, font);
		}).ToArray();
		Font = font;
		MaxLabelHeight = Options.Max((DropListOption p) => p.LabelHeight);
		int num = Array.IndexOf(items, selectedValue);
		SelectedOption = ((num >= 0) ? Options[num] : Options.First());
		base.bounds.X = x;
		base.bounds.Y = y;
		ReinitializeComponents();
	}

	public void ReceiveScrollWheelAction(int direction)
	{
		Scroll((direction <= 0) ? 1 : (-1));
	}

	public bool TryClick(int x, int y, out bool itemClicked)
	{
		DropListOption dropListOption = Options.FirstOrDefault((DropListOption p) => ((ClickableComponent)p).visible && ((ClickableComponent)p).containsPoint(x, y));
		if (dropListOption != null)
		{
			SelectedOption = dropListOption;
			itemClicked = true;
			return true;
		}
		itemClicked = false;
		if (((ClickableComponent)UpArrow).containsPoint(x, y))
		{
			Scroll(-1);
			return true;
		}
		if (((ClickableComponent)DownArrow).containsPoint(x, y))
		{
			Scroll(1);
			return true;
		}
		return false;
	}

	public bool TrySelect(TValue value)
	{
		DropListOption dropListOption = Options.FirstOrDefault((DropListOption p) =>
		{
			int result;
			if (p.Value != null || value != null)
			{
				TValue value2 = p.Value;
				if (value2 == null)
				{
					result = 0;
				}
				else
				{
					object obj = value;
					result = (value2.Equals(obj) ? 1 : 0);
				}
			}
			else
			{
				result = 1;
			}
			return (byte)result != 0;
		});
		if (dropListOption == null)
		{
			return false;
		}
		SelectedOption = dropListOption;
		return true;
	}

	public override bool containsPoint(int x, int y)
	{
		return ((ClickableComponent)this).containsPoint(x, y) || ((ClickableComponent)UpArrow).containsPoint(x, y) || ((ClickableComponent)DownArrow).containsPoint(x, y);
	}

	public void Draw(SpriteBatch sprites, float opacity = 1f)
	{
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		DropListOption[] options = Options;
		foreach (DropListOption dropListOption in options)
		{
			if (((ClickableComponent)dropListOption).visible)
			{
				if (((ClickableComponent)dropListOption).containsPoint(Game1.getMouseX(), Game1.getMouseY()))
				{
					sprites.Draw(CommonSprites.DropDown.Sheet, ((ClickableComponent)dropListOption).bounds, (Nullable)CommonSprites.DropDown.HoverBackground, Color.White * opacity);
				}
				else if (dropListOption.Index == SelectedOption.Index)
				{
					sprites.Draw(CommonSprites.DropDown.Sheet, ((ClickableComponent)dropListOption).bounds, (Nullable)CommonSprites.DropDown.ActiveBackground, Color.White * opacity);
				}
				else
				{
					sprites.Draw(CommonSprites.DropDown.Sheet, ((ClickableComponent)dropListOption).bounds, (Nullable)CommonSprites.DropDown.InactiveBackground, Color.White * opacity);
				}
				Vector2 val = new Vector2((float)(((ClickableComponent)dropListOption).bounds.X + 5), (float)(((ClickableComponent)dropListOption).bounds.Y + 4));
				sprites.DrawString(Font, ((ClickableComponent)dropListOption).label, val, Color.Black * opacity);
			}
		}
		if (CanScrollUp)
		{
			UpArrow.draw(sprites, Color.White * opacity, 1f, 0, 0, 0);
		}
		if (CanScrollDown)
		{
			DownArrow.draw(sprites, Color.White * opacity, 1f, 0, 0, 0);
		}
	}

	[MemberNotNull(new string[] { "UpArrow", "DownArrow" })]
	public void ReinitializeComponents()
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected Obj, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected Obj, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		int x = base.bounds.X;
		int y = base.bounds.Y;
		int num = (MaxLabelWidth = Math.Max(Options.Max((DropListOption p) => p.LabelWidth), 128) + 10);
		int num3 = num;
		int maxLabelHeight = MaxLabelHeight;
		MaxItems = Math.Min((Game1.uiViewport.Height - y) / maxLabelHeight, Options.Length);
		FirstVisibleIndex = GetValidFirstItem(FirstVisibleIndex, MaxFirstVisibleIndex);
		base.bounds.Width = num3;
		base.bounds.Height = maxLabelHeight * MaxItems;
		int num4 = y;
		DropListOption[] options = Options;
		foreach (DropListOption dropListOption in options)
		{
			((ClickableComponent)dropListOption).visible = dropListOption.Index >= FirstVisibleIndex && dropListOption.Index <= LastVisibleIndex;
			if (((ClickableComponent)dropListOption).visible)
			{
				((ClickableComponent)dropListOption).bounds = new Rectangle(x, num4, num3, maxLabelHeight);
				num4 += maxLabelHeight;
			}
		}
		Rectangle upArrow = CommonSprites.Icons.UpArrow;
		Rectangle downArrow = CommonSprites.Icons.DownArrow;
		UpArrow = new ClickableTextureComponent("up-arrow", new Rectangle(x - upArrow.Width, y, upArrow.Width, upArrow.Height), "", "", CommonSprites.Icons.Sheet, upArrow, 1f, false);
		DownArrow = new ClickableTextureComponent("down-arrow", new Rectangle(x - downArrow.Width, y + base.bounds.Height - downArrow.Height, downArrow.Width, downArrow.Height), "", "", CommonSprites.Icons.Sheet, downArrow, 1f, false);
		ReinitializeControllerFlow();
	}

	public void ReinitializeControllerFlow()
	{
		int firstVisibleIndex = FirstVisibleIndex;
		int lastVisibleIndex = LastVisibleIndex;
		int num = 1100000;
		DropListOption[] options = Options;
		foreach (DropListOption dropListOption in options)
		{
			int index = dropListOption.Index;
			int num2 = (((ClickableComponent)dropListOption).myID = num + index);
			((ClickableComponent)dropListOption).upNeighborID = ((index > firstVisibleIndex) ? (num2 - 1) : (-99999));
			((ClickableComponent)dropListOption).downNeighborID = ((index < lastVisibleIndex) ? (num2 + 1) : (-1));
		}
	}

	public IEnumerable<ClickableComponent> GetChildComponents()
	{
		return (IEnumerable<ClickableComponent>)(object)Options;
	}

	private void Scroll(int amount)
	{
		int validFirstItem = GetValidFirstItem(FirstVisibleIndex + amount, MaxFirstVisibleIndex);
		if (validFirstItem != FirstVisibleIndex)
		{
			FirstVisibleIndex = validFirstItem;
			ReinitializeComponents();
		}
	}

	private int GetValidFirstItem(int value, int maxIndex)
	{
		return Math.Max(Math.Min(value, maxIndex), 0);
	}
}
