using System.Windows.Forms;

namespace UOAIO;

public class GMacroKeyEntry : GTextBox
{
	private bool m_Recording;

	public override bool ShowCaret => true;

	public GMacroKeyEntry(string text, int x, int y, int w, int h)
		: base(0, HasBorder: false, x, y, w, h, text, Engine.GetUniFont(2), GumpHues.WindowText, GumpHues.WindowText, GumpHues.WindowText)
	{
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		base.OnMouseUp(X, Y, mb);
		if (mb == MouseButtons.Middle)
		{
			this.Start();
			this.Finish((Keys)69634, Control.ModifierKeys);
		}
	}

	protected internal override void OnMouseWheel(int Delta)
	{
		base.OnMouseWheel(Delta);
		if (Delta > 0)
		{
			this.Start();
			this.Finish((Keys)69632, Control.ModifierKeys);
		}
		else if (Delta < 0)
		{
			this.Start();
			this.Finish((Keys)69633, Control.ModifierKeys);
		}
	}

	public void Finish(Keys key, Keys mods)
	{
		if (this.m_Recording)
		{
			this.m_Recording = false;
			base.String = GMacroEditorPanel.GetKeyName(key);
			if (base.m_Parent is GMacroEditorPanel gMacroEditorPanel)
			{
				gMacroEditorPanel.Macro.Key = key;
				gMacroEditorPanel.Macro.Mods = mods;
				gMacroEditorPanel.UpdateModifiers();
				gMacroEditorPanel.NotifyParent();
			}
		}
	}

	public void Start()
	{
		if (!this.m_Recording)
		{
			this.m_Recording = true;
		}
	}

	protected internal override bool OnKeyDown(char Key)
	{
		return true;
	}
}
