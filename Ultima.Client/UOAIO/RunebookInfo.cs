using UOAIO.Profiles;
using Veritas;

namespace UOAIO;

public class RunebookInfo : ItemRef
{
	public new static readonly PersistableType TypeCode;

	private RuneInfoCollection m_Runes;

	public override PersistableType TypeID => RunebookInfo.TypeCode;

	public RuneInfoCollection Runes => this.m_Runes;

	public bool IsValid
	{
		get
		{
			Mobile player = World.Player;
			if (player != null)
			{
				Item item = base.Find();
				if (item != null)
				{
					return item.InRange(player, 1);
				}
			}
			return false;
		}
	}

	private static PersistableObject Construct()
	{
		return new RunebookInfo();
	}

	private RunebookInfo()
		: this(null)
	{
	}

	public RunebookInfo(Item item)
		: base(item)
	{
		this.m_Runes = new RuneInfoCollection();
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.m_Runes.Count; i++)
		{
			this.m_Runes[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Runes.Add(ip.GetChild() as RuneInfo);
		}
	}

	static RunebookInfo()
	{
		RunebookInfo.TypeCode = new PersistableType("runebook", Construct, RuneInfo.TypeCode);
	}
}
