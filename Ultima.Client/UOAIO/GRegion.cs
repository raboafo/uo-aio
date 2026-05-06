namespace UOAIO;

public class GRegion : Gump, IClipable
{
	protected int m_Width;

	protected int m_Height;

	protected Clipper m_Clipper;

	public virtual Clipper Clipper
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

	public override int Width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
		}
	}

	public override int Height
	{
		get
		{
			return this.m_Height;
		}
		set
		{
			this.m_Height = value;
		}
	}

	public GRegion(int x, int y, int width, int height)
		: base(x, y)
	{
		this.m_Width = width;
		this.m_Height = height;
	}

	protected internal override bool HitTest(int x, int y)
	{
		return this.m_Clipper == null || this.m_Clipper.Evaluate(base.PointToScreen(new Point(x, y)));
	}
}
