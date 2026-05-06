using System;

namespace UOAIO;

public class GSellGump : GDragable
{
	private int m_Serial;

	private SellInfo[] m_Info;

	private SellInfo m_Selected;

	private GSellGump_OfferMenu m_OfferMenu;

	private int m_xLast;

	private int m_yLast;

	public GSellGump_OfferMenu OfferMenu => this.m_OfferMenu;

	public SellInfo Selected
	{
		get
		{
			return this.m_Selected;
		}
		set
		{
			if (this.m_Selected != value)
			{
				if (this.m_Selected != null)
				{
					this.m_Selected.InventoryGump.Selected = false;
				}
				this.m_Selected = value;
				this.m_Selected.InventoryGump.Selected = true;
			}
		}
	}

	public int ComputeTotalCost()
	{
		int num = 0;
		for (int i = 0; i < this.m_Info.Length; i++)
		{
			num += this.m_Info[i].ToSell * this.m_Info[i].Price;
		}
		return num;
	}

	public void Accept()
	{
		Network.Send(new PSellItems(this.m_Serial, this.m_Info));
		this.m_OfferMenu.WriteSignature();
	}

	public void Clear()
	{
		for (int i = 0; i < this.m_Info.Length; i++)
		{
			SellInfo sellInfo = this.m_Info[i];
			if (sellInfo.ToSell > 0)
			{
				sellInfo.ToSell = 0;
				sellInfo.InventoryGump.Available = sellInfo.Amount;
				Gumps.Destroy(sellInfo.OfferedGump);
				sellInfo.OfferedGump = null;
			}
		}
	}

	protected internal override void Draw(int x, int y)
	{
		if (this.m_xLast != x || this.m_yLast != y)
		{
			this.m_xLast = x;
			this.m_yLast = y;
			Clipper clipper = new Clipper(x + 31, y + 59, 197, 177);
			Gump[] array = base.m_Children.ToArray();
			foreach (Gump gump in array)
			{
				if (gump is GSellGump_InventoryItem)
				{
					((GSellGump_InventoryItem)gump).Clipper = clipper;
				}
			}
		}
		base.Draw(x, y);
	}

	private void Slider_OnValueChange(double v, double o, Gump slider)
	{
		Gump[] array = base.m_Children.ToArray();
		int offset = -(int)v;
		foreach (Gump gump in array)
		{
			if (gump is GSellGump_InventoryItem)
			{
				((GSellGump_InventoryItem)gump).Offset = offset;
			}
		}
	}

	public GSellGump(int serial, SellInfo[] info)
		: base(2162, 15, 15)
	{
		base.m_GUID = $"GSellGump-{serial}";
		this.m_Serial = serial;
		this.m_Info = info;
		UnicodeFont uniFont = Engine.GetUniFont(3);
		IHue hue = Hues.Load(648);
		Array.Sort(info);
		int num = 66;
		for (int i = 0; i < info.Length; i++)
		{
			bool flag = i != info.Length - 1;
			SellInfo sellInfo = info[i];
			GSellGump_InventoryItem gSellGump_InventoryItem = new GSellGump_InventoryItem(this, sellInfo, num, flag);
			base.m_Children.Add(gSellGump_InventoryItem);
			sellInfo.InventoryGump = gSellGump_InventoryItem;
			num += gSellGump_InventoryItem.Height;
			if (flag)
			{
				num += 16;
			}
		}
		if (num > 230)
		{
			GVSlider gVSlider = new GVSlider(2088, 237, 81, 34, 92, 0.0, 0.0, num - 230, 1.0)
			{
				OnValueChange = Slider_OnValueChange
			};
			base.m_Children.Add(gVSlider);
			base.m_Children.Add(new GHotspot(237, 66, 34, 122, gVSlider));
		}
		base.m_NonRestrictivePicking = true;
		this.m_OfferMenu = new GSellGump_OfferMenu(this);
		base.m_Children.Add(this.m_OfferMenu);
		base.m_X = (Engine.ScreenWidth - (this.m_OfferMenu.X + this.m_OfferMenu.Width)) / 2;
		base.m_Y = (Engine.ScreenHeight - (this.m_OfferMenu.Y + this.m_OfferMenu.Height)) / 2;
	}
}
