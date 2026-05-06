namespace UOAIO;

public class DesignerSystemButton : GButtonNew
{
	private DesignContext m_Context;

	private int m_Type;

	public DesignerSystemButton(DesignContext context, int x, int y, int type, string name)
		: base(2445, 2445, 2445, x, y)
	{
		this.m_Context = context;
		this.m_Type = type;
		GLabel gLabel = new GLabel(name, Engine.GetUniFont(1), Hues.Load(1), x, y);
		base.m_Children.Add(gLabel);
		gLabel.Center();
	}

	protected override void OnClicked()
	{
		switch (this.m_Type)
		{
		case 0:
			Network.Send(new PDesigner_Backup(this.m_Context.House));
			break;
		case 1:
			Network.Send(new PDesigner_Restore(this.m_Context.House));
			break;
		case 2:
			Network.Send(new PDesigner_Sync(this.m_Context.House));
			break;
		case 3:
			Network.Send(new PDesigner_Clear(this.m_Context.House));
			break;
		case 4:
			Network.Send(new PDesigner_Commit(this.m_Context.House));
			break;
		case 5:
			Network.Send(new PDesigner_Revert(this.m_Context.House));
			break;
		}
	}
}
