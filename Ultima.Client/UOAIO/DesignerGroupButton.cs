namespace UOAIO;

public class DesignerGroupButton : GButtonNew
{
	private DesignerGump m_Designer;

	private DesignerGroup m_Group;

	public DesignerGroupButton(DesignerGump designer, DesignerGroup group, int x, int y, int gumpID)
		: base(gumpID, gumpID + 1, gumpID + 2, x, y)
	{
		this.m_Designer = designer;
		this.m_Group = group;
	}

	protected override void OnClicked()
	{
		this.m_Designer.Group = this.m_Group;
	}
}
