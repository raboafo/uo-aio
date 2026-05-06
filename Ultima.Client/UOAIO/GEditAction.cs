using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GEditAction : GWindowsForm
{
	private GMacroEditorPanel m_Panel;

	private Macro m_Macro;

	private Action m_Action;

	public GEditAction(GMacroEditorPanel p, Macro macro, Action action)
		: base(0, 0, 103, 86)
	{
		this.m_Panel = p;
		this.m_Macro = macro;
		this.m_Action = action;
		Gumps.Modal = this;
		Gumps.Focus = this;
		base.Text = "Edit";
		base.m_NonRestrictivePicking = true;
		GSystemButton gSystemButton = this.AddButton("↑", 6, 7, 24, 24, Up_OnClick);
		gSystemButton.Tooltip = new Tooltip("Moves the instruction up", Big: true);
		gSystemButton = this.AddButton("↓", 6, 30, 24, 24, Down_OnClick);
		gSystemButton.Tooltip = new Tooltip("Moves the instruction down", Big: true);
		gSystemButton = this.AddButton("Delete", 39, 7, 50, 24, Delete_OnClick);
		gSystemButton.Tooltip = new Tooltip("Removes the instruction", Big: true);
		this.Center();
	}

	private void Delete_OnClick(Gump g)
	{
		this.m_Macro.Actions.Remove(this.m_Action);
		GMacroEditorForm gMacroEditorForm = this.m_Panel.Parent.Parent as GMacroEditorForm;
		if (gMacroEditorForm != null)
		{
			gMacroEditorForm.Current = gMacroEditorForm.Current;
		}
		Gumps.Destroy(this);
		Gumps.Focus = gMacroEditorForm;
	}

	private void Up_OnClick(Gump g)
	{
		int num = this.m_Macro.Actions.IndexOf(this.m_Action);
		if (num > 0)
		{
			this.m_Macro.Actions.RemoveAt(num);
			this.m_Macro.Actions.Insert(num - 1, this.m_Action);
		}
		if (this.m_Panel.Parent.Parent is GMacroEditorForm gMacroEditorForm)
		{
			gMacroEditorForm.Current = gMacroEditorForm.Current;
		}
	}

	private void Down_OnClick(Gump g)
	{
		int num = this.m_Macro.Actions.IndexOf(this.m_Action);
		if (num > this.m_Macro.Actions.Count - 1)
		{
			this.m_Macro.Actions.RemoveAt(num);
			this.m_Macro.Actions.Insert(num + 1, this.m_Action);
		}
		if (this.m_Panel.Parent.Parent is GMacroEditorForm gMacroEditorForm)
		{
			gMacroEditorForm.Current = gMacroEditorForm.Current;
		}
	}

	private GSystemButton AddButton(string name, int x, int y, int w, int h, OnClick onClick)
	{
		GSystemButton gSystemButton = new GSystemButton(x, y, w, h, SystemColors.Control, SystemColors.ControlText, name, Engine.GetUniFont(2));
		gSystemButton.OnClick = onClick;
		base.Client.Children.Add(gSystemButton);
		return gSystemButton;
	}

	private GTextBox AddTextBox(string name, int index, string initialText, char pc)
	{
		int num = 30 + index * 25;
		GLabel gLabel = new GLabel(name, Engine.GetUniFont(2), GumpHues.ControlText, 0, 0);
		gLabel.X = 10 - gLabel.Image.xMin;
		gLabel.Y = num + (22 - (gLabel.Image.yMax - gLabel.Image.yMin + 1)) / 2 - gLabel.Image.yMin;
		base.m_Children.Add(gLabel);
		GAlphaBackground gAlphaBackground = new GAlphaBackground(60, num, 200, 22);
		gAlphaBackground.ShouldHitTest = false;
		gAlphaBackground.FillColor = GumpColors.Window;
		gAlphaBackground.FillAlpha = 1f;
		base.m_Children.Add(gAlphaBackground);
		IHue windowText = GumpHues.WindowText;
		GTextBox gTextBox = new GTextBox(0, HasBorder: false, 64, num, 196, 22, initialText, Engine.GetUniFont(2), windowText, windowText, windowText, pc);
		base.Client.Children.Add(gTextBox);
		return gTextBox;
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Right)
		{
			Gumps.Destroy(this);
		}
	}
}
