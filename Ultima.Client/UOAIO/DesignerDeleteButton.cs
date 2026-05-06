using UOAIO.Targeting;

namespace UOAIO;

public class DesignerDeleteButton : GButtonNew
{
	private DesignerGump m_Designer;

	public DesignerDeleteButton(DesignerGump designer, int x, int y)
		: base(22118, 22119, 22120, x, y)
	{
		this.m_Designer = designer;
	}

	protected override void OnClicked()
	{
		this.m_Designer.Context.Entry = null;
		TargetManager.Client = new DesignerDeleteTarget(this.m_Designer.Context);
	}
}
