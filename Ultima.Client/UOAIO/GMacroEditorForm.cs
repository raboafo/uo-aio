using System.Drawing;
using Microsoft.Win32;

namespace UOAIO;

public class GMacroEditorForm : GWindowsForm
{
	private GSystemButton m_KeyboardFlipper;

	private GMacroKeyboard m_Keyboard;

	private Macro m_Current;

	private GMacroEditorPanel m_Panel;

	private static GMacroEditorForm m_Instance;

	private GLabel m_NoSel;

	private GAlphaBackground m_Sunken;

	public static bool IsOpen => GMacroEditorForm.m_Instance != null;

	public GMacroKeyboard Keyboard => this.m_Keyboard;

	public bool ShowKeyboard
	{
		get
		{
			return this.m_Keyboard != null;
		}
		set
		{
			if (value)
			{
				if (this.m_Keyboard == null)
				{
					this.m_KeyboardFlipper.Text = "Hide Keyboard";
					this.m_Keyboard = new GMacroKeyboard();
					base.m_Children.Insert(0, this.m_Keyboard);
					this.m_Keyboard.Center();
					this.m_Keyboard.Y = this.Height - 1;
				}
			}
			else if (this.m_Keyboard != null)
			{
				this.m_KeyboardFlipper.Text = "Show Keyboard";
				Gumps.Destroy(this.m_Keyboard);
				this.m_Keyboard = null;
			}
		}
	}

	public Macro Current
	{
		get
		{
			return this.m_Current;
		}
		set
		{
			bool flag = this.m_Current != null && this.m_Current != value && this.m_Current.Actions.Count == 0;
			if (flag && Macros.Current.Macros.Contains(this.m_Current))
			{
				Macros.Current.Macros.Remove(this.m_Current);
			}
			if (this.m_Panel != null)
			{
				Gumps.Destroy(this.m_Panel);
			}
			this.m_Panel = null;
			this.m_Current = value;
			if (this.m_Current != null)
			{
				this.m_Panel = new GMacroEditorPanel(this.m_Current);
				this.m_Panel.X = 1;
				this.m_Panel.Y = 2;
				base.Client.Children.Add(this.m_Panel);
			}
			if (this.m_Current == null && Macros.List.Count > 0)
			{
				this.Current = Macros.List[0];
			}
			else if (this.m_Current != null && this.m_NoSel != null)
			{
				Gumps.Destroy(this.m_NoSel);
				this.m_NoSel = null;
			}
			else if (this.m_Current == null && this.m_NoSel == null)
			{
				this.m_NoSel = new GLabel("No macro is currently selected", Engine.GetUniFont(1), Hues.Load(1153), 16, 18);
				base.Client.Children.Add(this.m_NoSel);
			}
			if (flag)
			{
				this.UpdateKeyboard();
			}
		}
	}

	public static void Open()
	{
		if (GMacroEditorForm.m_Instance == null)
		{
			GMacroEditorForm.m_Instance = new GMacroEditorForm();
			Gumps.Desktop.Children.Add(GMacroEditorForm.m_Instance);
			Gumps.Focus = GMacroEditorForm.m_Instance;
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		Renderer.SetTexture(null);
		GumpPaint.DrawSunken3D(X + base.Client.X + this.m_Sunken.X - 1, Y + base.Client.Y + this.m_Sunken.Y - 1, this.m_Sunken.Width + 2, this.m_Sunken.Height + 2);
	}

	private static Color ReadRegistryColor(string name)
	{
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Colors", writable: false);
			string text = registryKey.GetValue(name) as string;
			string[] array = text.Split(' ');
			return Color.FromArgb(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]));
		}
		catch
		{
		}
		return Color.White;
	}

	public void UpdateKeyboard()
	{
		if (this.m_Keyboard != null)
		{
			this.m_Keyboard.Update();
		}
	}

	public GMacroEditorForm()
		: base(0, 0, 269, 283)
	{
		Gumps.Focus = this;
		base.m_NonRestrictivePicking = true;
		base.Client.m_NonRestrictivePicking = true;
		base.Text = "Macro Editor";
		GAlphaBackground gAlphaBackground = (this.m_Sunken = new GAlphaBackground(1, 2, 259, 230));
		gAlphaBackground.ShouldHitTest = false;
		gAlphaBackground.FillAlpha = 1f;
		gAlphaBackground.FillColor = GumpColors.AppWorkspace;
		gAlphaBackground.DrawBorder = false;
		base.Client.Children.Add(gAlphaBackground);
		this.m_KeyboardFlipper = new GSystemButton(71, 236, 120, 20, SystemColors.Control, SystemColors.ControlText, "Show Keyboard", Engine.GetUniFont(2));
		this.m_KeyboardFlipper.OnClick = KeyboardFlipper_OnClick;
		base.Client.Children.Add(this.m_KeyboardFlipper);
		GSystemButton toAdd = new GSystemButton(240, 236, 20, 20, SystemColors.Control, SystemColors.ControlText, "→", Engine.GetUniFont(2))
		{
			Tooltip = new Tooltip("Advance to the next macro", Big: true),
			OnClick = Next_OnClick
		};
		base.Client.Children.Add(toAdd);
		GSystemButton toAdd2 = new GSystemButton(1, 236, 20, 20, SystemColors.Control, SystemColors.ControlText, "←", Engine.GetUniFont(2))
		{
			Tooltip = new Tooltip("Go back to the previous macro", Big: true),
			OnClick = Prev_OnClick
		};
		base.Client.Children.Add(toAdd2);
		this.Center();
		this.Y -= 92;
		if (Macros.List.Count > 0)
		{
			this.Current = Macros.List[0];
			return;
		}
		this.m_NoSel = new GLabel("No macro is currently selected", Engine.GetUniFont(1), Hues.Load(1153), 16, 18);
		base.Client.Children.Add(this.m_NoSel);
	}

	private void Next_OnClick(Gump g)
	{
		if (Macros.List.Count != 0)
		{
			int num = (Macros.List.IndexOf(this.m_Current) + 1) % Macros.List.Count;
			if (num >= 0 && num < Macros.List.Count)
			{
				this.Current = Macros.List[num];
			}
		}
	}

	private void Prev_OnClick(Gump g)
	{
		if (Macros.List.Count != 0)
		{
			int num = (Macros.List.IndexOf(this.m_Current) - 1) % Macros.List.Count;
			if (num < 0)
			{
				num += Macros.List.Count;
			}
			if (num >= 0 && num < Macros.List.Count)
			{
				this.Current = Macros.List[num];
			}
		}
	}

	private void KeyboardFlipper_OnClick(Gump g)
	{
		this.ShowKeyboard = !this.ShowKeyboard;
	}

	protected internal override void OnDispose()
	{
		GMacroEditorForm.m_Instance = null;
		Macros.Save();
		base.OnDispose();
	}
}
