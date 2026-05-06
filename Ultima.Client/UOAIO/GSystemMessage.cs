using System;
using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GSystemMessage : GLabel, IMessage
{
	private TimeSync m_Dispose;

	private float m_SolidDuration;

	private Rectangle m_Rectangle;

	private Rectangle m_ImageRect;

	private DateTime m_UpdateTime;

	private int m_DupeCount;

	private string m_OrigText;

	private GLabel m_DupeLabel;

	public DateTime UpdateTime
	{
		get
		{
			return this.m_UpdateTime;
		}
		set
		{
			this.m_UpdateTime = value;
		}
	}

	public int DupeCount
	{
		get
		{
			return this.m_DupeCount;
		}
		set
		{
			this.m_DupeCount = value;
			if (this.m_DupeCount > 1)
			{
				string text = "(" + this.m_DupeCount.ToString("N0") + ")";
				if (this.m_DupeLabel == null)
				{
					this.m_DupeLabel = new GLabel(text, this.Font, this.Hue, 0, 0);
				}
				else
				{
					this.m_DupeLabel.Text = text;
				}
			}
		}
	}

	public Rectangle Rectangle => this.m_Rectangle;

	public Rectangle ImageRect => this.m_ImageRect;

	public string OrigText
	{
		get
		{
			return this.m_OrigText;
		}
		set
		{
			this.m_OrigText = value;
		}
	}

	public GSystemMessage(string text, IFont font, IHue hue, float duration)
		: base(text, font, hue, 0, 0)
	{
		base.m_OverridesCursor = false;
		this.m_SolidDuration = duration;
		this.m_Dispose = new TimeSync(this.m_SolidDuration + 1f);
		this.m_UpdateTime = DateTime.Now;
		this.m_DupeCount = 1;
		this.m_OrigText = text;
	}

	protected internal override bool HitTest(int x, int y)
	{
		if (base.m_Invalidated)
		{
			this.Refresh();
		}
		return base.m_Draw && base.m_Image.HitTest(x, y);
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Engine.amMoving = false;
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
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Point point = base.PointToScreen(new Point(x, y));
			int distance = 0;
			Engine.movingDir = Engine.GetDirection(point.X, point.Y, ref distance);
			Engine.amMoving = true;
		}
	}

	protected internal override void OnMouseEnter(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
	}

	protected internal override void Draw(int x, int y)
	{
		Gump[] array = Gumps.Desktop.Children.ToArray();
		float num = 1f;
		for (int i = Array.IndexOf(array, this) + 1; i < array.Length; i++)
		{
			Gump gump = array[i];
			if (gump is IMessage)
			{
				IMessage message = (IMessage)gump;
				if (message.Visible && message.ImageRect.IntersectsWith(this.m_ImageRect))
				{
					num += message.Alpha;
				}
			}
		}
		float alpha = this.Alpha;
		this.Alpha = (float)(1.0 / (double)num * (double)alpha);
		if (this.m_DupeLabel != null)
		{
			this.m_DupeLabel.Alpha = this.Alpha;
			this.m_DupeLabel.Draw(x + 5 + this.Width, y);
		}
		base.Draw(x, y);
		this.Alpha = alpha;
	}

	public Rectangle OnBeginRender()
	{
		double elapsed = this.m_Dispose.Elapsed;
		if (elapsed >= (double)(this.m_SolidDuration + 1f))
		{
			base.Visible = false;
			MessageManager.Remove(this);
			return this.m_ImageRect = (this.m_Rectangle = Rectangle.Empty);
		}
		if (elapsed >= (double)this.m_SolidDuration)
		{
			this.Alpha = (float)(1.0 - (elapsed - (double)this.m_SolidDuration));
		}
		base.Visible = true;
		this.X = Engine.GameX + 2;
		this.Y = MessageManager.yStack - this.Height;
		MessageManager.yStack = this.Y - 2;
		this.m_Rectangle.X = this.X;
		this.m_Rectangle.Y = this.Y;
		this.m_Rectangle.Width = this.Width;
		this.m_Rectangle.Height = this.Height;
		this.m_ImageRect.X = this.X + base.Image.xMin;
		this.m_ImageRect.Y = this.Y + base.Image.yMin;
		this.m_ImageRect.Width = base.Image.xMax - base.Image.xMin + 1;
		this.m_ImageRect.Height = base.Image.yMax - base.Image.yMin + 1;
		base.Scissor(new Clipper(Engine.GameX, Engine.GameY, Engine.GameWidth, Engine.GameHeight));
		return this.m_Rectangle;
	}
}
