using System;
using System.Collections.Generic;
using Ultima.Data;

namespace UOAIO;

public class DynamicItem : IAgentCell, IAgentView, ICell, IDisposable, IItem, ITile, IEntity
{
	public Item m_Item;

	private Texture m_LastImage;

	public IHue m_Hue;

	private IHue m_LastImageHue;

	public ushort m_ID;

	private ushort m_LastImageID;

	public byte m_Height;

	public sbyte m_Z;

	private static Type MyType;

	public ItemId ItemId => this.m_Item.ItemId;

	public Type CellType => DynamicItem.MyType;

	public byte CalcHeight
	{
		get
		{
			if (Map.m_ItemFlags[this.m_ID][TileFlag.Bridge])
			{
				return (byte)(this.m_Height / 2);
			}
			return this.m_Height;
		}
	}

	public PhysicalAgent Agent => this.m_Item;

	public List<ICell> Owner { get; set; }

	public int Serial => this.m_Item.Serial;

	public ushort ID => this.m_ID;

	public IHue Hue => this.m_Hue;

	public sbyte Z => this.m_Z;

	public sbyte SortZ
	{
		get
		{
			return this.m_Z;
		}
		set
		{
		}
	}

	public byte Height => this.m_Height;

	void IDisposable.Dispose()
	{
	}

	public Texture GetItem(IHue hue, ushort itemID)
	{
		if (this.m_LastImageHue != hue || this.m_LastImageID != itemID)
		{
			this.m_LastImageHue = hue;
			this.m_LastImageID = itemID;
			this.m_LastImage = hue.GetItem(itemID);
		}
		return this.m_LastImage;
	}

	public DynamicItem(Item i)
	{
		this.m_Item = i;
		this.Update();
	}

	public void OnAgentUpdated()
	{
		Map.Update(this.m_Item);
	}

	public void OnAgentDeleted()
	{
		Map.Update(this.m_Item);
	}

	public void Update()
	{
		this.m_ID = (ushort)this.m_Item.ID;
		this.m_Z = (sbyte)this.m_Item.Z;
		this.m_Hue = Hues.GetItemHue(this.m_ID, this.m_Item.Hue);
		this.m_Height = (byte)this.m_Item.Height;
	}

	static DynamicItem()
	{
		DynamicItem.MyType = typeof(DynamicItem);
	}
}
