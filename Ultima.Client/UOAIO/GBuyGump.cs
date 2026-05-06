using System;

namespace UOAIO;

public class GBuyGump : GDragable
{
	private int m_Serial;

	private BuyInfo[] m_Info;

	private BuyInfo m_Selected;

	private GBuyGump_OfferMenu m_OfferMenu;

	private int m_xLast;

	private int m_yLast;

	public GBuyGump_OfferMenu OfferMenu => this.m_OfferMenu;

	public BuyInfo Selected
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
			num += this.m_Info[i].ToBuy * this.m_Info[i].Price;
		}
		return num;
	}

	public void Accept()
	{
		Network.Send(new PBuyItems(this.m_Serial, this.m_Info));
		this.m_OfferMenu.WriteSignature();
	}

	public void Clear()
	{
		for (int i = 0; i < this.m_Info.Length; i++)
		{
			BuyInfo buyInfo = this.m_Info[i];
			if (buyInfo.ToBuy > 0)
			{
				buyInfo.ToBuy = 0;
				buyInfo.InventoryGump.Available = buyInfo.Amount;
				Gumps.Destroy(buyInfo.OfferedGump);
				buyInfo.OfferedGump = null;
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
				if (gump is GBuyGump_InventoryItem)
				{
					((GBuyGump_InventoryItem)gump).Clipper = clipper;
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
			if (gump is GBuyGump_InventoryItem)
			{
				((GBuyGump_InventoryItem)gump).Offset = offset;
			}
		}
	}

	public GBuyGump(int serial, BuyInfo[] info)
		: base(2160, 15, 15)
	{
		base.m_GUID = $"GBuyGump-{serial}";
		this.m_Serial = serial;
		this.m_Info = info;
		UnicodeFont uniFont = Engine.GetUniFont(3);
		IHue hue = Hues.Load(648);
		Array.Sort(info);
		int num = 66;
		for (int i = 0; i < info.Length; i++)
		{
			bool flag = i != info.Length - 1;
			BuyInfo buyInfo = info[i];
			GBuyGump_InventoryItem gBuyGump_InventoryItem = new GBuyGump_InventoryItem(this, buyInfo, num, flag);
			base.m_Children.Add(gBuyGump_InventoryItem);
			buyInfo.InventoryGump = gBuyGump_InventoryItem;
			num += gBuyGump_InventoryItem.Height;
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
		this.m_OfferMenu = new GBuyGump_OfferMenu(this);
		base.m_Children.Add(this.m_OfferMenu);
		base.m_X = (Engine.ScreenWidth - (this.m_OfferMenu.X + this.m_OfferMenu.Width)) / 2;
		base.m_Y = (Engine.ScreenHeight - (this.m_OfferMenu.Y + this.m_OfferMenu.Height)) / 2;
	}
}
