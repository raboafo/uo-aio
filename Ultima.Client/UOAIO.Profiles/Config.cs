using System.IO;
using UOAIO;
using Veritas;

namespace UOAIO.Profiles;

public class Config : PersistableObject
{
	public static readonly PersistableType TypeCode;

	public static readonly Config Current;

	private ProfileList m_Profiles;

	private ServerList m_Servers;

	public override PersistableType TypeID => Config.TypeCode;

	public ProfileList Profiles => this.m_Profiles;

	public ServerList Servers => this.m_Servers;

	private static PersistableObject Construct()
	{
		return new Config();
	}

	public Config()
	{
		this.m_Profiles = new ProfileList();
		this.m_Servers = new ServerList();
		this.Load();
	}

	public void Load()
	{
		this.m_Profiles = new ProfileList();
		this.m_Servers = new ServerList();

		string profileName = ClientRuntimeEnvironment.ActiveProfileName;
		CharacterRuntimeState characterState = RuntimeProfileStores.LoadCharacterState();
		Profile profile = this.m_Profiles[profileName];
		profile.ApplyRuntimeState(RuntimeProfileStores.CreatePreferencesOrDefault(), characterState?.GuildRoster);

		string serverName = Engine.m_ServerName;
		if (string.IsNullOrWhiteSpace(serverName))
		{
			serverName = ClientRuntimeEnvironment.ServerName;
		}

		if (!string.IsNullOrWhiteSpace(serverName))
		{
			Player player = characterState?.Player;
			player?.SetProfileName(profileName);
			Server server = this.m_Servers[serverName];
			server.ApplyRuntimeState(RuntimeProfileStores.LoadServerState(), player);
		}
	}

	public void Save()
	{
		Profile profile = Profile.Current;
		RuntimeProfileStores.SavePreferences(profile.Preferences);

		string serverName = Engine.m_ServerName;
		if (string.IsNullOrWhiteSpace(serverName))
		{
			serverName = ClientRuntimeEnvironment.ServerName;
		}

		if (!string.IsNullOrWhiteSpace(serverName))
		{
			RuntimeProfileStores.SaveServerState(this.m_Servers[serverName]);
		}

		RuntimeProfileStores.SaveCharacterState(profile, Player.Current);
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		this.m_Profiles.Serialize(op);
		this.m_Servers.Serialize(op);
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			object child = ip.GetChild();
			if (child is ProfileList)
			{
				this.m_Profiles = child as ProfileList;
			}
			else if (child is ServerList)
			{
				this.m_Servers = child as ServerList;
			}
		}
	}

	static Config()
	{
		Config.TypeCode = new PersistableType("config", Construct, ProfileList.TypeCode, ServerList.TypeCode);
		Config.Current = new Config();
	}
}
