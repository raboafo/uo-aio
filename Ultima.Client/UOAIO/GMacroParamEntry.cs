namespace UOAIO;

public class GMacroParamEntry : GTextBox
{
	private Action m_Action;

	public GMacroParamEntry(Action action, string text, int x, int y, int w, int h)
		: base(0, HasBorder: false, x, y, w, h, text, Engine.GetUniFont(2), GumpHues.WindowText, GumpHues.WindowText, GumpHues.WindowText)
	{
		this.m_Action = action;
		base.OnTextChange = UpdateParam;
	}

	private void UpdateParam(string str, Gump g)
	{
		this.m_Action.Param = str;
	}
}
