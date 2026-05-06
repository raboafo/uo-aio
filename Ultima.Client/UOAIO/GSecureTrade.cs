using System.Windows.Forms;

namespace UOAIO;

public class GSecureTrade : GAlphaBackground
{
	private int m_Serial;

	public Gump m_Container;

	private bool m_ShouldClose;

	public string Truncate(string text, IFont font, int width)
	{
		if (font.GetStringWidth(text) > width)
		{
			while (text.Length > 0 && font.GetStringWidth(text + "...") > width)
			{
				text = text.Substring(0, text.Length - 1);
			}
			text += "...";
		}
		return text;
	}

	public GSecureTrade(int serial, Gump container, string myName, string theirName)
		: base(50, 50, 281, 116)
	{
		this.m_Serial = serial;
		this.m_Container = container;
		base.m_CanDrop = true;
		base.FillAlpha = 0.5f;
		base.FillColor = 6324479;
		GBorder3D toAdd = new GBorder3D(inset: false, 0, 0, this.Width, this.Height)
		{
			FillAlpha = 0f,
			ShouldHitTest = false
		};
		base.m_Children.Add(toAdd);
		GBorder3D toAdd2 = new GBorder3D(inset: true, 6, 6, 132, 104)
		{
			FillAlpha = 0f,
			ShouldHitTest = false
		};
		base.m_Children.Add(toAdd2);
		GBorder3D gBorder3D = new GBorder3D(inset: false, 7, 7, 130, 20)
		{
			ShouldHitTest = false
		};
		GLabel gLabel = new GLabel(this.Truncate(myName, Engine.GetUniFont(1), gBorder3D.Width - 28), Engine.GetUniFont(1), Hues.Load(1), 0, 0);
		gBorder3D.Children.Add(gLabel);
		gLabel.Center();
		gLabel.X = 28 - gLabel.Image.xMin;
		base.m_Children.Add(gBorder3D);
		GBorder3D toAdd3 = new GBorder3D(inset: true, 143, 6, 132, 104)
		{
			FillAlpha = 0f,
			ShouldHitTest = false
		};
		base.m_Children.Add(toAdd3);
		GBorder3D gBorder3D2 = new GBorder3D(inset: false, 144, 7, 130, 20)
		{
			ShouldHitTest = false
		};
		GLabel gLabel2 = new GLabel(this.Truncate(theirName, Engine.GetUniFont(1), gBorder3D2.Width - 28), Engine.GetUniFont(1), Hues.Load(1), 0, 0);
		gBorder3D2.Children.Add(gLabel2);
		gLabel2.Center();
		gLabel2.X = gBorder3D2.Width - 28 - gLabel2.Image.xMax;
		base.m_Children.Add(gBorder3D2);
		GAlphaBackground toAdd4 = new GAlphaBackground(1, 1, 5, 114)
		{
			ShouldHitTest = false,
			BorderColor = 12632256,
			FillColor = 12632256,
			FillAlpha = 1f
		};
		base.m_Children.Add(toAdd4);
		toAdd4 = new GAlphaBackground(275, 1, 5, 114)
		{
			ShouldHitTest = false,
			BorderColor = 12632256,
			FillColor = 12632256,
			FillAlpha = 1f
		};
		base.m_Children.Add(toAdd4);
		toAdd4 = new GAlphaBackground(6, 1, 269, 5)
		{
			ShouldHitTest = false,
			BorderColor = 12632256,
			FillColor = 12632256,
			FillAlpha = 1f
		};
		base.m_Children.Add(toAdd4);
		toAdd4 = new GAlphaBackground(6, 110, 269, 5)
		{
			ShouldHitTest = false,
			BorderColor = 12632256,
			FillColor = 12632256,
			FillAlpha = 1f
		};
		base.m_Children.Add(toAdd4);
		toAdd4 = new GAlphaBackground(138, 6, 5, 104)
		{
			ShouldHitTest = false,
			BorderColor = 12632256,
			FillColor = 12632256,
			FillAlpha = 1f
		};
		base.m_Children.Add(toAdd4);
	}

	protected internal override void OnDragDrop(Gump g)
	{
		if (this.m_Container != null)
		{
			this.m_Container.OnDragDrop(g);
		}
	}

	public void Close()
	{
		Network.Send(new PCancelTrade(this.m_Serial));
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			this.m_ShouldClose = true;
		}
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if (this.m_ShouldClose && (mb & MouseButtons.Right) != MouseButtons.None)
		{
			this.Close();
		}
		this.m_ShouldClose = false;
	}

	protected internal override void OnMouseEnter(int x, int y, MouseButtons mb)
	{
		if (this.m_ShouldClose && (mb & MouseButtons.Right) == 0)
		{
			this.m_ShouldClose = false;
		}
	}
}
