using System.Windows.Forms;

namespace UOAIO;

public class GBuyGump_InventoryItem : GRegion
{
	private GBuyGump m_Owner;

	private BuyInfo m_Info;

	private GItemArt m_Image;

	private GLabel m_Description;

	private GLabel m_Available;

	private GImage[] m_Separator;

	private bool m_Selected;

	private int m_xAvailable;

	private int m_yBase;

	public override Clipper Clipper
	{
		get
		{
			return base.m_Clipper;
		}
		set
		{
			base.m_Clipper = value;
			this.m_Image.Clipper = value;
			this.m_Description.Clipper = value;
			this.m_Available.Clipper = value;
			for (int i = 0; i < this.m_Separator.Length; i++)
			{
				this.m_Separator[i].Clipper = value;
			}
		}
	}

	public bool Selected
	{
		get
		{
			return this.m_Selected;
		}
		set
		{
			if (this.m_Selected != value)
			{
				this.m_Selected = value;
				GLabel description = this.m_Description;
				IHue hue = (this.m_Available.Hue = Hues.Load(value ? 1644 : 648));
				description.Hue = hue;
			}
		}
	}

	public int Available
	{
		get
		{
			return this.m_xAvailable;
		}
		set
		{
			if (this.m_xAvailable != value)
			{
				this.m_xAvailable = value;
				this.m_Available.Text = value.ToString();
				this.m_Available.X = 195 - this.m_Available.Image.xMax - 1;
				this.m_Available.Y = (base.m_Height - (this.m_Available.Image.yMax - this.m_Available.Image.yMin + 1)) / 2 - this.m_Available.Image.yMin;
			}
		}
	}

	public int Offset
	{
		set
		{
			base.m_Y = this.m_yBase + value;
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Left) != MouseButtons.None)
		{
			this.m_Owner.Selected = this.m_Info;
		}
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Gumps.Destroy(this.m_Owner);
		}
	}

	protected internal override void OnDoubleClick(int x, int y)
	{
		if (this.m_Info.ToBuy < this.m_Info.Amount)
		{
			if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
			{
				this.m_Info.ToBuy = this.m_Info.Amount;
			}
			else
			{
				BuyInfo info = this.m_Info;
				int toBuy = info.ToBuy + 1;
				info.ToBuy = toBuy;
			}
			if (this.m_Info.OfferedGump == null)
			{
				this.m_Info.OfferedGump = new GBuyGump_OfferedItem(this.m_Owner, this.m_Info);
				this.m_Owner.OfferMenu.Children.Add(this.m_Info.OfferedGump);
			}
			this.m_Info.OfferedGump.Amount = this.m_Info.ToBuy;
			this.Available = this.m_Info.Amount - this.m_Info.ToBuy;
			this.m_Owner.OfferMenu.UpdateTotal();
		}
	}

	public GBuyGump_InventoryItem(GBuyGump owner, BuyInfo si, int y, bool seperate)
		: base(32, y, 195, 0)
	{
		this.m_Owner = owner;
		this.m_yBase = y;
		this.m_Info = si;
		IFont uniFont = Engine.GetUniFont(3);
		IHue hue = Hues.Load(648);
		this.m_Image = new GItemArt(0, 0, si.ItemID, si.Hue);
		this.m_Description = new GWrappedLabel($"{si.Name} at {si.Price} gp", uniFont, hue, 58, 0, 105);
		this.m_Available = new GLabel(si.Amount.ToString(), uniFont, hue, 195, 0);
		int height = this.m_Image.Image.yMax - this.m_Image.Image.yMin + 1;
		base.m_Height = height;
		height = this.m_Description.Image.yMax - this.m_Description.Image.yMin + 1;
		if (height > base.m_Height)
		{
			base.m_Height = height;
		}
		height = this.m_Available.Image.yMax - this.m_Available.Image.yMin + 1;
		if (height > base.m_Height)
		{
			base.m_Height = height;
		}
		this.m_Image.X += (56 - (this.m_Image.Image.xMax - this.m_Image.Image.xMin + 1)) / 2;
		this.m_Image.Y += (base.m_Height - (this.m_Image.Image.yMax - this.m_Image.Image.yMin + 1)) / 2;
		this.m_Image.X -= this.m_Image.Image.xMin;
		this.m_Image.Y -= this.m_Image.Image.yMin;
		this.m_Description.X -= this.m_Description.Image.xMin;
		this.m_Description.Y += (base.m_Height - (this.m_Description.Image.yMax - this.m_Description.Image.yMin + 1)) / 2;
		this.m_Description.Y -= this.m_Description.Image.yMin;
		this.m_Available.X -= this.m_Available.Image.xMax + 1;
		this.m_Available.Y += (base.m_Height - (this.m_Available.Image.yMax - this.m_Available.Image.yMin + 1)) / 2;
		this.m_Available.Y -= this.m_Available.Image.yMin;
		base.m_Children.Add(this.m_Image);
		base.m_Children.Add(this.m_Description);
		base.m_Children.Add(this.m_Available);
		this.m_xAvailable = si.Amount;
		if (seperate)
		{
			this.m_Separator = new GImage[11];
			base.m_Children.Add(this.m_Separator[0] = new GImage(57, 0, base.m_Height));
			for (int i = 0; i < 9; i++)
			{
				base.m_Children.Add(this.m_Separator[i + 1] = new GImage(58, 30 + i * 16, base.m_Height));
			}
			base.m_Children.Add(this.m_Separator[10] = new GImage(59, 165, base.m_Height));
		}
		else
		{
			this.m_Separator = new GImage[0];
		}
		if (Engine.ServerFeatures.AOS)
		{
			base.Tooltip = new ItemTooltip(si.Item);
		}
	}
}
