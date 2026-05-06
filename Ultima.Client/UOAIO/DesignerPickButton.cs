namespace UOAIO;

public class DesignerPickButton : GButtonNew
{
	private DesignerGump m_Designer;

	public DesignerPickButton(DesignerGump designer, int x, int y)
		: base(22121, 22122, 22123, x, y)
	{
		this.m_Designer = designer;
	}

	protected override void OnClicked()
	{
	}
}
