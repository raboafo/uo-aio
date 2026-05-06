using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GMacroKeyboard : GAlphaBackground
{
	private MacroModifiers m_Mods;

	private object[] m_Buttons;

	private object[] m_HighButtons;

	private GSystemButton m_All;

	private GSystemButton m_Ctrl;

	private GSystemButton m_Alt;

	private GSystemButton m_Shift;

	private bool m_Bold = true;

	private float m_fX;

	private float m_fY;

	private const int Size = 28;

	public MacroModifiers Mods
	{
		get
		{
			return this.m_Mods;
		}
		set
		{
			this.m_Mods = value;
			this.Update();
			this.UpdateModifiers();
		}
	}

	public void Update()
	{
		ArrayList dataStore = Engine.GetDataStore();
		Gump[] array = base.m_Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is GMacroKeyButton { Macro: not null } gMacroKeyButton)
			{
				dataStore.Add(gMacroKeyButton);
			}
		}
		bool flag = (this.m_Mods & MacroModifiers.All) != 0;
		bool flag2 = (this.m_Mods & MacroModifiers.Ctrl) != 0;
		bool flag3 = (this.m_Mods & MacroModifiers.Alt) != 0;
		bool flag4 = (this.m_Mods & MacroModifiers.Shift) != 0;
		MacroCollection list = Macros.List;
		for (int j = 0; j < list.Count; j++)
		{
			Macro macro = list[j];
			if (!flag && (macro.Control != flag2 || macro.Alt != flag3 || macro.Shift != flag4))
			{
				continue;
			}
			object button = this.GetButton(macro.Key);
			if (button is GMacroKeyButton[])
			{
				GMacroKeyButton[] array2 = (GMacroKeyButton[])button;
				for (int k = 0; k < array2.Length; k++)
				{
					this.SetMacro(dataStore, array2[k], macro);
				}
			}
			else if (button is GMacroKeyButton)
			{
				this.SetMacro(dataStore, (GMacroKeyButton)button, macro);
			}
		}
		for (int l = 0; l < dataStore.Count; l++)
		{
			((GMacroKeyButton)dataStore[l]).Macro = null;
		}
		Engine.ReleaseDataStore(dataStore);
	}

	private void SetMacro(ArrayList list, GMacroKeyButton btn, Macro mc)
	{
		if (list.Contains(btn) || btn.Macro == null)
		{
			list.Remove(btn);
			btn.Macro = mc;
		}
		else if (btn.Macro is Macro)
		{
			btn.Macro = new Macro[2]
			{
				(Macro)btn.Macro,
				mc
			};
		}
		else if (btn.Macro is Macro[])
		{
			Macro[] array = (Macro[])btn.Macro;
			Macro[] array2 = new Macro[array.Length + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = array[i];
			}
			array2[array.Length] = mc;
			btn.Macro = array2;
		}
	}

	private object GetButton(Keys key)
	{
		int num = key switch
		{
			Keys.Shift => 65536, 
			Keys.Alt => 65537, 
			Keys.Control => 65538, 
			_ => (int)key, 
		};
		if (num >= 0 && num < this.m_Buttons.Length)
		{
			return this.m_Buttons[num];
		}
		num -= 65536;
		if (num >= 0 && num < this.m_HighButtons.Length)
		{
			return this.m_HighButtons[num];
		}
		return null;
	}

	public void SetButton(Keys key, GMacroKeyButton btn)
	{
		int num = key switch
		{
			Keys.Shift => 65536, 
			Keys.Alt => 65537, 
			Keys.Control => 65538, 
			_ => (int)key, 
		};
		if (num >= 0 && num < this.m_Buttons.Length)
		{
			this.SetButton(this.m_Buttons, num, btn);
			return;
		}
		num -= 65536;
		if (num >= 0 && num < this.m_HighButtons.Length)
		{
			this.SetButton(this.m_HighButtons, num, btn);
		}
	}

	private void SetButton(object[] objs, int index, GMacroKeyButton btn)
	{
		object obj = objs[index];
		if (!(obj is GMacroKeyButton[]))
		{
			if (obj is GMacroKeyButton)
			{
				objs[index] = new GMacroKeyButton[2]
				{
					(GMacroKeyButton)obj,
					btn
				};
			}
			else
			{
				objs[index] = btn;
			}
		}
	}

	protected internal override void OnDragStart()
	{
		if (base.m_Parent != null)
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
			Point point = base.PointToScreen(new Point(0, 0)) - base.m_Parent.PointToScreen(new Point(0, 0));
			base.m_Parent.m_OffsetX = point.X + base.m_OffsetX;
			base.m_Parent.m_OffsetY = point.Y + base.m_OffsetY;
			base.m_Parent.m_IsDragging = true;
			Gumps.Drag = base.m_Parent;
		}
	}

	private void UpdateModifiers()
	{
		bool flag = (this.m_Mods & MacroModifiers.All) == 0;
		this.UpdateModifier(this.m_All, "", enabled: true, flag);
		this.UpdateModifier(this.m_Ctrl, "Ctrl", flag, (this.m_Mods & MacroModifiers.Ctrl) != 0);
		this.UpdateModifier(this.m_Alt, "Alt", flag, (this.m_Mods & MacroModifiers.Alt) != 0);
		this.UpdateModifier(this.m_Shift, "Shift", flag, (this.m_Mods & MacroModifiers.Shift) != 0);
	}

	private void UpdateModifier(GSystemButton btn, string prefix, bool enabled, bool opt)
	{
		if (!enabled)
		{
			Color color = (btn.PressedColor = SystemColors.Control);
			Color inactiveColor = (btn.ActiveColor = color);
			btn.InactiveColor = inactiveColor;
		}
		else if (opt)
		{
			btn.SetBackColor(GumpPaint.Blend(Color.SteelBlue, SystemColors.Control, 0.5f));
		}
		else
		{
			btn.SetBackColor(GumpPaint.Blend(Color.SteelBlue, SystemColors.Control, 0.25f));
			btn.InactiveColor = GumpPaint.Blend(Color.White, SystemColors.Control, 0.5f);
		}
	}

	private void All_OnClick(Gump g)
	{
		this.Mods ^= MacroModifiers.All;
	}

	private void Ctrl_OnClick(Gump g)
	{
		if ((this.m_Mods & MacroModifiers.All) == 0)
		{
			this.Mods ^= MacroModifiers.Ctrl;
		}
	}

	private void Alt_OnClick(Gump g)
	{
		if ((this.m_Mods & MacroModifiers.All) == 0)
		{
			this.Mods ^= MacroModifiers.Alt;
		}
	}

	private void Shift_OnClick(Gump g)
	{
		if ((this.m_Mods & MacroModifiers.All) == 0)
		{
			this.Mods ^= MacroModifiers.Shift;
		}
	}

	public GMacroKeyboard()
		: base(0, 0, 639, 184)
	{
		this.m_Buttons = new object[256];
		this.m_HighButtons = new object[256];
		base.FillColor = GumpColors.Control;
		base.FillAlpha = 1f;
		base.m_NonRestrictivePicking = true;
		int num = this.Width - 98;
		this.m_All = new GSystemButton(num - 19, 10, 20, 20, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), Color.Black, "", Engine.GetUniFont(2));
		this.m_Ctrl = new GSystemButton(num, 10, 32, 20, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), Color.Black, "Ctrl", Engine.GetUniFont(2));
		this.m_Alt = new GSystemButton(num + 31, 10, 32, 20, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), Color.Black, "Alt", Engine.GetUniFont(2));
		this.m_Shift = new GSystemButton(num + 62, 10, 32, 20, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), Color.Black, "Shift", Engine.GetUniFont(2));
		this.m_All.OnClick = All_OnClick;
		this.m_Ctrl.OnClick = Ctrl_OnClick;
		this.m_Alt.OnClick = Alt_OnClick;
		this.m_Shift.OnClick = Shift_OnClick;
		base.m_Children.Add(this.m_All);
		base.m_Children.Add(this.m_Ctrl);
		base.m_Children.Add(this.m_Alt);
		base.m_Children.Add(this.m_Shift);
		this.PlaceKey(Keys.Escape, "Esc");
		this.Skip();
		this.PlaceKey(Keys.F1);
		this.PlaceKey(Keys.F2);
		this.PlaceKey(Keys.F3);
		this.PlaceKey(Keys.F4);
		this.Skip(0.5f);
		this.PlaceKey(Keys.F5);
		this.PlaceKey(Keys.F6);
		this.PlaceKey(Keys.F7);
		this.PlaceKey(Keys.F8);
		this.Skip(0.5f);
		this.PlaceKey(Keys.F9);
		this.PlaceKey(Keys.F10);
		this.PlaceKey(Keys.F11);
		this.PlaceKey(Keys.F12);
		this.Skip(0.25f);
		this.m_Bold = false;
		this.PlaceKey(Keys.Snapshot, "Prnt");
		this.PlaceKey(Keys.Scroll, "Scrl");
		this.PlaceKey(Keys.Pause, "Paus");
		this.m_Bold = true;
		this.m_fX = 0f;
		this.m_fY += 1.25f;
		this.PlaceKey(Keys.Oemtilde, "~");
		this.PlaceKey(Keys.D1, "1");
		this.PlaceKey(Keys.D2, "2");
		this.PlaceKey(Keys.D3, "3");
		this.PlaceKey(Keys.D4, "4");
		this.PlaceKey(Keys.D5, "5");
		this.PlaceKey(Keys.D6, "6");
		this.PlaceKey(Keys.D7, "7");
		this.PlaceKey(Keys.D8, "8");
		this.PlaceKey(Keys.D9, "9");
		this.PlaceKey(Keys.D0, "0");
		this.PlaceKey(Keys.OemMinus, "-");
		this.PlaceKey(Keys.Oemplus, "+");
		this.m_Bold = false;
		this.PlaceKey(Keys.Back, "Backspace", 2f);
		this.m_Bold = true;
		this.Skip(0.25f);
		this.m_Bold = false;
		this.PlaceKey(Keys.Insert, "Ins");
		this.PlaceKey(Keys.Home);
		this.m_Bold = true;
		this.PlaceKey(Keys.Prior, "↑");
		this.Skip(0.25f);
		this.PlaceKey(Keys.NumLock, "Num");
		this.PlaceKey(Keys.Divide, "/");
		this.PlaceKey(Keys.Multiply, "*");
		this.PlaceKey(Keys.Subtract, "-");
		this.m_fX = 0f;
		this.m_fY += 1f;
		this.PlaceKey(Keys.Tab, 1.5f);
		this.PlaceKey(Keys.Q);
		this.PlaceKey(Keys.W);
		this.PlaceKey(Keys.E);
		this.PlaceKey(Keys.R);
		this.PlaceKey(Keys.T);
		this.PlaceKey(Keys.Y);
		this.PlaceKey(Keys.U);
		this.PlaceKey(Keys.I);
		this.PlaceKey(Keys.O);
		this.PlaceKey(Keys.P);
		this.PlaceKey(Keys.OemOpenBrackets, "[");
		this.PlaceKey(Keys.OemCloseBrackets, "]");
		this.PlaceKey(Keys.OemPipe, "\\", 1.5f);
		this.Skip(0.25f);
		this.m_Bold = false;
		this.PlaceKey(Keys.Delete, "Del");
		this.PlaceKey(Keys.End);
		this.m_Bold = true;
		this.PlaceKey(Keys.Next, "↓");
		this.Skip(0.25f);
		this.PlaceKey(Keys.NumPad7, "7");
		this.PlaceKey(Keys.NumPad8, "8");
		this.PlaceKey(Keys.NumPad9, "9");
		this.PlaceKey(Keys.Add, "+", 1f, 2f);
		this.m_fX = 0f;
		this.m_fY += 1f;
		this.PlaceKey(Keys.Capital, "Caps", 1.75f);
		this.PlaceKey(Keys.A);
		this.PlaceKey(Keys.S);
		this.PlaceKey(Keys.D);
		this.PlaceKey(Keys.F);
		this.PlaceKey(Keys.G);
		this.PlaceKey(Keys.H);
		this.PlaceKey(Keys.J);
		this.PlaceKey(Keys.K);
		this.PlaceKey(Keys.L);
		this.PlaceKey(Keys.OemSemicolon, ";");
		this.PlaceKey(Keys.OemQuotes, "'");
		this.PlaceKey(Keys.Return, 2.25f);
		this.Skip(3.5f);
		this.PlaceKey(Keys.NumPad4, "4");
		this.PlaceKey(Keys.NumPad5, "5");
		this.PlaceKey(Keys.NumPad6, "6");
		this.m_fX = 0f;
		this.m_fY += 1f;
		this.PlaceKey(Keys.ShiftKey, "Shift", 2.25f);
		this.PlaceKey(Keys.Z);
		this.PlaceKey(Keys.X);
		this.PlaceKey(Keys.C);
		this.PlaceKey(Keys.V);
		this.PlaceKey(Keys.B);
		this.PlaceKey(Keys.N);
		this.PlaceKey(Keys.M);
		this.PlaceKey(Keys.Oemcomma, ",");
		this.PlaceKey(Keys.OemPeriod, ".");
		this.PlaceKey(Keys.OemQuestion, "/");
		this.PlaceKey(Keys.ShiftKey, "Shift", 2.75f);
		this.Skip(1.25f);
		this.PlaceKey(Keys.Up, "↑");
		this.Skip(1.25f);
		this.PlaceKey(Keys.NumPad1, "1");
		this.PlaceKey(Keys.NumPad2, "2");
		this.PlaceKey(Keys.NumPad3, "3");
		this.m_Bold = false;
		this.PlaceKey(Keys.Return, "Entr", 1f, 2f);
		this.m_Bold = true;
		this.m_fX = 0f;
		this.m_fY += 1f;
		this.PlaceKey(Keys.ControlKey, "Ctrl", 1.5f);
		this.PlaceKey(Keys.LWin, "Win", 1.25f);
		this.PlaceKey(Keys.Menu, "Alt", 1.25f);
		this.PlaceKey(Keys.Space, 5.75f);
		this.PlaceKey(Keys.Menu, "Alt", 1.25f);
		this.PlaceKey(Keys.RWin, "Win", 1.25f);
		this.PlaceKey(Keys.Apps, 1.25f);
		this.PlaceKey(Keys.ControlKey, "Ctrl", 1.5f);
		this.Skip(0.25f);
		this.PlaceKey(Keys.Left, "←");
		this.PlaceKey(Keys.Down, "↓");
		this.PlaceKey(Keys.Right, "→");
		this.Skip(0.25f);
		this.PlaceKey(Keys.NumPad0, "0", 2f);
		this.PlaceKey(Keys.Decimal, ".");
		this.Mods = MacroModifiers.All;
	}

	protected internal override void Render(int X, int Y)
	{
		base.Render(X, Y);
		if (Gumps.LastOver is GMenuItem)
		{
			Renderer.SetTexture(null);
			Renderer.PushAlpha(0.4f);
			Renderer.SolidRect(0, X + base.m_X + 1, Y + base.m_Y + 1, base.m_Width - 2, base.m_Height - 2);
			Renderer.PopAlpha();
		}
	}

	public void Skip()
	{
		this.Skip(1f);
	}

	public void Skip(float w)
	{
		this.m_fX += w;
	}

	public void PlaceKey(Keys key)
	{
		this.PlaceKey(key, key.ToString(), 1f);
	}

	public void PlaceKey(Keys key, string name)
	{
		this.PlaceKey(key, name, 1f);
	}

	public void PlaceKey(Keys key, float w)
	{
		this.PlaceKey(key, key.ToString(), w);
	}

	public void PlaceKey(Keys key, string name, float w)
	{
		this.PlaceKey(key, name, w, 1f);
	}

	public void PlaceKey(Keys key, string name, float w, float h)
	{
		GMacroKeyButton gMacroKeyButton = new GMacroKeyButton(key, name, this.m_Bold, 4 + (int)(this.m_fX * 28f), 4 + (int)(this.m_fY * 28f), 1 + (int)(w * 28f), 1 + (int)(h * 28f));
		this.SetButton(key, gMacroKeyButton);
		base.m_Children.Add(gMacroKeyButton);
		this.m_fX += w;
	}
}
