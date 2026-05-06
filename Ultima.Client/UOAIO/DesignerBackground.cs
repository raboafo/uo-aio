using System.Windows.Forms;

namespace UOAIO;

public class DesignerBackground : GBackground
{
	private DesignContext m_Context;

	public DesignerBackground(DesignContext context, int gumpID, int x, int y, int width, int height)
		: base(gumpID, width, height, x, y, HasBorder: false)
	{
		this.m_Context = context;
		base.m_QuickDrag = true;
		base.m_CanDrag = true;
	}

	protected internal override void OnDragStart()
	{
		base.m_IsDragging = false;
		Gumps.Drag = null;
		Point point = base.PointToScreen(new Point(0, 0)) - base.m_Parent.PointToScreen(new Point(0, 0));
		base.m_Parent.m_OffsetX = point.X + base.m_OffsetX;
		base.m_Parent.m_OffsetY = point.Y + base.m_OffsetY;
		base.m_Parent.m_IsDragging = true;
		Gumps.Drag = base.m_Parent;
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.m_Parent.BringToTop();
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Network.Send(new PDesigner_Close(this.m_Context.House));
			DesignContext.Current = null;
		}
	}
}
