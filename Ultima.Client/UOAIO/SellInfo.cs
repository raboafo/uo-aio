using System;
using Ultima.Data;

namespace UOAIO;

public class SellInfo : IComparable
{
	private Item m_Item;

	private int m_ItemID;

	private IHue m_Hue;

	private int m_Amount;

	private int m_Price;

	private string m_Name;

	private double m_ToSell;

	private GSellGump_InventoryItem m_InventoryGump;

	private GSellGump_OfferedItem m_OfferedGump;

	public GSellGump_OfferedItem OfferedGump
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

	public GSellGump_InventoryItem InventoryGump
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

	public int ToSell
	{
		get
		{
			return (int)(this.m_ToSell + 0.5);
		}
		set
		{
			this.m_ToSell = value;
		}
	}

	public double dToSell
	{
		get
		{
			return this.m_ToSell;
		}
		set
		{
			this.m_ToSell = value;
		}
	}

	public Item Item => this.m_Item;

	public int ItemID => this.m_ItemID;

	public IHue Hue => this.m_Hue;

	public int Amount => this.m_Amount;

	public int Price => this.m_Price;

	public string Name => this.m_Name;

	int IComparable.CompareTo(object x)
	{
		if (x == null)
		{
			return 1;
		}
		if (!(x is SellInfo sellInfo))
		{
			throw new ArgumentException();
		}
		int num = Map.GetQuality(this.m_Item.ID).CompareTo(Map.GetQuality(sellInfo.m_Item.ID));
		if (num == 0)
		{
			num = this.m_Item.ID.CompareTo(sellInfo.m_Item.ID);
			if (num == 0)
			{
				num = this.m_Item.Serial.CompareTo(sellInfo.m_Item.Serial);
			}
		}
		return num;
	}

	public SellInfo(Item item, int itemID, int hue, int amount, int price, string name)
	{
		this.m_Item = item;
		this.m_ItemID = itemID;
		this.m_Amount = amount;
		this.m_Price = price;
		try
		{
			this.m_Name = Localization.GetString(Convert.ToInt32(name));
		}
		catch
		{
			this.m_Name = name;
		}
		if (!Map.m_ItemFlags[itemID][TileFlag.PartialHue])
		{
			hue ^= 0x8000;
		}
		this.m_Hue = Hues.Load(hue);
	}
}
