using UOAIO.Profiles;
using Veritas;

namespace UOAIO;

public class UseOnceAgent : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private ItemRefCollection m_Items;

	private int m_Index;

	public override PersistableType TypeID => UseOnceAgent.TypeCode;

	public ItemRefCollection Items => this.m_Items;

	public int Index
	{
		get
		{
			return this.m_Index;
		}
		set
		{
			this.m_Index = value;
		}
	}

	public ItemRef this[Item item]
	{
		get
		{
			if (item != null)
			{
				foreach (ItemRef item2 in this.m_Items)
				{
					if (item2.Serial == item.Serial)
					{
						return item2;
					}
				}
			}
			return null;
		}
	}

	private static PersistableObject Construct()
	{
		return new UseOnceAgent();
	}

	public UseOnceAgent()
	{
		this.m_Items = new ItemRefCollection();
	}

	public void Validate()
	{
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			Item item = this.m_Items[i].FindOnPlayer();
			if (item == null)
			{
				this.m_Items.RemoveAt(i--);
			}
		}
	}

	public void Use()
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			Item item = this.m_Items[(this.m_Index + i) % this.m_Items.Count].FindOnPlayer();
			if (item != null)
			{
				this.m_Index += i;
				item.Use();
				this.m_Index++;
				this.m_Index %= this.m_Items.Count;
				return;
			}
		}
		if (this.m_Items.Count == 0)
		{
			Engine.AddTextMessage("There are no items in your use-once list.");
		}
		else
		{
			Engine.AddTextMessage("No use-once items were found on your person.");
		}
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetInt32("index", this.m_Index);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Index = ip.GetInt32("index");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			this.m_Items[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Items.Add(ip.GetChild() as ItemRef);
		}
	}

	static UseOnceAgent()
	{
		UseOnceAgent.TypeCode = new PersistableType("useOnce", Construct, ItemRef.TypeCode);
	}
}
