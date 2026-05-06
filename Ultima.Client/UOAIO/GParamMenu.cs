namespace UOAIO;

public class GParamMenu : GMenuItem
{
	private ParamNode m_Param;

	private Action m_Action;

	private ActionHandler m_Handler;

	public Action Action => this.m_Action;

	public ParamNode Param => this.m_Param;

	public ActionHandler Handler => this.m_Handler;

	public GParamMenu(ParamNode param, ActionHandler handler, Action action)
		: base(param.Name)
	{
		this.m_Param = param;
		this.m_Handler = handler;
		this.m_Action = action;
		if (this.m_Action == null)
		{
			base.Tooltip = new Tooltip($"Click here to add the instruction:\n{handler.Name} {param.Name}", Big: true);
		}
		else
		{
			base.Tooltip = new Tooltip("Click here to change the parameter", Big: true);
		}
		base.Tooltip.Delay = 3f;
	}

	public override void OnClick()
	{
		string param = this.m_Param.Param;
		if (param == null)
		{
			return;
		}
		Action action = this.m_Action;
		if (action == null)
		{
			Gump parent = base.m_Parent;
			while (parent != null && !(parent is GMacroEditorPanel))
			{
				parent = parent.Parent;
			}
			if (parent is GMacroEditorPanel)
			{
				Macro macro = ((GMacroEditorPanel)parent).Macro;
				macro.AddAction(new Action(this.m_Handler.Action, param));
				((GMacroEditorForm)parent.Parent.Parent).Current = macro;
			}
		}
		else
		{
			action.Param = param;
			GMenuItem gMenuItem = this;
			while (gMenuItem.Parent is GMenuItem)
			{
				gMenuItem = (GMenuItem)gMenuItem.Parent;
			}
			gMenuItem.Text = this.m_Param.Name;
		}
	}
}
