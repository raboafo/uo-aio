namespace UOAIO;

public class DesignerGroupItem : ItemButtonGump
{
	private DesignerGump m_Designer;

	private DesignerGroup m_Group;

	private int m_ClipWidth;

	private int m_ClipHeight;

	public DesignerGroupItem(DesignerGump designer, DesignerGroup group, int x, int y, int ow, int oh)
		: base(x, y, group.ID.DisplayID)
	{
		this.m_Designer = designer;
		this.m_Group = group;
		this.m_ClipWidth = ow;
		this.m_ClipHeight = oh;
	}

	public override void OnClick()
	{
		this.m_Designer.Group = this.m_Group;
		if (this.m_Group.QuickUse && this.m_Group.Entries.Length != 0)
		{
			this.m_Designer.Context.Entry = this.m_Group.Entries[0];
		}
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return X < this.m_ClipWidth && Y < this.m_ClipHeight;
	}

	protected internal override void Draw(int X, int Y)
	{
		if (base.m_Draw)
		{
			base.m_Clipper = Clipper.TemporaryInstance(X, Y, this.m_ClipWidth, this.m_ClipHeight);
			base.m_Image.DrawClipped(X, Y, base.m_Clipper);
		}
	}
}
