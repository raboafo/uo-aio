using System;

namespace UOAIO;

public class BuyInfo : IComparable
{
	private Item m_Item;

	private string m_Name;

	private int m_Price;

	private double m_ToBuy;

	private GBuyGump_InventoryItem m_InventoryGump;

	private GBuyGump_OfferedItem m_OfferedGump;

	public GBuyGump_OfferedItem OfferedGump
	{
		get
		{
			return this.m_OfferedGump;
		}
		set
		{
			this.m_OfferedGump = value;
		}
	}

	public GBuyGump_InventoryItem InventoryGump
	{
		get
		{
			return this.m_InventoryGump;
		}
		set
		{
			this.m_InventoryGump = value;
		}
	}

	public int ToBuy
	{
		get
		{
			return (int)(this.m_ToBuy + 0.5);
		}
		set
		{
			this.m_ToBuy = value;
		}
	}

	public double dToBuy
	{
		get
		{
			return this.m_ToBuy;
		}
		set
		{
			this.m_ToBuy = value;
		}
	}

	public Item Item => this.m_Item;

	public int ItemID => this.m_Item.ID;

	public IHue Hue => Hues.GetItemHue(this.m_Item.ID, this.m_Item.Hue);

	public int Amount => this.m_Item.Amount;

	public int Price => this.m_Price;

	public string Name => this.m_Name;

	int IComparable.CompareTo(object x)
	{
		if (x == null)
		{
			return 1;
		}
		if (!(x is BuyInfo buyInfo))
		{
			throw new ArgumentException();
		}
		int num = Map.GetQuality(this.m_Item.ID).CompareTo(Map.GetQuality(buyInfo.m_Item.ID));
		if (num == 0)
		{
			num = this.m_Item.ID.CompareTo(buyInfo.m_Item.ID);
			if (num == 0)
			{
				num = this.m_Item.Serial.CompareTo(buyInfo.m_Item.Serial);
			}
		}
		return num;
	}

	public BuyInfo(Item item, int price, string name)
	{
		this.m_Item = item;
		this.m_Price = price;
		try
		{
			this.m_Name = Localization.GetString(Convert.ToInt32(name));
		}
		catch
		{
			this.m_Name = name;
		}
	}
}
