using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class GVResizer : Gump
{
	protected IResizable m_Target;

	protected int m_yOffset;

	public override int Width => this.m_Target.Width;

	public override int Height => 6;

	public GVResizer(IResizable Target)
		: base(0, 0)
	{
		this.m_Target = Target;
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		Gumps.Capture = this;
		this.m_yOffset = Y;
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
			int num = point2.Y - point.Y + 6 - this.m_yOffset;
			if (num < this.m_Target.MinHeight)
			{
				num = this.m_Target.MinHeight;
			}
			else if (num > this.m_Target.MaxHeight)
			{
				num = this.m_Target.MaxHeight;
			}
			this.m_Target.Height = num;
			Engine.Redraw();
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		base.m_Y = this.m_Target.Height - 5;
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return !Engine.amMoving && !TargetManager.IsActive;
	}
}
