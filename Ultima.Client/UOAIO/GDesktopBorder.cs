using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GDesktopBorder : GBackground
{
	private enum BorderInteraction
	{
		None,
		Move,
		Resize
	}

	private const int ResizeHandleSize = 16;

	private const int ResizeHandleOffset = 3;

	public static GDesktopBorder Instance;

	private BorderInteraction m_Interaction;

	private Point m_MouseOrigin;

	private Rectangle m_ViewportOrigin;

	private bool m_SizeChanged;

	public GDesktopBorder()
		: base(Engine.GameX - Engine.GameBorderSize, Engine.GameY - Engine.GameBorderSize, Engine.GameWidth + Engine.GameBorderSize * 2, Engine.GameHeight + Engine.GameBorderSize * 2, 2700, 2700, 2700, 2701, 0, 2701, 2700, 2700, 2700)
	{
		GDesktopBorder.Instance = this;
		base.m_CanDrag = false;
		base.m_QuickDrag = false;
		base.m_NonRestrictivePicking = true;
		base.CanClose = false;
	}

	private Rectangle GetResizeHandleBounds()
	{
		return new Rectangle(base.Width - GDesktopBorder.ResizeHandleSize + GDesktopBorder.ResizeHandleOffset, base.Height - GDesktopBorder.ResizeHandleSize + GDesktopBorder.ResizeHandleOffset, GDesktopBorder.ResizeHandleSize, GDesktopBorder.ResizeHandleSize);
	}

	private void SyncToEngine()
	{
		base.X = Engine.GameX - Engine.GameBorderSize;
		base.Y = Engine.GameY - Engine.GameBorderSize;
		base.Width = Engine.GameWidth + Engine.GameBorderSize * 2;
		base.Height = Engine.GameHeight + Engine.GameBorderSize * 2;
	}

	private bool IsResizeHandle(int x, int y)
	{
		return this.GetResizeHandleBounds().Contains(x, y);
	}

	private void BeginInteraction(BorderInteraction interaction)
	{
		this.SyncToEngine();
		this.m_Interaction = interaction;
		this.m_MouseOrigin = new Point(Engine.m_xMouse, Engine.m_yMouse);
		this.m_ViewportOrigin = new Rectangle(Engine.GameX, Engine.GameY, Engine.GameWidth, Engine.GameHeight);
		this.m_SizeChanged = false;
		Gumps.Capture = this;
		Gumps.Focus = this;
	}

	private void EndInteraction()
	{
		Gumps.Capture = null;
		this.m_Interaction = BorderInteraction.None;
		if (this.m_SizeChanged)
		{
			Engine.RebuildViewportChrome();
		}
		else
		{
			this.SyncToEngine();
		}
	}

	protected internal override bool HitTest(int X, int Y)
	{
		if (Engine.amMoving)
		{
			return false;
		}
		return this.IsResizeHandle(X, Y) || (X >= 0 && Y >= 0 && (X < Engine.GameBorderSize || Y < Engine.GameBorderSize || X >= base.Width - Engine.GameBorderSize || Y >= base.Height - Engine.GameBorderSize));
	}

	protected internal override void OnDispose()
	{
		if (Gumps.Capture == this)
		{
			Gumps.Capture = null;
		}
		if (GDesktopBorder.Instance == this)
		{
			GDesktopBorder.Instance = null;
		}
		base.OnDispose();
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		if (mb != MouseButtons.Left || Engine.amMoving)
		{
			return;
		}
		this.BeginInteraction(this.IsResizeHandle(x, y) ? BorderInteraction.Resize : BorderInteraction.Move);
	}

	protected internal override void OnMouseMove(int x, int y, MouseButtons mb)
	{
		if (this.m_Interaction == BorderInteraction.None || (mb & MouseButtons.Left) == MouseButtons.None)
		{
			return;
		}
		Rectangle viewportOrigin = this.m_ViewportOrigin;
		int x2 = viewportOrigin.X;
		int y2 = viewportOrigin.Y;
		int width = viewportOrigin.Width;
		int height = viewportOrigin.Height;
		int x3 = Engine.m_xMouse - this.m_MouseOrigin.X;
		int y3 = Engine.m_yMouse - this.m_MouseOrigin.Y;
		if (this.m_Interaction == BorderInteraction.Resize)
		{
			width += x3;
			height += y3;
		}
		else
		{
			x2 += x3;
			y2 += y3;
		}
		int gameWidth = Engine.GameWidth;
		int gameHeight = Engine.GameHeight;
		Engine.ApplyGameViewportBounds(x2, y2, width, height, rebuildChrome: false);
		this.m_SizeChanged |= Engine.GameWidth != gameWidth || Engine.GameHeight != gameHeight;
		this.SyncToEngine();
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if (mb == MouseButtons.Left && this.m_Interaction != BorderInteraction.None)
		{
			this.EndInteraction();
		}
	}

	public void DoRender()
	{
		if (this.m_Interaction == BorderInteraction.None)
		{
			this.SyncToEngine();
		}
		base.Render(Engine.GameX - Engine.GameBorderSize - this.X, Engine.GameY - Engine.GameBorderSize - this.Y);
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		int num = X + base.Width - 5 + GDesktopBorder.ResizeHandleOffset;
		int num2 = Y + base.Height - 5 + GDesktopBorder.ResizeHandleOffset;
		Renderer.SetTexture(null);
		Renderer.PushAlpha(0.8f);
		for (int i = 0; i < 3; i++)
		{
			int num3 = i * 4;
			Renderer.SolidRect(16777215, num - num3, num2 - 1 - num3, 2, 2);
			Renderer.SolidRect(16777215, num - 1 - num3, num2 - num3, 2, 2);
		}
		Renderer.PopAlpha();
	}

	protected internal override void Render(int X, int Y)
	{
	}
}
