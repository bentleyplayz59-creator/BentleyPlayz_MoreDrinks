using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile.Dimensions;

namespace Pathoschild.Stardew.Common.UI;

internal abstract class BaseOverlay : IDisposable
{
	private readonly IModEvents Events;

	protected readonly IInputHelper InputHelper;

	protected readonly IReflectionHelper Reflection;

	private readonly int ScreenId;

	private Rectangle LastViewport;

	private readonly Func<bool>? KeepAliveCheck;

	private readonly bool? AssumeUiMode;

	public virtual void Dispose()
	{
		Events.Display.RenderedActiveMenu -= OnRendered;
		Events.Display.RenderedWorld -= OnRenderedWorld;
		Events.GameLoop.UpdateTicked -= OnUpdateTicked;
		Events.Input.ButtonPressed -= OnButtonPressed;
		Events.Input.ButtonsChanged -= OnButtonsChanged;
		Events.Input.CursorMoved -= OnCursorMoved;
		Events.Input.MouseWheelScrolled -= OnMouseWheelScrolled;
	}

	protected BaseOverlay(IModEvents events, IInputHelper inputHelper, IReflectionHelper reflection, Func<bool>? keepAlive = null, bool? assumeUiMode = null)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Events = events;
		InputHelper = inputHelper;
		Reflection = reflection;
		KeepAliveCheck = keepAlive;
		LastViewport = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
		ScreenId = Context.ScreenId;
		AssumeUiMode = assumeUiMode;
		events.GameLoop.UpdateTicked += OnUpdateTicked;
		if (IsMethodOverridden("DrawUi"))
		{
			events.Display.RenderedActiveMenu += OnRendered;
		}
		if (IsMethodOverridden("DrawWorld"))
		{
			events.Display.RenderedWorld += OnRenderedWorld;
		}
		if (IsMethodOverridden("ReceiveLeftClick"))
		{
			events.Input.ButtonPressed += OnButtonPressed;
		}
		if (IsMethodOverridden("ReceiveButtonsChanged"))
		{
			events.Input.ButtonsChanged += OnButtonsChanged;
		}
		if (IsMethodOverridden("ReceiveCursorHover"))
		{
			events.Input.CursorMoved += OnCursorMoved;
		}
		if (IsMethodOverridden("ReceiveScrollWheelAction"))
		{
			events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
		}
	}

	protected virtual void Update()
	{
	}

	protected virtual void DrawUi(SpriteBatch batch)
	{
	}

	protected virtual void DrawWorld(SpriteBatch batch)
	{
	}

	protected virtual bool ReceiveLeftClick(int x, int y)
	{
		return false;
	}

	protected virtual void ReceiveButtonsChanged(object? sender, ButtonsChangedEventArgs e)
	{
	}

	protected virtual bool ReceiveScrollWheelAction(int amount)
	{
		return false;
	}

	protected virtual bool ReceiveCursorHover(int x, int y)
	{
		return false;
	}

	protected virtual void ReceiveGameWindowResized()
	{
	}

	protected void DrawCursor()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (!Game1.options.hardwareCursor)
		{
			Vector2 val = new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY());
			if ((int)Constants.TargetPlatform == 0)
			{
				val *= Game1.options.zoomLevel / Reflection.GetProperty<float>(typeof(Game1), "NativeZoomLevel", true).GetValue();
			}
			Game1.spriteBatch.Draw(Game1.mouseCursors, val, (Nullable)Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.SnappyMenus ? 44 : 0, 16, 16), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, (SpriteEffects)0, 1f);
		}
	}

	private void OnRendered(object? sender, RenderedActiveMenuEventArgs e)
	{
		if (Context.ScreenId == ScreenId)
		{
			DrawUi(Game1.spriteBatch);
		}
	}

	private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
	{
		if (Context.ScreenId == ScreenId)
		{
			DrawWorld(e.SpriteBatch);
		}
	}

	private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (Context.ScreenId != ScreenId)
		{
			if (!Context.HasScreenId(ScreenId))
			{
				Dispose();
			}
			return;
		}
		if (KeepAliveCheck != null && !KeepAliveCheck())
		{
			Dispose();
			return;
		}
		Rectangle lastViewport = Game1.uiViewport;
		if (LastViewport.Width != lastViewport.Width || LastViewport.Height != lastViewport.Height)
		{
			lastViewport = new Rectangle(lastViewport.X, lastViewport.Y, lastViewport.Width, lastViewport.Height);
			ReceiveGameWindowResized();
			LastViewport = lastViewport;
		}
		Update();
	}

	private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
	{
		if (Context.ScreenId == ScreenId)
		{
			ReceiveButtonsChanged(sender, e);
		}
	}

	private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (Context.ScreenId == ScreenId && ((int)e.Button == 1000 || SButtonExtensions.IsUseToolButton(e.Button)))
		{
			bool flag = AssumeUiMode ?? Game1.uiMode;
			bool flag2;
			if ((int)Constants.TargetPlatform == 0)
			{
				float value = Reflection.GetProperty<float>(typeof(Game1), "NativeZoomLevel", true).GetValue();
				flag2 = ReceiveLeftClick((int)((float)Game1.getMouseX() * Game1.options.zoomLevel / value), (int)((float)Game1.getMouseY() * Game1.options.zoomLevel / value));
			}
			else
			{
				flag2 = ReceiveLeftClick(Game1.getMouseX(flag), Game1.getMouseY(flag));
			}
			if (flag2)
			{
				InputHelper.Suppress(e.Button);
			}
		}
	}

	private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (Context.ScreenId == ScreenId && ReceiveScrollWheelAction(e.Delta))
		{
			MouseState oldMouseState = Game1.oldMouseState;
			Game1.oldMouseState = new MouseState(oldMouseState.X, oldMouseState.Y, e.NewValue, oldMouseState.LeftButton, oldMouseState.MiddleButton, oldMouseState.RightButton, oldMouseState.XButton1, oldMouseState.XButton2);
		}
	}

	private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
	{
		if (Context.ScreenId == ScreenId)
		{
			bool flag = AssumeUiMode ?? Game1.uiMode;
			if (ReceiveCursorHover(Game1.getMouseX(flag), Game1.getMouseY(flag)))
			{
				Game1.InvalidateOldMouseMovement();
			}
		}
	}

	private bool IsMethodOverridden(string name)
	{
		MethodInfo method = GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			throw new InvalidOperationException($"Can't find method {GetType().FullName}.{name}.");
		}
		return method.DeclaringType != typeof(BaseOverlay);
	}
}
