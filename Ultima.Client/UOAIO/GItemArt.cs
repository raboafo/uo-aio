namespace UOAIO;

public class GItemArt : Gump, IClipable
{
	protected bool m_Draw;

	protected Texture m_Image;

	protected int m_Width;

	protected int m_Height;

	protected int m_ItemID;

	protected IHue m_Hue;

	protected VertexCache m_vCache;

	protected Clipper m_Clipper;

	public Clipper Clipper
	{
		get
		{
			return this.m_Clipper;
		}
		set
		{
			this.m_Clipper = value;
		}
	}

	public override int Width => this.m_Width;

	public override int Height => this.m_Height;

	public Texture Image => this.m_Image;

	public IHue Hue
	{
		get
		{
			return this.m_Hue;
		}
		set
		{
			this.m_Hue = value;
			this.m_Image = this.m_Hue.GetItem(this.m_ItemID);
			if (this.m_Image != null && !this.m_Image.IsEmpty())
			{
				this.m_Width = this.m_Image.Width;
				this.m_Height = this.m_Image.Height;
				this.m_Draw = true;
			}
			this.m_vCache.Invalidate();
		}
	}

	public GItemArt(int x, int y, int itemID)
		: this(x, y, itemID, Hues.Default)
	{
	}

	public GItemArt(int x, int y, int itemID, IHue hue)
		: base(x, y)
	{
		this.m_vCache = new VertexCache();
		this.m_Hue = hue;
		this.m_ItemID = itemID;
		this.m_Image = hue.GetItem(itemID);
		if (this.m_Image != null && !this.m_Image.IsEmpty())
		{
			this.m_Width = this.m_Image.Width;
			this.m_Height = this.m_Image.Height;
			this.m_Draw = true;
		}
	}

	protected internal override void Draw(int x, int y)
	{
		if (this.m_Draw)
		{
			if (this.m_Clipper == null)
			{
				this.m_vCache.Draw(this.m_Image, x, y);
			}
			else
			{
				this.m_Image.DrawClipped(x, y, this.m_Clipper);
			}
		}
	}
}
