using System;
using System.Windows.Forms;

namespace UOAIO;

public class GJournal : GAlphaBackground, IResizable
{
	protected int m_CropWidth;

	protected bool m_ToClose;

	protected GAlphaVSlider m_Scroller;

	protected GHotspot m_Hotspot;

	private static VertexCache m_vCache;

	public int MinWidth => 100;

	public int MaxWidth => Engine.ScreenWidth;

	public int MinHeight => 100;

	public int MaxHeight => Engine.ScreenHeight;

	public override int Width
	{
		get
		{
			return base.m_Width;
		}
		set
		{
			base.m_Width = value;
			this.m_CropWidth = base.m_Width - 24;
			GAlphaVSlider scroller = this.m_Scroller;
			int x = (this.m_Hotspot.X = base.m_Width - 19);
			scroller.X = x;
		}
	}

	public override int Height
	{
		get
		{
			return base.m_Height;
		}
		set
		{
			base.m_Height = value;
			double value2 = this.m_Scroller.GetValue();
			this.m_Hotspot.Height = base.m_Height - 8;
			this.m_Scroller.Height = base.m_Height - 19;
			this.m_Scroller.SetValue(value2, CallOnChange: false);
		}
	}

	public GJournal()
		: base(50, 50, 300, 188)
	{
		int num = Engine.m_Journal.Count - 1;
		if (num < 0)
		{
			num = 0;
		}
		base.m_Children.Add(new GVResizer(this));
		base.m_Children.Add(new GHResizer(this));
		base.m_Children.Add(new GLResizer(this));
		base.m_Children.Add(new GTResizer(this));
		base.m_Children.Add(new GHVResizer(this));
		base.m_Children.Add(new GLTResizer(this));
		base.m_Children.Add(new GHTResizer(this));
		base.m_Children.Add(new GLVResizer(this));
		this.m_Scroller = new GAlphaVSlider(0, 10, 16, 169, num, 0.0, num, 1.0);
		this.m_Hotspot = new GHotspot(0, 4, 16, 180, this.m_Scroller);
		this.m_Hotspot.NormalHit = false;
		base.m_Children.Add(this.m_Scroller);
		base.m_Children.Add(this.m_Hotspot);
		this.Width = 300;
		this.Height = 188;
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		base.BringToTop();
		if (mb == MouseButtons.Right)
		{
			this.m_ToClose = true;
		}
	}

	protected internal override void OnDispose()
	{
		Engine.m_JournalOpen = false;
		Engine.m_JournalGump = null;
		base.OnDispose();
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (this.m_ToClose)
		{
			Gumps.Destroy(this);
			Engine.m_JournalOpen = false;
			Engine.m_JournalGump = null;
		}
	}

	protected internal override void OnMouseWheel(int Delta)
	{
		base.BringToTop();
		this.m_Scroller.SetValue(this.m_Scroller.GetValue() + (double)(-Math.Sign(Delta)) * 5.0 * this.m_Scroller.Increase, CallOnChange: true);
	}

	protected internal override void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
		if (this.m_ToClose && mb != MouseButtons.Right)
		{
			this.m_ToClose = false;
		}
	}

	public void OnEntryAdded()
	{
		double value = this.m_Scroller.GetValue();
		this.m_Scroller.End = Engine.m_Journal.Count;
		if (value != (double)(Engine.m_Journal.Count - 1))
		{
			this.m_Scroller.SetValue(value, CallOnChange: false);
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		int x = X + 2;
		int num = base.m_Height - 2;
		int count = Engine.m_Journal.Count;
		int num2 = (int)this.m_Scroller.GetValue();
		if (num2 >= count)
		{
			num2 = count - 1;
		}
		UnicodeFont uniFont = Engine.GetUniFont(3);
		int num3 = num2;
		while (num3 >= 0 && num3 < count)
		{
			JournalEntry journalEntry = Engine.m_Journal[num3];
			Texture texture;
			if (journalEntry.Width != this.m_CropWidth)
			{
				string text = Engine.WrapText(journalEntry.Text, this.m_CropWidth, uniFont);
				texture = uniFont.GetString(text, journalEntry.Hue);
				journalEntry.Width = this.m_CropWidth;
				journalEntry.Image = texture;
			}
			else
			{
				texture = journalEntry.Image;
			}
			if (texture != null && !texture.IsEmpty())
			{
				num -= texture.Height;
				if (num < 3)
				{
					texture.DrawClipped(x, Y + num, Clipper.TemporaryInstance(X, Y + 1, this.Width, this.Height));
					break;
				}
				GJournal.m_vCache.Draw(texture, x, Y + num);
				num -= 4;
			}
			num3--;
		}
	}

	static GJournal()
	{
		GJournal.m_vCache = new VertexCache();
	}
}
