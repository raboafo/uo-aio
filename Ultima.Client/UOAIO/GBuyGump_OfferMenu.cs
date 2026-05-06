using System.Windows.Forms;

namespace UOAIO;

public class GBuyGump_OfferMenu : GImage
{
	private int m_xLast;

	private int m_yLast;

	private GBuyAccept m_Accept;

	private GBuyClear m_Clear;

	private GBuyGump m_Owner;

	private GLabel m_Total;

	private GLabel m_Signature;

	private int m_yOffset;

	private Clipper m_ContentClipper;

	private GVSlider m_Slider;

	private TimeSync m_SignatureAnimation;

	private int m_LastHeight;

	private int m_LastOffset;

	public Clipper ContentClipper => this.m_ContentClipper;

	public int yOffset
	{
		get
		{
			return this.m_yOffset;
		}
		set
		{
			this.m_yOffset = value;
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Gumps.Destroy(this.m_Owner);
		}
	}

	public void UpdateTotal()
	{
		this.m_Total.Text = this.m_Owner.ComputeTotalCost().ToString();
	}

	public void WriteSignature()
	{
		if (this.m_Signature != null)
		{
			this.m_Signature.Visible = true;
			this.m_SignatureAnimation = new TimeSync(0.5);
		}
	}

	public GBuyGump_OfferMenu(GBuyGump owner)
		: base(2161, 170, 214)
	{
		this.m_Owner = owner;
		Mobile player = World.Player;
		string name;
		if (player != null && (name = player.Name) != null && (name = name.Trim()).Length > 0)
		{
			this.m_Signature = new GLabel(name, Engine.GetFont(5), Hues.Load(1109), 72, 194);
			this.m_Signature.Visible = false;
			base.m_Children.Add(this.m_Signature);
		}
		base.m_Children.Add(new GLabel((player != null) ? player.Gold.ToString() : "0", Engine.GetFont(6), Hues.Default, 188, 167));
		this.m_Total = new GLabel("0", Engine.GetFont(6), Hues.Default, 68, 167);
		this.m_Accept = new GBuyAccept(owner);
		this.m_Clear = new GBuyClear(owner);
		base.m_Children.Add(this.m_Total);
		base.m_Children.Add(this.m_Accept);
		base.m_Children.Add(this.m_Clear);
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		GVSlider gVSlider = (this.m_Slider = new GVSlider(2088, 237, 81, 34, 58, 0.0, 0.0, 50.0, 1.0));
		base.m_Children.Add(gVSlider);
		base.m_Children.Add(new GHotspot(237, 66, 34, 84, gVSlider));
	}

	protected internal override void OnDragStart()
	{
		base.m_IsDragging = false;
		Gumps.Drag = null;
		Point point = base.PointToScreen(new Point(0, 0)) - this.m_Owner.PointToScreen(new Point(0, 0));
		this.m_Owner.m_OffsetX = point.X + base.m_OffsetX;
		this.m_Owner.m_OffsetY = point.Y + base.m_OffsetY;
		this.m_Owner.m_IsDragging = true;
		Gumps.Drag = this.m_Owner;
	}

	protected internal override bool HitTest(int x, int y)
	{
		if (base.m_Invalidated)
		{
			base.Refresh();
		}
		return base.m_Image.HitTest(x, y);
	}

	protected internal override void Render(int x, int y)
	{
		base.Render(x, y);
		int num = this.m_yOffset + this.m_LastOffset;
		num -= 67;
		if (num > 90)
		{
			this.m_LastHeight = num - 90;
		}
		else
		{
			this.m_LastHeight = -1;
		}
	}

	protected internal override void Draw(int x, int y)
	{
		if (this.m_xLast != x || this.m_yLast != y)
		{
			this.m_xLast = x;
			this.m_yLast = y;
			Clipper clipper = (this.m_ContentClipper = new Clipper(x + 32, y + 66, 196, 92));
			Gump[] array = base.m_Children.ToArray();
			foreach (Gump gump in array)
			{
				if (gump is GBuyGump_OfferedItem)
				{
					((GBuyGump_OfferedItem)gump).Clipper = clipper;
				}
			}
		}
		if (this.m_Signature != null && this.m_SignatureAnimation != null)
		{
			double normalized = this.m_SignatureAnimation.Normalized;
			if (normalized >= 1.0)
			{
				this.m_Signature.Scissor(null);
				this.m_SignatureAnimation = null;
			}
			else
			{
				this.m_Signature.Scissor(0, 0, (int)(normalized * (double)this.m_Signature.Width), this.m_Signature.Height);
			}
			Engine.Redraw();
		}
		if (this.m_LastHeight >= 0)
		{
			this.m_yOffset = 67 - (int)(this.m_Slider.GetValue() / 50.0 * (double)this.m_LastHeight);
		}
		else
		{
			this.m_yOffset = 67;
		}
		this.m_LastOffset = 67 - this.m_yOffset;
		base.Draw(x, y);
	}
}
