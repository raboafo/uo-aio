using System.Windows.Forms;

namespace UOAIO;

public class GHitspot : Gump, IClipable
{
	private int m_Width;

	private int m_Height;

	private bool m_WasDown;

	private OnClick m_Target;

	private Clipper m_Clipper;

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

	public GHitspot(int X, int Y, int Width, int Height, OnClick Target)
		: base(X, Y)
	{
		this.m_Width = Width;
		this.m_Height = Height;
		this.m_Target = Target;
	}

	protected internal override void Draw(int X, int Y)
	{
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return this.m_Clipper == null || this.m_Clipper.Evaluate(base.PointToScreen(new Point(X, Y)));
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		this.m_WasDown = true;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (this.m_WasDown && this.m_Target != null)
		{
			this.m_Target(this);
		}
		this.m_WasDown = false;
	}
}
