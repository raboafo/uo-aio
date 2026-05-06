using UOAIOPlugins.Automation;
using Veritas;

namespace UOAIO.Profiles;

public class Player : Character
{
	public new static readonly PersistableType TypeCode;

	private static Server m_Server;

	private static Player m_Player;

	private string m_Profile;

	private Friends m_Friends;

	private UseOnceAgent m_UseOnceAgent;

	private EquipAgent equipAgent;

	private GuildRoster m_GuildRoster;

	private static RuneInfoExCollection m_Runes;

	private AutomationOptions m_AutomationOptions;

	public override PersistableType TypeID => Player.TypeCode;

	public static Player Current
	{
		get
		{
			Mobile mobile = World.Player;
			string text = Engine.m_ServerName;
			if (mobile == null)
			{
				text = null;
			}
			if (text == null)
			{
				mobile = null;
			}
			if (Player.m_Server != null && Player.m_Server.Name != text)
			{
				Player.m_Server = null;
				Player.m_Player = null;
			}
			if (Player.m_Player != null && (mobile == null || Player.m_Player.Serial != mobile.Serial))
			{
				Player.m_Player = null;
			}
			if (Player.m_Player == null && mobile != null)
			{
				Player.m_Server = Config.Current.Servers[text];
				Player.m_Player = Player.m_Server[mobile];
				foreach (Character character in Player.m_Player.Friends.Characters)
				{
					World.WantMobile(character.Serial).IsFriend = true;
				}
				foreach (Character character2 in Player.m_Server.IgnoreList.Characters)
				{
					World.WantMobile(character2.Serial).IsIgnored = true;
				}
				foreach (ItemRef item in Player.m_Player.UseOnceAgent.Items)
				{
					World.WantItem(item.Serial).OverrideHue(34);
				}
			}
			return Player.m_Player;
		}
	}

	public Server Server => Player.m_Server;

	public string Profile => this.m_Profile;

	public Friends Friends => this.m_Friends;

	public IgnoreList IgnoreList => Player.m_Server.IgnoreList;

	public TravelAgent TravelAgent => Player.m_Server.TravelAgent;

	public UseOnceAgent UseOnceAgent => this.m_UseOnceAgent;

	public RestockAgent RestockAgent { get; private set; }

	public OrganizeAgent OrganizeAgent { get; private set; }

	public EquipAgent EquipAgent => this.equipAgent;

	public GuildRoster GuildRoster => this.m_GuildRoster;

	public static RuneInfoExCollection Runes
	{
		get
		{
			if (Player.m_Runes == null)
			{
				Player.m_Runes = new RuneInfoExCollection();
			}
			return Player.m_Runes;
		}
	}

	public AutomationOptions AutomationOptions => this.m_AutomationOptions;

	private static PersistableObject Construct()
	{
		return new Player();
	}

	private Player()
	{
	}

	public Player(Mobile mob)
		: base(mob)
	{
		this.m_Profile = "Default";
		this.m_Friends = new Friends();
		this.m_UseOnceAgent = new UseOnceAgent();
		this.equipAgent = new EquipAgent();
		this.RestockAgent = new RestockAgent();
		this.OrganizeAgent = new OrganizeAgent();
		this.m_GuildRoster = new GuildRoster();
		this.m_AutomationOptions = new AutomationOptions();
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		base.SerializeAttributes(op);
		op.SetString("profile", this.m_Profile);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		base.DeserializeAttributes(ip);
		this.m_Profile = ip.GetString("profile");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		this.m_Friends.Serialize(op);
		this.m_UseOnceAgent.Serialize(op);
		this.equipAgent.Serialize(op);
		this.RestockAgent.Serialize(op);
		this.OrganizeAgent.Serialize(op);
		this.AutomationOptions.Serialize(op);
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			object child = ip.GetChild();
			if (child is Friends)
			{
				this.m_Friends = child as Friends;
			}
			else if (!(child is TravelAgent))
			{
				if (child is UseOnceAgent)
				{
					this.m_UseOnceAgent = child as UseOnceAgent;
				}
				else if (child is EquipAgent)
				{
					this.equipAgent = child as EquipAgent;
				}
				else if (child is RestockAgent)
				{
					this.RestockAgent = child as RestockAgent;
				}
				else if (child is OrganizeAgent)
				{
					this.OrganizeAgent = child as OrganizeAgent;
				}
				else if (child is GuildRoster)
				{
					this.m_GuildRoster = child as GuildRoster;
				}
				else if (child is AutomationOptions)
				{
					this.m_AutomationOptions = child as AutomationOptions;
				}
			}
		}
		if (this.m_Friends == null)
		{
			this.m_Friends = new Friends();
		}
		if (this.m_UseOnceAgent == null)
		{
			this.m_UseOnceAgent = new UseOnceAgent();
		}
		if (this.equipAgent == null)
		{
			this.equipAgent = new EquipAgent();
		}
		if (this.RestockAgent == null)
		{
			this.RestockAgent = new RestockAgent();
		}
		if (this.OrganizeAgent == null)
		{
			this.OrganizeAgent = new OrganizeAgent();
		}
		if (this.m_GuildRoster == null)
		{
			this.m_GuildRoster = new GuildRoster();
		}
		if (this.m_AutomationOptions == null)
		{
			this.m_AutomationOptions = new AutomationOptions();
		}
	}

	static Player()
	{
		Player.TypeCode = new PersistableType("player", Construct, Friends.TypeCode, TravelAgent.TypeCode, UseOnceAgent.TypeCode, EquipAgent.TypeCode, RestockAgent.TypeCode, OrganizeAgent.TypeCode, GuildRoster.TypeCode, AutomationOptions.TypeCode);
	}
}
