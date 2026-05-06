using Veritas;

namespace UOAIO.Profiles;

public class ServerList : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private ServerCollection m_Servers;

	public override PersistableType TypeID => ServerList.TypeCode;

	public Server this[string name]
	{
		get
		{
			Server server;
			for (int i = 0; i < this.m_Servers.Count; i++)
			{
				server = this.m_Servers[i];
				if (server.Name == name)
				{
					return server;
				}
			}
			this.m_Servers.Add(server = new Server(name));
			return server;
		}
	}

	private static PersistableObject Construct()
	{
		return new ServerList();
	}

	public ServerList()
	{
		this.m_Servers = new ServerCollection();
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.m_Servers.Count; i++)
		{
			this.m_Servers[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Servers.Add(ip.GetChild() as Server);
		}
	}

	static ServerList()
	{
		ServerList.TypeCode = new PersistableType("servers", Construct, Server.TypeCode);
	}
}
