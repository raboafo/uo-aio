using Veritas;

namespace UOAIO.Profiles;

public class ItemRef : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private int serial;

	private int itemId;

	public override PersistableType TypeID => ItemRef.TypeCode;

	public int Serial
	{
		get
		{
			return this.serial;
		}
		set
		{
			this.serial = value;
		}
	}

	public int ItemID
	{
		get
		{
			return this.itemId;
		}
		set
		{
			this.itemId = value;
		}
	}

	public bool IsNull => this.serial == 0 && this.itemId == 0;

	private static PersistableObject Construct()
	{
		return new ItemRef();
	}

	public Item Find()
	{
		Item item = World.FindItem(this.serial);
		if (item != null && (this.itemId == 0 || item.ID == this.itemId))
		{
			return item;
		}
		return null;
	}

	public Item FindOnPlayer()
	{
		Item item = this.Find();
		if (item != null)
		{
			Mobile player = World.Player;
			if (player == null || !item.IsChildOf(player))
			{
				item = null;
			}
		}
		return item;
	}

	protected ItemRef()
	{
	}

	public ItemRef(Item item)
	{
		if (item != null)
		{
			this.serial = item.Serial;
			this.itemId = item.ID;
		}
	}

	public ItemRef(int itemId)
	{
		this.itemId = itemId;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		if (!this.IsNull)
		{
			if (this.serial != 0)
			{
				op.SetInt32("serial", this.serial);
			}
			if (this.itemId != 0)
			{
				op.SetInt32("itemID", this.itemId);
			}
		}
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.serial = ip.GetInt32("serial");
		this.itemId = ip.GetInt32("itemID");
	}

	static ItemRef()
	{
		ItemRef.TypeCode = new PersistableType("item", Construct);
	}
}
