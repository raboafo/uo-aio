using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GMenuItem : GAlphaBackground
{
	private string m_Text;

	private Color m_DefaultColor;

	private Color m_OverColor;

	private Color m_ExpandedColor;

	private bool m_DropDown;

	private bool m_MakeTopmost;

	public bool MakeTopmost
	{
		get
		{
			return this.m_MakeTopmost;
		}
		set
		{
			this.m_MakeTopmost = value;
		}
	}

	public bool DropDown
	{
		get
		{
			return this.m_DropDown;
		}
		set
		{
			this.m_DropDown = value;
			this.Layout();
		}
	}

	public Color DefaultColor
	{
		get
		{
			return this.m_DefaultColor;
		}
		set
		{
			this.m_DefaultColor = value;
		}
	}

	public Color OverColor
	{
		get
		{
			return this.m_OverColor;
		}
		set
		{
			this.m_OverColor = value;
		}
	}

	public Color ExpandedColor
	{
		get
		{
			return this.m_ExpandedColor;
		}
		set
		{
			this.m_ExpandedColor = value;
		}
	}

	public string Text
	{
		get
		{
			return this.m_Text;
		}
		set
		{
			if (!(this.m_Text == value))
			{
				this.m_Text = value;
				GLabel gLabel = null;
				if (base.m_Children.Count > 0)
				{
					gLabel = base.m_Children[0] as GLabel;
				}
				if (gLabel != null)
				{
					gLabel.Text = this.m_Text;
					gLabel.Center();
					gLabel.X = 4 - gLabel.Image.xMin;
				}
			}
		}
	}

	public void SetHue(IHue hue)
	{
		GLabel gLabel = null;
		if (base.m_Children.Count > 0)
		{
			gLabel = base.m_Children[0] as GLabel;
		}
		if (gLabel != null)
		{
			gLabel.Hue = hue;
		}
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Left)
		{
			this.OnClick();
			if (base.GetType() != typeof(GMenuItem))
			{
				this.Unexpand();
			}
		}
	}

	public void Unexpand()
	{
		int fillColor = this.m_DefaultColor.ToArgb() & 0xFFFFFF;
		base.FillColor = fillColor;
		if (this.Width != 120)
		{
			this.Width = 120;
		}
		Gump[] array = base.m_Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is GMenuItem)
			{
				((GMenuItem)array[i]).Visible = false;
			}
		}
		if (base.m_Parent is GMenuItem)
		{
			((GMenuItem)base.m_Parent).Unexpand();
		}
	}

	protected internal override void Render(int rx, int ry)
	{
		int fillColor;
		bool flag;
		if (!(Gumps.LastOver is GMenuItem gMenuItem))
		{
			fillColor = this.m_DefaultColor.ToArgb() & 0xFFFFFF;
			flag = true;
		}
		else
		{
			GMenuItem gMenuItem2 = gMenuItem;
			while (gMenuItem2 != null && gMenuItem2 != this)
			{
				gMenuItem2 = gMenuItem2.Parent as GMenuItem;
			}
			flag = gMenuItem2 != this;
			fillColor = (flag ? (this.m_DefaultColor.ToArgb() & 0xFFFFFF) : ((gMenuItem != this) ? (this.m_ExpandedColor.ToArgb() & 0xFFFFFF) : (this.m_OverColor.ToArgb() & 0xFFFFFF)));
		}
		base.FillColor = fillColor;
		if (flag)
		{
			if (this.Width != 120)
			{
				this.Width = 120;
			}
			Gump[] array = base.m_Children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is GMenuItem)
				{
					((GMenuItem)array[i]).Visible = false;
				}
			}
		}
		else
		{
			bool flag2 = false;
			Gump[] array2 = base.m_Children.ToArray();
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j] is GMenuItem)
				{
					((GMenuItem)array2[j]).Visible = true;
					flag2 = true;
				}
			}
			int num = ((flag2 && !this.m_DropDown) ? 125 : 120);
			if (this.Width != num)
			{
				this.Width = num;
			}
			if (flag2 && this.m_MakeTopmost)
			{
				base.BringToTop();
			}
			this.Layout();
		}
		base.Render(rx, ry);
	}

	public bool Contains(GMenuItem child)
	{
		return base.m_Children.IndexOf(child) >= 0;
	}

	public void Add(GMenuItem child)
	{
		if (child != this && !this.Contains(child))
		{
			base.m_Children.Add(child);
			child.Visible = false;
			this.Layout();
		}
	}

	public void Remove(GMenuItem child)
	{
		if (child != this && this.Contains(child))
		{
			base.m_Children.Remove(child);
			child.Visible = false;
			this.Layout();
		}
	}

	public void Layout()
	{
		int num = 0;
		Gump[] array = base.m_Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is GMenuItem)
			{
				num++;
			}
		}
		int num2;
		int num3;
		if (this.m_DropDown)
		{
			num2 = 0;
			num3 = this.Height - 1;
		}
		else
		{
			Gump desktop = Gumps.Desktop;
			num2 = 124;
			num3 = 0;
			if (desktop != null)
			{
				int num4 = 1 + num * 23;
				Point p = base.PointToScreen(new Point(0, 0));
				p = desktop.PointToClient(p);
				int y = p.Y;
				int num5 = desktop.Height - y - 1;
				num5 /= 23;
				if (num5 < 1)
				{
					num5 = 1;
				}
				if (num5 < num)
				{
					num3 = this.Height - (num - num5 + 1) * 23 - 1;
				}
				if (y + num3 < 0)
				{
					num3 = -y;
				}
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] is GMenuItem gMenuItem2)
			{
				if (gMenuItem2.X != num2)
				{
					gMenuItem2.X = num2;
				}
				if (gMenuItem2.Y != num3)
				{
					gMenuItem2.Y = num3;
				}
				num3 += 23;
			}
		}
	}

	public virtual void OnClick()
	{
	}

	public GMenuItem(string text)
		: base(0, 50, 120, 24)
	{
		this.m_Text = text;
		this.m_DefaultColor = Color.FromArgb(192, 192, 192);
		this.m_OverColor = Color.FromArgb(32, 64, 128);
		this.m_ExpandedColor = Color.FromArgb(32, 64, 128);
		base.FillAlpha = 0.25f;
		base.m_CanDrag = false;
		GLabel gLabel = new GLabel(text, Engine.DefaultFont, Hues.Load(1153), 0, 0);
		base.m_Children.Add(gLabel);
		gLabel.Center();
		gLabel.X = 4 - gLabel.Image.xMin;
		base.m_NonRestrictivePicking = true;
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return !Engine.amMoving;
	}
}
