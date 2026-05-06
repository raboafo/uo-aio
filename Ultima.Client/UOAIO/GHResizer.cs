using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class GHResizer : Gump
{
	protected IResizable m_Target;

	protected int m_xOffset;

	public override int Width => 6;

	public override int Height => this.m_Target.Height;

	public GHResizer(IResizable Target)
		: base(0, 0)
	{
		this.m_Target = Target;
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		Gumps.Capture = this;
		this.m_xOffset = X;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		Gumps.Capture = null;
	}

	protected internal override void OnMouseMove(int X, int Y, MouseButtons mb)
	{
		if (Gumps.Capture == this)
		{
			Point point = ((Gump)this.m_Target).PointToScreen(new Point(0, 0));
			Point point2 = base.PointToScreen(new Point(X, Y));
			int num = point2.X - point.X + 6 - this.m_xOffset;
			if (num < this.m_Target.MinWidth)
			{
				num = this.m_Target.MinWidth;
			}
			else if (num > this.m_Target.MaxWidth)
			{
				num = this.m_Target.MaxWidth;
			}
			this.m_Target.Width = num;
			Engine.Redraw();
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		base.m_X = this.m_Target.Width - 5;
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return !Engine.amMoving && !TargetManager.IsActive;
	}
}
