using System;
using System.Collections.Generic;
using Ultima.Data;
using UOAIO.Profiles;

namespace UOAIO;

public sealed class StaticItem : IItem, ITile, ICell, IDisposable
{
	public int Serial;

	public int m_SortInfluence;

	private Texture m_LastImage;

	private IHue m_LastImageHue;

	public Texture m_sDraw;

	public IHue m_Hue;

	public TransformedColoredTextured[] m_vPool;

	private ushort m_LastImageID;

	public ushort m_ID;

	public ushort m_RealID;

	public byte m_Height;

	public float m_fAlpha;

	public sbyte m_Z;

	public bool m_bDraw;

	public bool m_bInit;

	private static Type MyType;

	private static Queue<StaticItem> m_InstancePool;

	public ItemId ItemId => (ItemId)this.m_ID;

	public unsafe ItemData* ItemDataPointer => Map.GetItemDataPointer(this.ItemId);

	public Type CellType => StaticItem.MyType;

	public int SortInfluence => this.m_SortInfluence;

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

	public unsafe TileFlag TileFlags => this.ItemDataPointer->Flags;

	public bool IsBridge => (this.TileFlags & TileFlag.Bridge) != 0;

	public byte CalcHeight
	{
		get
		{
			byte b = this.Height;
			if (this.IsBridge)
			{
				b /= 2;
			}
			return b;
		}
	}

	public byte Height => this.m_Height;

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

	public bool IsChair(out Chairs.ChairData data)
	{
		int iD = this.m_ID;
		if (Chairs.CheckItemAsChair(iD, out data))
		{
			Engine.AddTextMessage("Item is a fucking chair!");
			return true;
		}
		return false;
	}

	public static StaticItem Instantiate(HuedTile tile, int influence, int serial)
	{
		if (StaticItem.m_InstancePool.Count > 0)
		{
			StaticItem staticItem = StaticItem.m_InstancePool.Dequeue();
			staticItem.m_RealID = (ushort)tile.itemId;
			staticItem.m_ID = StaticItem.GetID(staticItem.m_RealID);
			staticItem.m_Z = tile.z;
			staticItem.m_Hue = Hues.GetItemHue(staticItem.m_ID, tile.hueId);
			staticItem.m_Height = Map.GetItemHeight(tile.itemId);
			staticItem.m_SortInfluence = influence;
			staticItem.Serial = serial;
			staticItem.m_LastImage = null;
			staticItem.m_LastImageHue = null;
			staticItem.m_LastImageID = 0;
			staticItem.m_fAlpha = 0f;
			staticItem.m_bDraw = false;
			staticItem.m_bInit = false;
			return staticItem;
		}
		return new StaticItem(tile, influence, serial);
	}

	public static StaticItem Instantiate(ushort itemID, ushort realID, sbyte z, int serial)
	{
		if (StaticItem.m_InstancePool.Count > 0)
		{
			StaticItem staticItem = StaticItem.m_InstancePool.Dequeue();
			staticItem.m_RealID = realID;
			staticItem.m_ID = StaticItem.GetID(itemID);
			staticItem.m_Z = z;
			staticItem.m_Hue = Hues.Default;
			staticItem.m_Height = Map.GetItemHeight((ItemId)realID);
			staticItem.m_SortInfluence = 0;
			staticItem.Serial = serial;
			staticItem.m_LastImage = null;
			staticItem.m_LastImageHue = null;
			staticItem.m_LastImageID = 0;
			staticItem.m_fAlpha = 0f;
			staticItem.m_bDraw = false;
			staticItem.m_bInit = false;
			return staticItem;
		}
		return new StaticItem(itemID, z, serial);
	}

	public static StaticItem Instantiate(ushort itemID, sbyte z, int serial)
	{
		if (StaticItem.m_InstancePool.Count > 0)
		{
			StaticItem staticItem = StaticItem.m_InstancePool.Dequeue();
			staticItem.m_RealID = itemID;
			staticItem.m_ID = StaticItem.GetID(itemID);
			staticItem.m_Z = z;
			staticItem.m_Hue = Hues.Default;
			staticItem.m_Height = Map.GetItemHeight((ItemId)staticItem.m_RealID);
			staticItem.m_SortInfluence = 0;
			staticItem.Serial = serial;
			staticItem.m_LastImage = null;
			staticItem.m_LastImageHue = null;
			staticItem.m_LastImageID = 0;
			staticItem.m_fAlpha = 0f;
			staticItem.m_bDraw = false;
			staticItem.m_bInit = false;
			return staticItem;
		}
		return new StaticItem(itemID, z, serial);
	}

	private static ushort GetID(ushort id)
	{
		if (Preferences.Current.Options.HideTrees && !Renderer.ScreenshotMode)
		{
			if (id < 14848 && Map.m_ItemFlags[id][TileFlag.Foliage])
			{
				return 1;
			}
			switch (id)
			{
			case 3230:
			case 3274:
			case 3275:
			case 3276:
			case 3277:
			case 3280:
			case 3283:
			case 3286:
			case 3288:
			case 3290:
			case 3293:
			case 3296:
			case 3299:
			case 3302:
			case 3476:
			case 3480:
			case 3484:
			case 3488:
			case 3492:
			case 3496:
				return 3673;
			default:
				return id;
			}
		}
		return id;
	}

	private StaticItem(HuedTile tile, int influence, int serial)
	{
		this.m_ID = (ushort)tile.itemId;
		this.m_Z = tile.z;
		this.m_RealID = this.m_ID;
		this.m_ID = StaticItem.GetID(this.m_ID);
		this.m_Hue = Hues.GetItemHue(this.m_ID, tile.hueId);
		this.m_Height = Map.GetItemHeight(tile.itemId);
		this.m_SortInfluence = influence;
		this.Serial = serial;
		this.m_vPool = VertexConstructor.Create();
	}

	private StaticItem(ushort ItemID, sbyte Z, int serial)
	{
		this.m_RealID = ItemID;
		this.m_ID = StaticItem.GetID(this.m_RealID);
		this.m_Z = Z;
		this.m_Height = Map.GetItemHeight((ItemId)this.m_RealID);
		this.m_Hue = Hues.Default;
		this.Serial = serial;
		this.m_vPool = VertexConstructor.Create();
	}

	void IDisposable.Dispose()
	{
		StaticItem.m_InstancePool.Enqueue(this);
	}

	static StaticItem()
	{
		StaticItem.MyType = typeof(StaticItem);
		StaticItem.m_InstancePool = new Queue<StaticItem>();
	}
}
