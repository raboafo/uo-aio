namespace UOAIO;

public class GNewActionMenu : GMenuItem
{
	private GMacroEditorPanel m_Panel;

	private Macro m_Macro;

	private ActionHandler m_Action;

	public GNewActionMenu(GMacroEditorPanel panel, Macro macro, ActionHandler action)
		: base(action.Name)
	{
		this.m_Panel = panel;
		this.m_Macro = macro;
		this.m_Action = action;
		if (this.m_Action.Params != null && this.m_Action.Params.Length != 0)
		{
			base.Tooltip = new Tooltip("Choose a parameter from the menu to the right, or just click here to add the instruction with a default parameter.", Big: false, 200);
		}
		else
		{
			base.Tooltip = new Tooltip("Click here to add this instruction.", Big: false, 200);
		}
		base.Tooltip.Delay = 2f;
	}

	public override void OnClick()
	{
		this.m_Macro.Actions.Add(new Action(this.m_Action.Action, this.FindFirst(this.m_Action.Params)));
		if (this.m_Panel.Parent.Parent is GMacroEditorForm gMacroEditorForm)
		{
			gMacroEditorForm.Current = gMacroEditorForm.Current;
		}
	}

	private string FindFirst(ParamNode[] nodes)
	{
		string text = "";
		int num = 0;
		while (nodes != null && text == null && num < nodes.Length)
		{
			text = this.FindFirst(nodes[num]);
			num++;
		}
		return text;
	}

	private string FindFirst(ParamNode node)
	{
		if (node.Param != null)
		{
			return node.Param;
		}
		return this.FindFirst(node.Nodes);
	}
}
