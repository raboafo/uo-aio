using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UOAIO.Profiles;
using UOAIO.Prompts;
using UOAIO.Targeting;

namespace UOAIO;

public class Display : Form
{
	private Container components = null;

	protected override void OnClosed(EventArgs e)
	{
		Engine.exiting = true;
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		e.Cancel = true;
		Engine.exiting = true;
	}

	protected override void OnSystemColorsChanged(EventArgs e)
	{
		base.OnSystemColorsChanged(e);
		GumpColors.Invalidate();
		GumpHues.Invalidate();
		GumpPaint.Invalidate();
	}

	public Display()
	{
		this.InitializeComponent();
		base.KeyPress += Display_KeyPress;
		base.MouseDown += Engine.MouseDown;
		base.MouseMove += Engine.MouseMove;
		base.MouseUp += Engine.MouseUp;
		base.MouseWheel += Engine.MouseWheel;
		this.Cursor.Dispose();
	}

	protected override void OnLocationChanged(EventArgs e)
	{
		if (Engine.m_EventOk && !Engine.m_Fullscreen && base.WindowState != FormWindowState.Minimized)
		{
			base.OnLocationChanged(e);
			Preferences.Current.Layout.Update();
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		if (!Engine.m_EventOk || Engine.m_Fullscreen || base.WindowState == FormWindowState.Minimized)
		{
			return;
		}
		base.OnSizeChanged(e);
		Preferences.Current.Layout.Update();
		try
		{
			Engine.ResetDevice();
			GC.Collect();
		}
		catch
		{
		}
	}

	protected override void OnResize(EventArgs ea)
	{
		if (Engine.m_EventOk && !Engine.m_Fullscreen && base.WindowState != FormWindowState.Minimized)
		{
			base.OnResize(ea);
			Preferences.Current.Layout.Update();
			try
			{
				Engine.ResetDevice();
				GC.Collect();
			}
			catch
			{
			}
			GC.Collect();
		}
	}

	protected override void OnClick(EventArgs e)
	{
		if (Engine.m_EventOk)
		{
			Engine.ClickMessage(this, e);
		}
	}

	protected override void OnDoubleClick(EventArgs e)
	{
		if (Engine.m_EventOk)
		{
			Engine.DoubleClick(this, e);
		}
	}

	protected override bool ProcessDialogKey(Keys key)
	{
		if (!Engine.m_EventOk)
		{
			return false;
		}
		if (Gumps.Focus is GMacroKeyEntry)
		{
			return false;
		}
		KeyEventArgs e = new KeyEventArgs(key);
		Engine.KeyDown(this, e);
		if (((key & Keys.Alt) == Keys.Alt && (key & Keys.F4) != Keys.F4) || (key & Keys.F10) == Keys.F10)
		{
			return true;
		}
		return base.ProcessDialogKey(key);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (Engine.m_EventOk && e.KeyCode != Keys.ShiftKey && e.KeyCode != Keys.ControlKey && e.KeyCode != Keys.Menu && Gumps.Focus is GMacroKeyEntry gMacroKeyEntry)
		{
			gMacroKeyEntry.Start();
		}
	}

	protected override void OnKeyUp(KeyEventArgs key)
	{
		if (Engine.m_EventOk)
		{
			Engine.KeyUp(key);
			if (Gumps.Focus is GMacroKeyEntry gMacroKeyEntry)
			{
				gMacroKeyEntry.Finish(key.KeyCode, key.Modifiers);
			}
		}
	}

	public void Display_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (!Engine.m_EventOk)
		{
			return;
		}
		if (Gumps.KeyDown(e.KeyChar))
		{
			e.Handled = true;
			return;
		}
		e.Handled = true;
		if (e.KeyChar == '\u001b')
		{
			if (TargetManager.IsActive)
			{
				TargetManager.Active.Cancel();
			}
			else if (Engine.Prompt != null)
			{
				Engine.Prompt.OnCancel(PromptCancelType.UserCancel);
				Engine.Prompt = null;
				return;
			}
		}
		if (Engine.m_Locked)
		{
			return;
		}
		if (e.KeyChar == '\b')
		{
			if (Engine.m_Text.Length > 0)
			{
				Engine.m_Text = Engine.m_Text.Substring(0, Engine.m_Text.Length - 1);
				Renderer.SetText(Engine.m_Text);
			}
			return;
		}
		if (e.KeyChar == '\r')
		{
			Engine.commandEntered(Engine.Encode(Engine.m_Text));
			Engine.m_Text = "";
			Renderer.SetText("");
			return;
		}
		if (e.KeyChar < ' ')
		{
			e.Handled = false;
			e.Handled = true;
			return;
		}
		string input = Engine.m_Text + e.KeyChar;
		string text = Engine.Encode(input) + "_";
		Mobile player = World.Player;
		int num = ((player == null || !player.OpenedStatus || player.StatusBar != null) ? (Engine.GameWidth - 4) : (Engine.GameWidth - 46));
		if (Engine.GetUniFont(3).GetStringWidth(text) < num)
		{
			Engine.m_Text = input;
			Renderer.SetText(input);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && this.components != null)
		{
			this.components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UOAIO.Display));
		base.SuspendLayout();
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(640, 480);
		this.ForeColor = System.Drawing.SystemColors.ControlText;
		// base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "Display";
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "Ultima Online";
		base.ResumeLayout(false);
	}
}
