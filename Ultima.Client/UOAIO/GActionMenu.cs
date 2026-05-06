namespace UOAIO;

public class GActionMenu : GMenuItem
{
	private GMacroEditorPanel m_Panel;

	private Macro m_Macro;

	private Action m_Action;

	public GActionMenu(GMacroEditorPanel panel, Macro macro, Action action)
		: base(action.Handler.Name)
	{
		this.m_Panel = panel;
		this.m_Macro = macro;
		this.m_Action = action;
		base.Tooltip = new Tooltip("Click here to edit this action", Big: true);
	}

	public override void OnClick()
	{
		Gumps.Desktop.Children.Add(new GEditAction(this.m_Panel, this.m_Macro, this.m_Action));
	}
}
