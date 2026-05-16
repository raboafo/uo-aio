using System.Reflection;
using Veritas;

namespace UOAIO.Profiles;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Profile : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private static Player m_Player;

	private static Profile m_Current;

	private string m_Name;

	private Preferences m_Preferences;

	private GuildRoster m_GuildRoster;

	public override PersistableType TypeID => Profile.TypeCode;

	public static Profile Current
	{
		get
		{
			Player current = Player.Current;
			if (Profile.m_Current == null || current != Profile.m_Player)
			{
				Profile.m_Player = current;
				string profileName = (current == null) ? ClientRuntimeEnvironment.ActiveProfileName : current.Profile;
				Profile.m_Current = Config.Current.Profiles[profileName];
			}
			return Profile.m_Current;
		}
	}

	public string Name => this.m_Name;

	public Preferences Preferences => this.m_Preferences;

	public GuildRoster GuildRoster => this.m_GuildRoster;

	private static PersistableObject Construct()
	{
		return new Profile();
	}

	private Profile()
	{
	}

	public Profile(string name)
	{
		this.m_Name = name;
		this.m_Preferences = new Preferences();
		this.m_GuildRoster = new GuildRoster();
	}

	internal void ApplyRuntimeState(Preferences preferences, GuildRoster guildRoster)
	{
		this.m_Preferences = preferences ?? new Preferences();
		this.m_GuildRoster = guildRoster ?? new GuildRoster();
	}

	internal static void InvalidateCurrent()
	{
		Profile.m_Player = null;
		Profile.m_Current = null;
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
		this.m_Preferences.Serialize(op);
		this.m_GuildRoster.Serialize(op);
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			object child = ip.GetChild();
			if (child is Preferences)
			{
				this.m_Preferences = child as Preferences;
			}
			else if (child is GuildRoster)
			{
				this.m_GuildRoster = child as GuildRoster;
			}
			if (this.m_GuildRoster == null)
			{
				this.m_GuildRoster = new GuildRoster();
			}
		}
	}

	static Profile()
	{
		Profile.TypeCode = new PersistableType("profile", Construct, Preferences.TypeCode, GuildRoster.TypeCode);
	}
}
