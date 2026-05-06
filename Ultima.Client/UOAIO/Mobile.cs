using System;
using System.Collections.Generic;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace UOAIO;

public class Mobile : PhysicalAgent, IMessageOwner, IPoint2D, IAnimationOwner, IRadarTrackable
{
	private class PartyAction : TargetContext
	{
		public override bool Spoof => true;

		public PartyAction(Mobile mob)
			: base(mob)
		{
		}

		public override void OnDispatch()
		{
			Network.Send(new PParty_AddMember());
			base.OnDispatch();
		}
	}

	public Chairs.ChairData ChairData = Chairs.ChairData.Null;

	public int SittingZ;

	private byte greenHealthLevel;

	private byte yellowHealthLevel;

	private byte redHealthLevel;

	private short m_XReal;

	private short m_YReal;

	private short m_ZReal;

	private byte m_DReal;

	public DateTime m_LastDamage;

	private bool m_OpenedStatus;

	private string guild;

	private Faction faction;

	private string guessedName;

	private string m_Name = "";

	private ushort m_Body;

	private byte m_Direction;

	private ushort m_Hue;

	private MobileFlags m_Flags;

	private bool m_IsMoving;

	private int m_MovedTiles;

	private int m_LastWalk;

	private bool m_Human;

	private bool m_Ghost;

	private bool m_HumanOrGhost;

	private Queue<WalkAnimation> m_Walking;

	private int m_CorpseSerial;

	private Notoriety m_Notoriety;

	private IMobileStatus m_StatusBar;

	private Animation m_Animation;

	private bool m_Refresh;

	private bool m_BigStatus;

	private int m_MessageFrame;

	private int m_MessageX;

	private int m_MessageY;

	private int m_ScreenX;

	private int m_ScreenY;

	private AnimationVertexCache m_vCache;

	private IHue m_hAnimationPool;

	private int m_iAnimationPool;

	private Frames m_fAnimationPool;

	private bool _isDeadPet;

	private int m_LightLevel;

	private int m_Props;

	private bool _meditating;

	public int m_KUOC_X;

	public int m_KUOC_Y;

	public int m_KUOC_Z;

	public int m_KUOC_F;

	private DateTime m_NextQueryProps;

	private int m_LastFrame = -12345;

	private int m_Sounds;

	private int m_HorseFootsteps;

	private int m_PaperdollX = int.MaxValue;

	private int m_PaperdollY = int.MaxValue;

	private bool m_IsIgnored;

	private List<Item> _sortedItems;

	private LayerComparer _sortComparer;

	private static string[] m_NotorietyStrings;

	private bool m_IsFriend;

	public string lastSpell;

	private static Layer[] m_DisturbLayers;

	private DateTime m_LastDisturb;

	private DateTime _radarExpirationTime;

	private MobileAttributes attributes;

	private bool _isUnfriended;

	public int AnimationId => this.m_Body;

	public int Speed
	{
		get
		{
			int num = 1;
			if (this.IsMounted)
			{
				num *= 2;
			}
			if (this.Body == 987)
			{
				num *= 4;
			}
			return num;
		}
	}

	public bool HasName
	{
		get
		{
			if (this.m_Name != null)
			{
				return this.m_Name.Length > 0;
			}
			return false;
		}
	}

	public bool IsInParty
	{
		get
		{
			if (Party.State == PartyState.Joined)
			{
				return Array.IndexOf(Party.Members, this) >= 0;
			}
			return false;
		}
	}

	public bool IsFriend
	{
		get
		{
			return this.m_IsFriend;
		}
		set
		{
			this.m_IsFriend = value;
			this._isUnfriended = !this.m_IsFriend;
		}
	}

	public bool IsDead
	{
		get
		{
			if (!this.Ghost)
			{
				return this.IsDeadPet;
			}
			return true;
		}
	}

	public bool IsMounted => this.FindEquip(Layer.Mount) != null;

	public bool IsSitting
	{
		get
		{
			if (this.AnimationId == 4 || this.IsMoving || this.IsMounted)
			{
				return false;
			}
			return this.ChairData.ItemID != Chairs.ChairData.Null.ItemID;
		}
	}

	public bool IsGuarded => World.Viewport.IsGuarded(Engine.m_World, base.X, base.Y);

	public bool IsPoisoned
	{
		get
		{
			if (!this.m_Flags[MobileFlag.Poisoned])
			{
				return this.greenHealthLevel > 0;
			}
			return true;
		}
	}

	public bool IsInvulnerable
	{
		get
		{
			if (!this.m_Flags[MobileFlag.YellowHits])
			{
				return this.yellowHealthLevel > 0;
			}
			return true;
		}
	}

	public short XReal
	{
		get
		{
			return this.m_XReal;
		}
		set
		{
			this.m_XReal = value;
		}
	}

	public short YReal
	{
		get
		{
			return this.m_YReal;
		}
		set
		{
			this.m_YReal = value;
		}
	}

	public short ZReal
	{
		get
		{
			return this.m_ZReal;
		}
		set
		{
			this.m_ZReal = value;
		}
	}

	public byte DReal
	{
		get
		{
			return this.m_DReal;
		}
		set
		{
			this.m_DReal = value;
		}
	}

	public string Guild
	{
		get
		{
			return this.guild;
		}
		set
		{
			if (value != null && value.Length == 0)
			{
				value = null;
			}
			this.guild = value;
			if (this.guild != null)
			{
				if (Party.CheckAutomatedAccept())
				{
					Network.Send(new PParty_Accept(Party.Leader));
				}
				else if (!this.Player && Party.CheckAutomatedInvite(this) && TargetManager.Server == null)
				{
					new PartyAction(this).Enqueue();
				}
			}
		}
	}

	public Faction Faction
	{
		get
		{
			return this.faction;
		}
		set
		{
			this.faction = value;
		}
	}

	public string GuessedName
	{
		get
		{
			return this.guessedName;
		}
		set
		{
			this.guessedName = value;
		}
	}

	public string Identifier
	{
		get
		{
			string name = this.m_Name;
			if (string.IsNullOrEmpty(name))
			{
				name = this.guessedName;
				if (string.IsNullOrEmpty(name))
				{
					return null;
				}
			}
			if (this.guild != null && this.faction != null)
			{
				return $"{name} [{this.guild}, {this.faction.Abbreviation}]";
			}
			if (this.guild != null)
			{
				return $"{name} [{this.guild}]";
			}
			if (this.faction != null)
			{
				return $"{name} [{this.faction.Abbreviation}]";
			}
			return name;
		}
	}

	public int LastSeen { get; set; }

	public bool Meditating
	{
		get
		{
			return this._meditating;
		}
		set
		{
			this._meditating = true;
		}
	}

	public int PropertyID
	{
		get
		{
			return this.m_Props;
		}
		set
		{
			if (this.m_Props != value)
			{
				this.m_NextQueryProps = DateTime.Now;
			}
			this.m_Props = value;
		}
	}

	public ObjectPropertyList PropertyList => ObjectPropertyList.Find(base.Serial, this.m_Props);

	public int LightLevel
	{
		get
		{
			if (Preferences.Current.Options.AlwaysLight)
			{
				return 100;
			}
			return this.m_LightLevel;
		}
		set
		{
			this.m_LightLevel = value;
		}
	}

	public bool IsDeadPet
	{
		get
		{
			return this._isDeadPet;
		}
		set
		{
			this._isDeadPet = value;
		}
	}

	public int ScreenX
	{
		get
		{
			return this.m_ScreenX;
		}
		set
		{
			this.m_ScreenX = value;
		}
	}

	public int ScreenY
	{
		get
		{
			return this.m_ScreenY;
		}
		set
		{
			this.m_ScreenY = value;
		}
	}

	public bool IsIgnored
	{
		get
		{
			return this.m_IsIgnored;
		}
		set
		{
			this.m_IsIgnored = value;
		}
	}

	public int MessageFrame
	{
		get
		{
			return this.m_MessageFrame;
		}
		set
		{
			this.m_MessageFrame = value;
		}
	}

	public int MessageX
	{
		get
		{
			return this.m_MessageX;
		}
		set
		{
			this.m_MessageX = value;
		}
	}

	public int MessageY
	{
		get
		{
			return this.m_MessageY;
		}
		set
		{
			this.m_MessageY = value;
		}
	}

	public int PaperdollX
	{
		get
		{
			return this.m_PaperdollX;
		}
		set
		{
			this.m_PaperdollX = value;
		}
	}

	public int PaperdollY
	{
		get
		{
			return this.m_PaperdollY;
		}
		set
		{
			this.m_PaperdollY = value;
		}
	}

	public bool Warmode
	{
		get
		{
			if (this.m_Flags[MobileFlag.Warmode])
			{
				return !this.Ghost;
			}
			return false;
		}
	}

	public Item MountItem => this.FindEquip(Layer.Mount);

	public Item Backpack => this.FindEquip(Layer.Backpack);

	public int HorseFootsteps
	{
		get
		{
			return this.m_HorseFootsteps;
		}
		set
		{
			this.m_HorseFootsteps = value;
		}
	}

	public int Sounds
	{
		get
		{
			return this.m_Sounds;
		}
		set
		{
			this.m_Sounds = value;
		}
	}

	public int LastFrame
	{
		get
		{
			return this.m_LastFrame;
		}
		set
		{
			this.m_LastFrame = value;
		}
	}

	public bool Player => base.Serial == World.Serial;

	public bool OpenedStatus
	{
		get
		{
			return this.m_OpenedStatus;
		}
		set
		{
			this.m_OpenedStatus = value;
		}
	}

	public bool BigStatus
	{
		get
		{
			return this.m_BigStatus;
		}
		set
		{
			this.m_BigStatus = value;
		}
	}

	public Queue<WalkAnimation> Walking => this.m_Walking;

	public int CorpseSerial
	{
		get
		{
			return this.m_CorpseSerial;
		}
		set
		{
			this.m_CorpseSerial = value;
		}
	}

	public Animation Animation
	{
		get
		{
			return this.m_Animation;
		}
		set
		{
			if (this.m_Animation != value)
			{
				if (this.m_Animation != null && this.m_Animation.OnAnimationEnd != null)
				{
					this.m_Animation.OnAnimationEnd(this.m_Animation, this);
				}
				this.m_Animation = value;
			}
		}
	}

	public Notoriety Notoriety
	{
		get
		{
			return this.m_Notoriety;
		}
		set
		{
			if (this.m_Notoriety != value)
			{
				this.m_Notoriety = value;
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnNotorietyChange(value);
				}
				if (this.Player && Engine.m_Ingame && (int)this.m_Notoriety >= 1 && (int)this.m_Notoriety <= 7)
				{
					Engine.AddTextMessage(Mobile.m_NotorietyStrings[(uint)(this.m_Notoriety - 1)], Engine.DefaultFont, Hues.GetNotoriety(this.m_Notoriety));
				}
			}
		}
	}

	public int NotorietyPriority
	{
		get
		{
			Mobile player = World.Player;
			bool flag = player != null && player.Notoriety == Notoriety.Murderer;
			switch (this.m_Notoriety)
			{
			case Notoriety.Innocent:
				if (!flag)
				{
					return 10;
				}
				return 90;
			case Notoriety.Attackable:
				return 95;
			case Notoriety.Criminal:
				return 50;
			case Notoriety.Enemy:
				return 100;
			case Notoriety.Murderer:
				if (!flag)
				{
					return 90;
				}
				return 10;
			default:
				return 0;
			}
		}
	}

	public IMobileStatus StatusBar
	{
		get
		{
			return this.m_StatusBar;
		}
		set
		{
			this.m_StatusBar = value;
		}
	}

	public GPaperdoll Paperdoll => (GPaperdoll)base.ContainerView;

	public MobileFlags Flags
	{
		get
		{
			return this.m_Flags;
		}
		set
		{
			this.InternalOnFlagsChanged(value);
		}
	}

	public bool IsMoving
	{
		get
		{
			return this.m_IsMoving;
		}
		set
		{
			this.m_IsMoving = value;
		}
	}

	public int MovedTiles
	{
		get
		{
			return this.m_MovedTiles;
		}
		set
		{
			this.m_MovedTiles = value;
		}
	}

	public int LastWalk
	{
		get
		{
			return this.m_LastWalk;
		}
		set
		{
			this.m_LastWalk = value;
		}
	}

	public byte Direction
	{
		get
		{
			return this.m_Direction;
		}
		set
		{
			this.m_Direction = value;
		}
	}

	public ushort Body
	{
		get
		{
			return this.m_Body;
		}
		set
		{
			if (this.m_Body != value)
			{
				this.m_Body = value;
				int bodyID = this.m_Body;
				Engine.m_Animations.Translate(ref bodyID);
				this.m_Human = bodyID == 400 || bodyID == 401 || bodyID == 991 || bodyID == 987 || bodyID == 990;
				this.m_Ghost = bodyID == 402 || bodyID == 403;
				this.m_HumanOrGhost = this.m_Human || this.m_Ghost;
			}
		}
	}

	public ushort Hue
	{
		get
		{
			return this.m_Hue;
		}
		set
		{
			if (this.m_Hue != value)
			{
				this.m_Hue = value;
				base.RaiseUpdateEvents();
			}
		}
	}

	public bool HumanOrGhost
	{
		get
		{
			if (!this.m_HumanOrGhost)
			{
				if (this.Player)
				{
					return Renderer.m_DeathOverride;
				}
				return false;
			}
			return true;
		}
	}

	public bool Ghost
	{
		get
		{
			if (!this.m_Ghost)
			{
				if (this.Player)
				{
					return Renderer.m_DeathOverride;
				}
				return false;
			}
			return true;
		}
	}

	public bool Human => this.m_Human;

	public string Name
	{
		get
		{
			return this.m_Name;
		}
		set
		{
			if (!(this.m_Name != value))
			{
				return;
			}
			if (!this.m_Refresh && this.m_StatusBar != null)
			{
				this.m_StatusBar.OnNameChange(value);
			}
			this.m_Name = value;
			if (this.Player)
			{
				string name = this.m_Name;
				string text;
				if (name == null || (name = name.Trim()).Length <= 0)
				{
					text = "Ultima Online";
				}
				else
				{
					text = "Ultima Online - " + name;
					UOAIO.Profiles.Player.Current.Name = name;
				}
				Engine.m_Display.Text = text;
			}
		}
	}

	public bool Refresh
	{
		get
		{
			return this.m_Refresh;
		}
		set
		{
			if (this.m_Refresh && !value && this.m_StatusBar != null)
			{
				this.m_StatusBar.OnRefresh();
			}
			this.m_Refresh = value;
		}
	}

	int IRadarTrackable.X
	{
		get
		{
			if (!this.Player && !base.Visible && !this.IsFriend)
			{
				return this.m_KUOC_X;
			}
			return base.X;
		}
	}

	int IRadarTrackable.Y
	{
		get
		{
			if (!this.Player && !base.Visible && !this.IsFriend)
			{
				return this.m_KUOC_Y;
			}
			return base.Y;
		}
	}

	string IRadarTrackable.Name
	{
		get
		{
			if (!this.Player && !this.IsInParty && !this.IsFriend && !WorldEx.IsGuildMember(this.guild))
			{
				return null;
			}
			string name = this.Name;
			if (string.IsNullOrEmpty(name))
			{
				if (this.IsFriend)
				{
					return "Friend - Unknown";
				}
				if (WorldEx.IsGuildMember(this.guild))
				{
					return "Guild - Unknown";
				}
			}
			return name;
		}
	}

	int IRadarTrackable.Facet
	{
		get
		{
			if (!this.Player && !base.Visible && !this.IsFriend)
			{
				return this.m_KUOC_F;
			}
			return Engine.m_World;
		}
	}

	int IRadarTrackable.Color
	{
		get
		{
			if (this.Player)
			{
				return 16777215;
			}
			if (!this.IsInParty && !this.IsFriend && !WorldEx.IsGuildMember(this.guild))
			{
				return 16737843;
			}
			return 3407718;
		}
	}

	bool IRadarTrackable.HasExpired
	{
		get
		{
			if (this.IsInParty || this.IsFriend)
			{
				return false;
			}
			if (this._isUnfriended)
			{
				return true;
			}
			if (this.Player || base.Visible)
			{
				this.UpdateRadarExpiration();
				return false;
			}
			return DateTime.UtcNow >= this._radarExpirationTime;
		}
	}

	public int BodyGender
	{
		get
		{
			switch (this.m_Body)
			{
			case 400:
			case 402:
				return 0;
			case 401:
			case 403:
				return 1;
			default:
				return this.Gender;
			}
		}
	}

	public int Gender
	{
		get
		{
			return this.QueryAttributes().Gender;
		}
		set
		{
			if (this.Gender != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnGenderChange(value);
				}
				this.EnsureAttributes().Gender = value;
			}
		}
	}

	public bool IsPet
	{
		get
		{
			return this.QueryAttributes().IsControlable;
		}
		set
		{
			this.EnsureAttributes().IsControlable = value;
		}
	}

	public int Strength
	{
		get
		{
			return this.QueryAttributes().Strength;
		}
		set
		{
			int strength = this.Strength;
			if (strength != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnStrChange(value);
				}
				if (this.Player && Engine.m_Ingame && strength != 0)
				{
					this.StatChange("strength", strength, value);
				}
				this.EnsureAttributes().Strength = value;
			}
		}
	}

	public int CurrentHitPoints
	{
		get
		{
			return this.QueryAttributes().CurrentHitPoints;
		}
		set
		{
			if (this.CurrentHitPoints != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnHPCurChange(value);
				}
				this.EnsureAttributes().CurrentHitPoints = value;
			}
		}
	}

	public int MaximumHitPoints
	{
		get
		{
			return this.QueryAttributes().MaximumHitPoints;
		}
		set
		{
			if (this.MaximumHitPoints != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnHPMaxChange(value);
				}
				this.EnsureAttributes().MaximumHitPoints = value;
			}
		}
	}

	public int Dexterity
	{
		get
		{
			return this.QueryAttributes().Dexterity;
		}
		set
		{
			int dexterity = this.Dexterity;
			if (dexterity != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnDexChange(value);
				}
				if (this.Player && Engine.m_Ingame && dexterity != 0)
				{
					this.StatChange("dexterity", dexterity, value);
				}
				this.EnsureAttributes().Dexterity = value;
			}
		}
	}

	public int CurrentStamina
	{
		get
		{
			return this.QueryAttributes().CurrentStamina;
		}
		set
		{
			if (this.CurrentStamina != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnStamCurChange(value);
				}
				this.EnsureAttributes().CurrentStamina = value;
			}
		}
	}

	public int MaximumStamina
	{
		get
		{
			return this.QueryAttributes().MaximumStamina;
		}
		set
		{
			if (this.MaximumStamina != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnStamMaxChange(value);
				}
				this.EnsureAttributes().MaximumStamina = value;
			}
		}
	}

	public int Intelligence
	{
		get
		{
			return this.QueryAttributes().Intelligence;
		}
		set
		{
			int intelligence = this.Intelligence;
			if (intelligence != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnIntChange(value);
				}
				if (this.Player && Engine.m_Ingame && intelligence != 0)
				{
					this.StatChange("intelligence", intelligence, value);
				}
				this.EnsureAttributes().Intelligence = value;
			}
		}
	}

	public int CurrentMana
	{
		get
		{
			return this.QueryAttributes().CurrentMana;
		}
		set
		{
			if (this.CurrentMana != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnManaCurChange(value);
				}
				this.EnsureAttributes().CurrentMana = value;
			}
		}
	}

	public int MaximumMana
	{
		get
		{
			return this.QueryAttributes().MaximumMana;
		}
		set
		{
			if (this.MaximumMana != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnManaMaxChange(value);
				}
				this.EnsureAttributes().MaximumMana = value;
			}
		}
	}

	public int StatCap
	{
		get
		{
			return this.QueryAttributes().StatCap;
		}
		set
		{
			if (this.StatCap != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnStatCapChange(value);
				}
				this.EnsureAttributes().StatCap = value;
			}
		}
	}

	public int FollowersCur
	{
		get
		{
			return this.QueryAttributes().CurrentFollowers;
		}
		set
		{
			if (this.FollowersCur != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnFollCurChange(value);
				}
				this.EnsureAttributes().CurrentFollowers = value;
			}
		}
	}

	public int FollowersMax
	{
		get
		{
			return this.QueryAttributes().MaximumFollowers;
		}
		set
		{
			if (this.FollowersMax != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnFollMaxChange(value);
				}
				this.EnsureAttributes().MaximumFollowers = value;
			}
		}
	}

	public int Armor
	{
		get
		{
			return this.QueryAttributes().Armor;
		}
		set
		{
			if (this.Armor != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnArmorChange(value);
				}
				this.EnsureAttributes().Armor = value;
			}
		}
	}

	public int Weight
	{
		get
		{
			return this.QueryAttributes().Weight;
		}
		set
		{
			if (this.Weight != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnWeightChange(value);
				}
				this.EnsureAttributes().Weight = value;
			}
		}
	}

	public int Gold
	{
		get
		{
			return this.QueryAttributes().Gold;
		}
		set
		{
			if (this.Gold != value)
			{
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnGoldChange(value);
				}
				this.EnsureAttributes().Gold = value;
			}
		}
	}

	public int Luck
	{
		get
		{
			return this.QueryAttributes().Luck;
		}
		set
		{
			if (this.Luck != value)
			{
				this.EnsureAttributes().Luck = value;
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnLuckChange();
				}
			}
		}
	}

	public int TithingPoints
	{
		get
		{
			return this.QueryAttributes().TithingPoints;
		}
		set
		{
			if (this.TithingPoints != value)
			{
				this.EnsureAttributes().TithingPoints = value;
			}
		}
	}

	public int DamageMin
	{
		get
		{
			return this.QueryAttributes().MinimumDamage;
		}
		set
		{
			if (this.DamageMin != value)
			{
				this.EnsureAttributes().MinimumDamage = value;
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnDamageChange();
				}
			}
		}
	}

	public int DamageMax
	{
		get
		{
			return this.QueryAttributes().MaximumDamage;
		}
		set
		{
			if (this.DamageMax != value)
			{
				this.EnsureAttributes().MaximumDamage = value;
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnDamageChange();
				}
			}
		}
	}

	public int FireResist
	{
		get
		{
			return this.QueryAttributes().FireResist;
		}
		set
		{
			if (this.FireResist != value)
			{
				this.EnsureAttributes().FireResist = value;
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnFireChange();
				}
			}
		}
	}

	public int ColdResist
	{
		get
		{
			return this.QueryAttributes().ColdResist;
		}
		set
		{
			if (this.ColdResist != value)
			{
				this.EnsureAttributes().ColdResist = value;
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnColdChange();
				}
			}
		}
	}

	public int PoisonResist
	{
		get
		{
			return this.QueryAttributes().PoisonResist;
		}
		set
		{
			if (this.PoisonResist != value)
			{
				this.EnsureAttributes().PoisonResist = value;
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnPoisonChange();
				}
			}
		}
	}

	public int EnergyResist
	{
		get
		{
			return this.QueryAttributes().EnergyResist;
		}
		set
		{
			if (this.EnergyResist != value)
			{
				this.EnsureAttributes().EnergyResist = value;
				if (!this.m_Refresh && this.m_StatusBar != null)
				{
					this.m_StatusBar.OnEnergyChange();
				}
			}
		}
	}

	public bool IsInGuild
	{
		get
		{
			if (this.guild != null)
			{
				return this.guild == World.Player.Guild;
			}
			return false;
		}
	}

	protected override IAgentCell CreateViewportCell()
	{
		return new MobileCell(this);
	}

	public void SetHealthLevel(int type, byte value)
	{
		switch (type)
		{
		case 1:
			this.greenHealthLevel = value;
			break;
		case 2:
			this.yellowHealthLevel = value;
			break;
		case 3:
			this.redHealthLevel = value;
			break;
		}
	}

	public void QueryProperties()
	{
		if (Engine.ServerFeatures.AOS && !(DateTime.Now < this.m_NextQueryProps))
		{
			this.m_NextQueryProps = DateTime.Now + TimeSpan.FromSeconds(1.0);
			Network.Send(new PQueryProperties(base.Serial));
		}
	}

	Frames IAnimationOwner.GetOwnedFrames(IHue hue, int realID)
	{
		if (this.m_iAnimationPool == realID && this.m_hAnimationPool == hue && !this.m_fAnimationPool.Disposed)
		{
			return this.m_fAnimationPool;
		}
		this.m_fAnimationPool = hue.GetAnimation(realID);
		this.m_hAnimationPool = hue;
		this.m_iAnimationPool = realID;
		return this.m_fAnimationPool;
	}

	public void Draw(Texture t, int x, int y)
	{
		if (this.m_vCache == null)
		{
			this.m_vCache = new AnimationVertexCache();
		}
		this.m_vCache.Draw(t, x, y);
	}

	public void DrawGame(Texture t, int x, int y, int color)
	{
		if (this.m_vCache == null)
		{
			this.m_vCache = new AnimationVertexCache();
		}
		this.m_vCache.DrawGame(t, x, y, color);
	}

	public void OnTarget()
	{
		TargetManager.Target(this);
	}

	public void OnSingleClick()
	{
		this.Look();
	}

	public void OnDoubleClick()
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		if (player.Flags[MobileFlag.Warmode] && !player.Ghost && !this.Ghost && !this.Player)
		{
			NotoQueryType notorietyQuery = Options.Current.NotorietyQuery;
			if (this.m_Notoriety == Notoriety.Innocent && (notorietyQuery == NotoQueryType.On || (notorietyQuery == NotoQueryType.Smart && (this.IsGuarded || player.IsGuarded))))
			{
				Gumps.Desktop.Children.Add(new GCriminalAttackQuery(this));
			}
			else
			{
				this.Attack();
			}
		}
		else
		{
			this.Use();
			PUseRequest.Last = this;
		}
	}

	public bool Attack()
	{
		return Network.Send(new PAttackRequest(this));
	}

	public bool Use()
	{
		return Network.Send(new PUseRequest(this));
	}

	public bool Look()
	{
		if (this.IsIgnored)
		{
			this.AddTextMessage("", "[Ignored]", Engine.GetFont(3), Hues.Load(946), unremovable: true);
		}
		return Network.Send(new PLookRequest(this));
	}

	public void Update()
	{
	}

	public bool QueryStats()
	{
		this.m_OpenedStatus = true;
		return Network.Send(new PQueryStats(base.Serial));
	}

	public Item FindEquip(Layer layer)
	{
		List<Item> items = base.Items;
		for (int i = 0; i < items.Count; i++)
		{
			Item item = items[i];
			if (item.Layer == layer)
			{
				return item;
			}
		}
		return null;
	}

	public bool HasEquipOnLayer(Layer check)
	{
		return this.FindEquip(check) != null;
	}

	public bool UsingTwoHandedWeapon()
	{
		Item item = this.FindEquip(Layer.TwoHanded);
		if (item != null)
		{
			int iD = item.ID;
			return iD < 7026 || iD > 7035 || iD < 7107 || iD > 7111;
		}
		return false;
	}

	public Item FindEquip(IItemValidator check)
	{
		List<Item> items = base.Items;
		for (int i = 0; i < items.Count; i++)
		{
			Item item = items[i];
			if (check.IsValid(item))
			{
				return item;
			}
		}
		return null;
	}

	public bool HasEquip(Item check)
	{
		if (check != null)
		{
			return check.Parent == this;
		}
		return false;
	}

	public void EquipRemoved()
	{
		GCombatGump.Update();
	}

	public void EquipChanged()
	{
		GCombatGump.Update();
		if (this.Player)
		{
			UOAIO.Profiles.Player.Current.EquipAgent.UpdateEquipment();
		}
	}

	protected override void OnChildRemoved(Agent child)
	{
		base.OnChildRemoved(child);
		if (child is Item)
		{
			Item item = (Item)child;
			if (this._sortedItems != null)
			{
				this._sortedItems.Remove(item);
			}
		}
	}

	protected override void OnLocationChanged()
	{
		base.OnLocationChanged();
		if (base.InWorld && base.Visible)
		{
			this.m_KUOC_X = base.X;
			this.m_KUOC_Y = base.Y;
			this.m_KUOC_Z = base.Z;
			this.m_KUOC_F = Engine.m_World;
		}
	}

	protected override void OnChildAdded(Agent child)
	{
		base.OnChildAdded(child);
		if (!(child is Item))
		{
			return;
		}
		Item item = (Item)child;
		if (this._sortedItems != null)
		{
			int num = this._sortedItems.BinarySearch(item, this._sortComparer);
			if (num < 0)
			{
				num = ~num;
			}
			this._sortedItems.Insert(num, item);
		}
	}

	public List<Item> GetSortedItems()
	{
		return this.GetSortedItems(this.m_Direction);
	}

	public List<Item> GetSortedItems(int direction)
	{
		if (this.CorpseSerial != 0)
		{
			Item item = World.FindItem(this.CorpseSerial);
			if (item != null && item.CorpseItems != null)
			{
				return item.GetSortedCorpseItems(this.m_Direction);
			}
		}
		LayerComparer layerComparer = LayerComparer.FromDirection(this.m_Direction);
		if (this._sortedItems == null)
		{
			this._sortedItems = new List<Item>(base.Items);
		}
		if (layerComparer != this._sortComparer)
		{
			this._sortComparer = layerComparer;
			this._sortedItems.Sort(this._sortComparer);
		}
		return this._sortedItems;
	}

	public void UpdateReal()
	{
		this.SetReal(base.X, base.Y, base.Z, this.m_Direction);
	}

	public void SetReal(int x, int y, int z, int d)
	{
		bool visible = base.Visible;
		this.m_XReal = (short)x;
		this.m_YReal = (short)y;
		this.m_ZReal = (short)z;
		this.m_DReal = (byte)d;
		bool visible2 = base.Visible;
		if (visible != visible2 && this.m_StatusBar != null && !this.m_Refresh)
		{
			this.m_StatusBar.OnRefresh();
		}
	}

	private void StatChange(string name, int oldValue, int newValue)
	{
		int num = newValue - oldValue;
		if (num != 0)
		{
			Engine.AddTextMessage(string.Format("Your {0} has {1} by {2}. It is now {3}.", name, (num > 0) ? "increased" : "decreased", Math.Abs(num), newValue), Engine.GetFont(3), Hues.Load(368));
		}
	}

	public Mobile(int serial)
		: base(serial)
	{
		this.m_Flags = new MobileFlags(this);
		this.m_Walking = new Queue<WalkAnimation>(0);
	}

	protected override void OnDeleted()
	{
		base.OnDeleted();
		if (Engine.m_Highlight == this)
		{
			Engine.m_Highlight = null;
		}
		this.greenHealthLevel = 0;
		this.yellowHealthLevel = 0;
		this.redHealthLevel = 0;
		this.IsDeadPet = false;
		bool flag = false;
		if (this.m_CorpseSerial != 0)
		{
			Item item = World.FindItem(this.m_CorpseSerial);
			if (item != null && item.CorpseSerial == base.Serial)
			{
				item.CorpseSerial = 0;
			}
		}
		if (this.StatusBar != null && !(this.StatusBar is GPartyHealthBar))
		{
			this.StatusBar.Close();
			this.StatusBar = null;
			this.OpenedStatus = false;
			flag = true;
		}
		else if (this.OpenedStatus)
		{
			this.OpenedStatus = false;
			flag = true;
		}
		if (this.StatusBar != null)
		{
			this.StatusBar.OnRefresh();
		}
		if (flag)
		{
			Network.Send(new PCloseStatus(this));
		}
	}

	internal void OnFlagsChanged()
	{
		this.InternalOnFlagsChanged(this.m_Flags);
	}

	private void InternalOnFlagsChanged(MobileFlags flags)
	{
		if (!this.m_Refresh && this.m_StatusBar != null)
		{
			this.m_StatusBar.OnFlagsChange(flags);
		}
		this.m_Flags = flags;
		if (this.Player && (this.m_Flags.Value & 0x80) == 0)
		{
			Engine.m_Stealth = false;
			Engine.m_StealthSteps = 0;
		}
	}

	public void AddTextMessage(string Name, string Message, IFont Font, IHue Hue, bool unremovable)
	{
		string text = ((Name.Length <= 0) ? Message : (Name + ": " + Message));
		if (Message.Length > 0)
		{
			Engine.AddToJournal(new JournalEntry(text, Hue, base.Serial));
			Message = Engine.WrapText(Message, 200, Font).TrimEnd();
			if (Message.Length > 0)
			{
				MessageManager.AddMessage(new GDynamicMessage(unremovable, this, Message, Font, Hue));
			}
		}
	}

	public void Disturb()
	{
		if (this.m_LastDisturb + TimeSpan.FromSeconds(0.5) >= DateTime.Now)
		{
			return;
		}
		this.m_LastDisturb = DateTime.Now;
		Mobile player = World.Player;
		Item backpack = player.Backpack;
		if (backpack == null)
		{
			return;
		}
		Item item = null;
		for (int i = 0; i < Mobile.m_DisturbLayers.Length; i++)
		{
			item = player.FindEquip(Mobile.m_DisturbLayers[i]);
			if (item != null)
			{
				break;
			}
		}
		if (item == null)
		{
			item = backpack.FindItem(new ItemIDValidator(3921, 3922));
		}
		if (item != null)
		{
			this.AddTextMessage(this.m_Name, "- disrupt -", Engine.DefaultFont, Hues.Load(53), unremovable: true);
			new EquipContext(item, item.Amount, this, clickFirst: false).Enqueue();
			return;
		}
		item = backpack.FindItem(new ItemIDValidator(2575, 2594, 2597, 2600));
		if (item != null)
		{
			this.AddTextMessage(this.m_Name, "- disrupt -", Engine.DefaultFont, Hues.Load(53), unremovable: true);
			item.Use();
		}
	}

	public void OpenStatus(bool Drag)
	{
		int x = 0;
		int y = 0;
		bool num = this.m_StatusBar != null;
		bool flag = num && Gumps.Drag == this.m_StatusBar.Gump;
		bool flag2 = num && Gumps.StartDrag == this.m_StatusBar.Gump;
		int offsetX = (num ? this.m_StatusBar.Gump.m_OffsetX : 0);
		int offsetY = (num ? this.m_StatusBar.Gump.m_OffsetY : 0);
		bool flag3 = !num || Drag;
		if (num)
		{
			x = this.m_StatusBar.Gump.X;
			y = this.m_StatusBar.Gump.Y;
			this.m_StatusBar.Close();
		}
		if (this.m_BigStatus)
		{
			this.m_StatusBar = new GStatusBar(this, x, y);
		}
		else if (Party.State == PartyState.Joined && Array.IndexOf(Party.Members, this) >= 0)
		{
			this.m_StatusBar = new GPartyHealthBar(this, x, y);
		}
		else
		{
			this.m_StatusBar = new GPartyHealthBar(this, x, y);
		}
		if (flag3)
		{
			this.m_StatusBar.Gump.X = Engine.m_xMouse - this.m_StatusBar.Gump.Width / 2;
			this.m_StatusBar.Gump.Y = Engine.m_yMouse - this.m_StatusBar.Gump.Height / 2;
		}
		if (flag || Drag)
		{
			if (Drag)
			{
				this.m_StatusBar.Gump.m_OffsetX = this.m_StatusBar.Gump.Width / 2;
				this.m_StatusBar.Gump.m_OffsetY = this.m_StatusBar.Gump.Height / 2;
			}
			else
			{
				this.m_StatusBar.Gump.m_OffsetX = offsetX;
				this.m_StatusBar.Gump.m_OffsetY = offsetY;
			}
			this.m_StatusBar.Gump.m_IsDragging = true;
			Gumps.Drag = this.m_StatusBar.Gump;
		}
		else if (flag2)
		{
			this.m_StatusBar.Gump.m_OffsetX = offsetX;
			this.m_StatusBar.Gump.m_OffsetY = offsetY;
			Gumps.StartDrag = this.m_StatusBar.Gump;
		}
		Gumps.Desktop.Children.Add(this.m_StatusBar.Gump);
		this.m_OpenedStatus = true;
	}

	public void UpdateRadarExpiration()
	{
		this._radarExpirationTime = DateTime.UtcNow + TimeSpan.FromSeconds(2.0);
	}

	protected MobileAttributes QueryAttributes()
	{
		return this.attributes ?? MobileAttributes.Default;
	}

	protected MobileAttributes EnsureAttributes()
	{
		if (this.attributes == null)
		{
			this.attributes = new MobileAttributes();
		}
		return this.attributes;
	}

	static Mobile()
	{
		Mobile.m_NotorietyStrings = new string[7] { "You are now innocent.", "You are now innocent.", "You may now be attacked freely, but are not a criminal.", "You are now a criminal.", "You are now innocent.", "You are now a murderer.", "You are now invulnerable." };
		Mobile.m_DisturbLayers = new Layer[6]
		{
			Layer.Ring,
			Layer.Bracelet,
			Layer.Earrings,
			Layer.Neck,
			Layer.Gloves,
			Layer.OuterLegs
		};
	}
}
