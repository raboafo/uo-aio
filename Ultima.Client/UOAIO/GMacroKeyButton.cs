using System;
using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GMacroKeyButton : GSystemButton
{
	private object m_Macro;

	private Keys m_Key;

	private GLabel m_Dots;

	public Keys Key => this.m_Key;

	public object Macro
	{
		get
		{
			return this.m_Macro;
		}
		set
		{
			if (this.m_Macro == value)
			{
				return;
			}
			this.m_Macro = value;
			if (this.m_Macro == null)
			{
				base.SetBackColor(GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f));
				if (this.m_Dots != null)
				{
					Gumps.Destroy(this.m_Dots);
				}
				this.m_Dots = null;
				base.Tooltip = new Tooltip($"{GMacroEditorPanel.GetKeyName(this.m_Key)}\nClick to create", Big: true);
				return;
			}
			base.SetBackColor(GumpPaint.Blend(Color.SteelBlue, SystemColors.Control, 0.5f));
			int num = 1;
			if (this.m_Macro is Macro)
			{
				base.Tooltip = new Tooltip("Jump to the macro", Big: true);
			}
			else
			{
				base.Tooltip = new Tooltip("Jump to the macros", Big: true);
				num = ((Macro[])this.m_Macro).Length;
			}
			if (this.m_Dots == null)
			{
				this.m_Dots = new GLabel(new string('.', num), Engine.GetUniFont(0), Hues.Load(1153), 4, 4);
				this.m_Dots.X -= this.m_Dots.Image.xMin;
				this.m_Dots.Y -= this.m_Dots.Image.yMin;
				base.m_Children.Add(this.m_Dots);
			}
			else if (this.m_Dots.Text.Length != num)
			{
				this.m_Dots.Text = new string('.', num);
			}
		}
	}

	public override float Darkness => 0.25f;

	public GMacroKeyButton(Keys key, string name, bool bold, int x, int y, int w, int h)
		: base(x, y, w, h, GumpPaint.Blend(Color.WhiteSmoke, SystemColors.Control, 0.5f), SystemColors.ControlText, name, bold ? Engine.GetUniFont(1) : Engine.GetUniFont(2))
	{
		this.m_Key = key;
		base.Tooltip = new Tooltip($"{GMacroEditorPanel.GetKeyName(this.m_Key)}\nClick to create", Big: true);
		base.FillAlpha = 1f;
		base.m_QuickDrag = false;
		base.m_CanDrag = true;
		base.OnClick = Clicked;
	}

	private void Clicked(Gump g)
	{
		if (!(base.m_Parent.Parent is GMacroEditorForm gMacroEditorForm))
		{
			return;
		}
		if (this.m_Macro == null)
		{
			Keys keys = Keys.None;
			MacroModifiers macroModifiers = MacroModifiers.All;
			if (gMacroEditorForm.Keyboard != null)
			{
				macroModifiers = gMacroEditorForm.Keyboard.Mods;
			}
			if (macroModifiers == MacroModifiers.All)
			{
				keys = Control.ModifierKeys;
			}
			else
			{
				if ((macroModifiers & MacroModifiers.Alt) != MacroModifiers.None)
				{
					keys |= Keys.Alt;
				}
				if ((macroModifiers & MacroModifiers.Shift) != MacroModifiers.None)
				{
					keys |= Keys.Shift;
				}
				if ((macroModifiers & MacroModifiers.Ctrl) != MacroModifiers.None)
				{
					keys |= Keys.Control;
				}
			}
			MacroSet current = Macros.Current;
			MacroData macroData = new MacroData();
			macroData.Key = this.m_Key;
			macroData.Mods = keys;
			Macro macro = new Macro(macroData);
			current.Macros.Add(macro);
			gMacroEditorForm.Current = macro;
			gMacroEditorForm.UpdateKeyboard();
		}
		else if (this.m_Macro is Macro)
		{
			gMacroEditorForm.Current = (Macro)this.m_Macro;
		}
		else if (this.m_Macro is Macro[])
		{
			Macro[] array = (Macro[])this.m_Macro;
			int num = Array.IndexOf(array, gMacroEditorForm.Current);
			gMacroEditorForm.Current = array[(num + 1) % array.Length];
		}
	}

	protected internal override void OnDragStart()
	{
		if (base.m_Parent.Parent != null)
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
			Point point = base.PointToScreen(new Point(0, 0)) - base.m_Parent.Parent.PointToScreen(new Point(0, 0));
			base.m_Parent.Parent.m_OffsetX = point.X + base.m_OffsetX;
			base.m_Parent.Parent.m_OffsetY = point.Y + base.m_OffsetY;
			base.m_Parent.Parent.m_IsDragging = true;
			Gumps.Drag = base.m_Parent.Parent;
		}
	}
}
