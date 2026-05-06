using System.Windows.Forms;

namespace UOAIO;

public class GSpellPlaceholder : Gump
{
	private int m_GameOffsetX;

	private int m_GameOffsetY;

	public const int Seperator = 4;

	public override int Width => 48;

	public override int Height => 48;

	public GSpellPlaceholder(int xOffset, int yOffset)
		: base(xOffset, yOffset)
	{
		this.m_GameOffsetX = xOffset - 2;
		this.m_GameOffsetY = yOffset - 2;
		base.m_CanDrop = true;
	}

	protected internal override void Render(int X, int Y)
	{
		this.X = Engine.GameX + this.m_GameOffsetX;
		this.Y = Engine.GameY + this.m_GameOffsetY;
		base.Render(X, Y);
	}

	protected internal override void OnDragDrop(Gump g)
	{
		if (g is GSpellIcon)
		{
			g.X = Engine.GameX + this.m_GameOffsetX + 2;
			g.Y = Engine.GameY + this.m_GameOffsetY + 2;
		}
	}

	protected internal override void OnMouseMove(int X, int Y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None && Engine.amMoving)
		{
			Point point = base.PointToScreen(new Point(X, Y));
			int distance = 0;
			Engine.movingDir = Engine.GetDirection(point.X, point.Y, ref distance);
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None && (Control.ModifierKeys & Keys.Shift) == 0)
		{
			Point point = base.PointToScreen(new Point(x, y));
			int distance = 0;
			Engine.movingDir = Engine.GetDirection(point.X, point.Y, ref distance);
			Engine.amMoving = true;
		}
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None && (Control.ModifierKeys & Keys.Shift) == 0)
		{
			Engine.amMoving = false;
		}
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return !Engine.amMoving;
	}

	protected internal override void Draw(int X, int Y)
	{
		if (Gumps.LastOver == this)
		{
			X++;
			Y++;
			Renderer.SetTexture(null);
			Renderer.PushAlpha(0.1f);
			Renderer.SolidRect(16777215, X, Y, this.Width - 4 + 2, this.Height - 4 + 2);
			Renderer.SetAlpha(0.6f);
			Renderer.TransparentRect(16777215, X, Y, this.Width - 4 + 2, this.Height - 4 + 2);
			Renderer.PopAlpha();
		}
	}
}
