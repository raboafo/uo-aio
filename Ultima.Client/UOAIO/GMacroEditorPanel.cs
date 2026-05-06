using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GMacroEditorPanel : GAlphaBackground
{
	private static string[] m_Aliases;

	private GSystemButton m_Ctrl;

	private GSystemButton m_Alt;

	private GSystemButton m_Shift;

	private Macro m_Macro;

	public Macro Macro => this.m_Macro;

	public static string GetKeyName(Keys key)
	{
		switch (key)
		{
		case (Keys)69632:
			return "Wheel Up";
		case (Keys)69633:
			return "Wheel Down";
		case (Keys)69634:
			return "Wheel Press";
		default:
		{
			if (GMacroEditorPanel.m_Aliases == null)
			{
				GMacroEditorPanel.LoadAliases();
			}
			int num = (int)key;
			if (num >= 0 && num < GMacroEditorPanel.m_Aliases.Length && GMacroEditorPanel.m_Aliases[num] != null)
			{
				return GMacroEditorPanel.m_Aliases[num];
			}
			return key.ToString();
		}
		}
	}

	private static void LoadAliases()
	{
		GMacroEditorPanel.m_Aliases = new string[256];
		GMacroEditorPanel.SetAlias(Keys.Add, "Num +");
		GMacroEditorPanel.SetAlias(Keys.Back, "Backspace");
		GMacroEditorPanel.SetAlias(Keys.Capital, "Caps Lock");
		GMacroEditorPanel.SetAlias(Keys.ControlKey, "Control");
		GMacroEditorPanel.SetAlias(Keys.D0, "0");
		GMacroEditorPanel.SetAlias(Keys.D1, "1");
		GMacroEditorPanel.SetAlias(Keys.D2, "2");
		GMacroEditorPanel.SetAlias(Keys.D3, "3");
		GMacroEditorPanel.SetAlias(Keys.D4, "4");
		GMacroEditorPanel.SetAlias(Keys.D5, "5");
		GMacroEditorPanel.SetAlias(Keys.D6, "6");
		GMacroEditorPanel.SetAlias(Keys.D7, "7");
		GMacroEditorPanel.SetAlias(Keys.D8, "8");
		GMacroEditorPanel.SetAlias(Keys.D9, "9");
		GMacroEditorPanel.SetAlias(Keys.Decimal, "Num .");
		GMacroEditorPanel.SetAlias(Keys.Divide, "Num /");
		GMacroEditorPanel.SetAlias(Keys.Menu, "Alt");
		GMacroEditorPanel.SetAlias(Keys.Multiply, "Num *");
		GMacroEditorPanel.SetAlias(Keys.NumLock, "Num Lock");
		GMacroEditorPanel.SetAlias(Keys.NumPad0, "Num 0");
		GMacroEditorPanel.SetAlias(Keys.NumPad1, "Num 1");
		GMacroEditorPanel.SetAlias(Keys.NumPad2, "Num 2");
		GMacroEditorPanel.SetAlias(Keys.NumPad3, "Num 3");
		GMacroEditorPanel.SetAlias(Keys.NumPad4, "Num 4");
		GMacroEditorPanel.SetAlias(Keys.NumPad5, "Num 5");
		GMacroEditorPanel.SetAlias(Keys.NumPad6, "Num 6");
		GMacroEditorPanel.SetAlias(Keys.NumPad7, "Num 7");
		GMacroEditorPanel.SetAlias(Keys.NumPad8, "Num 8");
		GMacroEditorPanel.SetAlias(Keys.NumPad9, "Num 9");
		GMacroEditorPanel.SetAlias(Keys.OemClear, "Clear");
		GMacroEditorPanel.SetAlias(Keys.OemCloseBrackets, "]");
		GMacroEditorPanel.SetAlias(Keys.Oemcomma, ",");
		GMacroEditorPanel.SetAlias(Keys.OemMinus, "-");
		GMacroEditorPanel.SetAlias(Keys.OemOpenBrackets, "[");
		GMacroEditorPanel.SetAlias(Keys.OemPeriod, ".");
		GMacroEditorPanel.SetAlias(Keys.OemPipe, "\\");
		GMacroEditorPanel.SetAlias(Keys.OemBackslash, "\\");
		GMacroEditorPanel.SetAlias(Keys.Oemplus, "+");
		GMacroEditorPanel.SetAlias(Keys.OemQuestion, "?");
		GMacroEditorPanel.SetAlias(Keys.OemQuotes, "'");
		GMacroEditorPanel.SetAlias(Keys.OemSemicolon, ";");
		GMacroEditorPanel.SetAlias(Keys.Oemtilde, "~");
		GMacroEditorPanel.SetAlias(Keys.Next, "Page Down");
		GMacroEditorPanel.SetAlias(Keys.Next, "Page Down");
		GMacroEditorPanel.SetAlias(Keys.Prior, "Page Up");
		GMacroEditorPanel.SetAlias(Keys.Prior, "Page Up");
		GMacroEditorPanel.SetAlias(Keys.Snapshot, "Print Screen");
		GMacroEditorPanel.SetAlias(Keys.Scroll, "Scroll Lock");
		GMacroEditorPanel.SetAlias(Keys.ShiftKey, "Shift");
		GMacroEditorPanel.SetAlias(Keys.Subtract, "Num -");
	}

	private static void SetAlias(Keys key, string alias)
	{
		GMacroEditorPanel.m_Aliases[(int)key] = alias;
	}

	public void NotifyParent()
	{
		if (base.m_Parent.Parent is GMacroEditorForm gMacroEditorForm)
		{
			gMacroEditorForm.UpdateKeyboard();
		}
	}

	public void UpdateModifiers()
	{
		this.UpdateModifier(this.m_Ctrl, "Ctrl", this.m_Macro.Control);
		this.UpdateModifier(this.m_Alt, "Alt", this.m_Macro.Alt);
		this.UpdateModifier(this.m_Shift, "Shift", this.m_Macro.Shift);
	}

	protected internal override void Draw(int X, int Y)
	{
	}

	private void UpdateModifier(GSystemButton btn, string prefix, bool opt)
	{
		if (opt)
		{
			btn.SetBackColor(GumpPaint.Blend(Color.SteelBlue, SystemColors.Control, 0.5f));
			return;
		}
		btn.SetBackColor(GumpPaint.Blend(Color.SteelBlue, SystemColors.Control, 0.25f));
		btn.InactiveColor = GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f);
	}

	private void Ctrl_OnClick(Gump g)
	{
		this.m_Macro.Control = !this.m_Macro.Control;
		this.UpdateModifier(this.m_Ctrl, "Ctrl", this.m_Macro.Control);
		this.NotifyParent();
	}

	private void Alt_OnClick(Gump g)
	{
		this.m_Macro.Alt = !this.m_Macro.Alt;
		this.UpdateModifier(this.m_Alt, "Alt", this.m_Macro.Alt);
		this.NotifyParent();
	}

	private void Shift_OnClick(Gump g)
	{
		this.m_Macro.Shift = !this.m_Macro.Shift;
		this.UpdateModifier(this.m_Shift, "Shift", this.m_Macro.Shift);
		this.NotifyParent();
	}

	private void Delete_OnClick(Gump g)
	{
		if (Macros.List.Contains(this.m_Macro))
		{
			Macros.List.Remove(this.m_Macro);
		}
		if (base.m_Parent.Parent is GMacroEditorForm gMacroEditorForm)
		{
			gMacroEditorForm.Current = null;
			gMacroEditorForm.UpdateKeyboard();
		}
	}

	public GMacroEditorPanel(Macro m)
		: base(0, 0, 259, 230)
	{
		this.m_Macro = m;
		base.m_CanDrag = false;
		base.m_NonRestrictivePicking = true;
		base.ShouldHitTest = false;
		this.m_Ctrl = new GSystemButton(10, 10, 40, 20, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), SystemColors.ControlText, "Ctrl", Engine.GetUniFont(2));
		this.m_Alt = new GSystemButton(49, 10, 40, 20, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), SystemColors.ControlText, "Alt", Engine.GetUniFont(2));
		this.m_Shift = new GSystemButton(88, 10, 42, 20, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), SystemColors.ControlText, "Shift", Engine.GetUniFont(2));
		this.m_Ctrl.OnClick = Ctrl_OnClick;
		this.m_Alt.OnClick = Alt_OnClick;
		this.m_Shift.OnClick = Shift_OnClick;
		this.m_Ctrl.Tooltip = new Tooltip("Toggles the control key modifier", Big: true);
		this.m_Alt.Tooltip = new Tooltip("Toggles the alt key modifier", Big: true);
		this.m_Shift.Tooltip = new Tooltip("Toggles the shift key modifier", Big: true);
		base.m_Children.Add(this.m_Ctrl);
		base.m_Children.Add(this.m_Alt);
		base.m_Children.Add(this.m_Shift);
		this.UpdateModifiers();
		GAlphaBackground toAdd = new GAlphaBackground(129, 10, 74, 20)
		{
			FillAlpha = 1f,
			FillColor = GumpColors.Window
		};
		base.m_Children.Add(toAdd);
		GMacroKeyEntry toAdd2 = new GMacroKeyEntry(GMacroEditorPanel.GetKeyName(m.Key), 129, 10, 74, 20)
		{
			Tooltip = new Tooltip("Press any key here to change the macro", Big: true)
		};
		base.m_Children.Add(toAdd2);
		GSystemButton gSystemButton = new GSystemButton(10, 10, 40, 20, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), SystemColors.ControlText, "Delete", Engine.GetUniFont(2));
		gSystemButton.SetBackColor(GumpPaint.Blend(Color.SteelBlue, SystemColors.Control, 0.25f));
		gSystemButton.InactiveColor = GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f);
		gSystemButton.Tooltip = new Tooltip("Deletes the entire macro", Big: true);
		gSystemButton.OnClick = Delete_OnClick;
		gSystemButton.X = this.Width - 10 - gSystemButton.Width;
		base.m_Children.Add(gSystemButton);
		base.FillAlpha = 0.15f;
		GMainMenu gMainMenu;
		GMenuItem mi;
		for (int i = 0; i < m.Actions.Count; i++)
		{
			try
			{
				Action action = m.Actions[i];
				if (action.Handler == null)
				{
					continue;
				}
				ActionHandler handler = action.Handler;
				gMainMenu = new GMainMenu(10, 35 + i * 23);
				mi = new GActionMenu(this, m, action);
				gMainMenu.Add(this.FormatMenu(mi));
				if (handler.Params == null)
				{
					GAlphaBackground gAlphaBackground = new GAlphaBackground(129, 35 + i * 23, 120, 24)
					{
						FillAlpha = 1f,
						FillColor = GumpColors.Window
					};
					base.m_Children.Add(gAlphaBackground);
					IHue windowText = GumpHues.WindowText;
					GTextBox toAdd3 = new GMacroParamEntry(action, action.Param, gAlphaBackground.X + 4, gAlphaBackground.Y, gAlphaBackground.Width - 4, gAlphaBackground.Height)
					{
						MaxChars = 239
					};
					base.m_Children.Add(toAdd3);
				}
				else if (handler.Params.Length != 0)
				{
					string text = GMacroEditorPanel.Find(action.Param, handler.Params);
					if (text == null)
					{
						text = action.Param;
					}
					mi = this.GetMenuFrom(new ParamNode(text, handler.Params), action, handler);
					mi.DropDown = i == m.Actions.Count - 1;
					gMainMenu.Add(mi);
				}
				gMainMenu.LeftToRight = true;
				base.m_Children.Add(gMainMenu);
			}
			catch
			{
			}
		}
		gMainMenu = new GMainMenu(10, 35 + m.Actions.Count * 23);
		mi = this.GetMenuFrom(ActionHandler.Root);
		mi.Tooltip = new Tooltip("To create a new instruction pick one from the menu below", Big: false, 200);
		mi.Text = "New...";
		mi.DropDown = true;
		gMainMenu.Add(this.FormatMenu(mi));
		gMainMenu.LeftToRight = true;
		base.m_Children.Add(gMainMenu);
	}

	private GMenuItem FormatMenu(GMenuItem mi)
	{
		mi.FillAlpha = 1f;
		mi.DefaultColor = GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f);
		mi.OverColor = GumpPaint.Blend(Color.SteelBlue, SystemColors.Control, 0.5f);
		mi.ExpandedColor = GumpPaint.Blend(Color.SteelBlue, SystemColors.Control, 0.5f);
		mi.SetHue(Hues.Load(1));
		return mi;
	}

	public static string Find(string toFind, ParamNode[] nodes)
	{
		int num = 0;
		while (nodes != null && num < nodes.Length)
		{
			string text = GMacroEditorPanel.Find(toFind, nodes[num]);
			if (text != null)
			{
				return text;
			}
			num++;
		}
		return null;
	}

	public static string Find(string toFind, ParamNode n)
	{
		if (n.Param == toFind)
		{
			return n.Name;
		}
		return GMacroEditorPanel.Find(toFind, n.Nodes);
	}

	private GMenuItem GetMenuFrom(ActionNode n)
	{
		GMenuItem gMenuItem = new GMenuItem(n.Name);
		for (int i = 0; i < n.Nodes.Count; i++)
		{
			gMenuItem.Add(this.GetMenuFrom(n.Nodes[i]));
		}
		for (int j = 0; j < n.Handlers.Count; j++)
		{
			ActionHandler actionHandler = n.Handlers[j];
			GMenuItem gMenuItem2 = new GNewActionMenu(this, this.m_Macro, actionHandler);
			int num = 0;
			while (actionHandler.Params != null && num < actionHandler.Params.Length)
			{
				gMenuItem2.Add(this.GetMenuFrom(actionHandler.Params[num], null, actionHandler));
				num++;
			}
			gMenuItem.Add(this.FormatMenu(gMenuItem2));
		}
		return this.FormatMenu(gMenuItem);
	}

	private GMenuItem GetMenuFrom(ParamNode n, Action a, ActionHandler ah)
	{
		GMenuItem gMenuItem = ((n.Param == null) ? new GMenuItem(n.Name) : new GParamMenu(n, ah, a));
		if (n.Nodes != null)
		{
			for (int i = 0; i < n.Nodes.Length; i++)
			{
				gMenuItem.Add(this.GetMenuFrom(n.Nodes[i], a, ah));
			}
		}
		return this.FormatMenu(gMenuItem);
	}
}
