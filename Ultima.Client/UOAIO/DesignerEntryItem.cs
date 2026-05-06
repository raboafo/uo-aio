namespace UOAIO;

public class DesignerEntryItem : ItemButtonGump
{
	private DesignerGump m_Designer;

	private DesignerEntry m_Entry;

	private int m_ClipX;

	private int m_ClipY;

	private int m_ClipWidth;

	private int m_ClipHeight;

	public int ClipX
	{
		get
		{
			return this.m_ClipX;
		}
		set
		{
			this.m_ClipX = value;
		}
	}

	public int ClipY
	{
		get
		{
			return this.m_ClipY;
		}
		set
		{
			this.m_ClipY = value;
		}
	}

	public int ClipWidth
	{
		get
		{
			return this.m_ClipWidth;
		}
		set
		{
			this.m_ClipWidth = value;
		}
	}

	public int ClipHeight
	{
		get
		{
			return this.m_ClipHeight;
		}
		set
		{
			this.m_ClipHeight = value;
		}
	}

	public DesignerEntryItem(DesignerGump designer, DesignerEntry entry, int x, int y, int ow, int oh)
		: base(x, y, entry.ID.DisplayID)
	{
		this.m_Designer = designer;
		this.m_Entry = entry;
		this.m_ClipWidth = ow;
		this.m_ClipHeight = oh;
	}

	public override void OnClick()
	{
		this.m_Designer.Context.Entry = this.m_Entry;
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return base.m_ItemID > 0 && X >= this.m_ClipX && X < this.m_ClipX + this.m_ClipWidth && Y >= this.m_ClipY && Y < this.m_ClipY + this.m_ClipHeight;
	}

	protected internal override void Draw(int X, int Y)
	{
		if (base.m_Draw && base.m_ItemID != 0)
		{
			base.m_Clipper = Clipper.TemporaryInstance(X + this.m_ClipX, Y + this.m_ClipY, this.m_ClipWidth, this.m_ClipHeight);
			base.m_Image.DrawClipped(X, Y, base.m_Clipper);
		}
	}
}
