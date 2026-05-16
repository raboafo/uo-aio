using Veritas;

namespace UOAIO.Profiles;

public class Server : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private string m_Name;

	private IgnoreList m_IgnoreList;

	private TravelAgent m_TravelAgent;

	private PlayerCollection m_Players;

	public override PersistableType TypeID => Server.TypeCode;

	public string Name => this.m_Name;

	public IgnoreList IgnoreList => this.m_IgnoreList;

	public TravelAgent TravelAgent => this.m_TravelAgent;

	public Player this[Mobile mob]
	{
		get
		{
			Player player;
			for (int i = 0; i < this.m_Players.Count; i++)
			{
				player = this.m_Players[i];
				if (player.Serial == mob.Serial)
				{
					return player;
				}
			}
			this.m_Players.Add(player = new Player(mob));
			return player;
		}
	}

	private static PersistableObject Construct()
	{
		return new Server();
	}

	private Server()
	{
		this.m_Players = new PlayerCollection();
	}

	public Server(string name)
		: this()
	{
		this.m_Name = name;
		this.m_IgnoreList = new IgnoreList();
		this.m_TravelAgent = new TravelAgent();
	}

	internal void ApplyRuntimeState(ServerRuntimeState serverState, Player player)
	{
		this.m_IgnoreList = serverState?.IgnoreList ?? new IgnoreList();
		this.m_TravelAgent = serverState?.TravelAgent ?? new TravelAgent();
		this.m_Players.Clear();
		if (player != null)
		{
			this.m_Players.Add(player);
		}
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetString("name", this.m_Name);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Name = ip.GetString("name");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		this.m_IgnoreList.Serialize(op);
		this.m_TravelAgent.Serialize(op);
		foreach (Player player in this.m_Players)
		{
			player.Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			object child = ip.GetChild();
			if (child is Player)
			{
				this.m_Players.Add(child as Player);
			}
			else if (child is IgnoreList)
			{
				this.m_IgnoreList = child as IgnoreList;
			}
			else if (child is TravelAgent)
			{
				this.m_TravelAgent = child as TravelAgent;
			}
		}
		if (this.m_IgnoreList == null)
		{
			this.m_IgnoreList = new IgnoreList();
		}
		if (this.m_TravelAgent == null)
		{
			this.m_TravelAgent = new TravelAgent();
		}
	}

	static Server()
	{
		Server.TypeCode = new PersistableType("server", Construct, IgnoreList.TypeCode, TravelAgent.TypeCode, Player.TypeCode);
	}
}
