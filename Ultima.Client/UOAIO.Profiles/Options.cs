using System;
using System.Reflection;
using Veritas;

namespace UOAIO.Profiles;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Options : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private OptionFlag m_Flags;

	private NotoQueryType m_NotoQuery;

	private int m_HouseLevel;

	public override PersistableType TypeID => Options.TypeCode;

	public static Options Current => Profile.Current.Preferences.Options;

	[Optionable("Always Run", "Options", Default = false)]
	public bool AlwaysRun
	{
		get
		{
			return this[OptionFlag.AlwaysRun];
		}
		set
		{
			this[OptionFlag.AlwaysRun] = value;
		}
	}

	[Optionable("Incoming Names", "Options", Default = true)]
	public bool IncomingNames
	{
		get
		{
			return this[OptionFlag.IncomingNames];
		}
		set
		{
			this[OptionFlag.IncomingNames] = value;
		}
	}

	[Optionable("Notoriety Halos", "Options", Default = true)]
	public bool NotorietyHalos
	{
		get
		{
			return this[OptionFlag.NotorietyHalos];
		}
		set
		{
			this[OptionFlag.NotorietyHalos] = value;
		}
	}

	[Optionable("Protect Bandages", "Options", Default = true)]
	public bool ProtectBandages
	{
		get
		{
			return this[OptionFlag.ProtectBandages];
		}
		set
		{
			this[OptionFlag.ProtectBandages] = value;
		}
	}

	[Optionable("Protect Heals", "Options", Default = true)]
	public bool ProtectHeals
	{
		get
		{
			return this[OptionFlag.ProtectHeals];
		}
		set
		{
			this[OptionFlag.ProtectHeals] = value;
		}
	}

	[Optionable("Protect Cures", "Options", Default = true)]
	public bool ProtectCures
	{
		get
		{
			return this[OptionFlag.ProtectCures];
		}
		set
		{
			this[OptionFlag.ProtectCures] = value;
		}
	}

	[Optionable("Protect Poisons", "Options", Default = true)]
	public bool ProtectPoisons
	{
		get
		{
			return this[OptionFlag.ProtectPoisons];
		}
		set
		{
			this[OptionFlag.ProtectPoisons] = value;
		}
	}

	[Optionable("Siege Ruleset", "Options", Default = false)]
	public bool SiegeRuleset
	{
		get
		{
			return this[OptionFlag.SiegeRuleset];
		}
		set
		{
			this[OptionFlag.SiegeRuleset] = value;
		}
	}

	[Optionable("Queue Targets", "Options", Default = false)]
	public bool QueueTargets
	{
		get
		{
			return this[OptionFlag.QueueTargets];
		}
		set
		{
			this[OptionFlag.QueueTargets] = value;
		}
	}

	[Optionable("Enabled", "Scavenger", Default = true)]
	public bool Scavenger
	{
		get
		{
			return this[OptionFlag.Scavenger];
		}
		set
		{
			this[OptionFlag.Scavenger] = value;
		}
	}

	[Optionable("Screenshots", "Options", Default = true)]
	public bool Screenshots
	{
		get
		{
			return this[OptionFlag.Screenshots];
		}
		set
		{
			this[OptionFlag.Screenshots] = value;
		}
	}

	[Optionable("Health Icons", "Options", Default = true)]
	public bool MiniHealth
	{
		get
		{
			return this[OptionFlag.MiniHealth];
		}
		set
		{
			this[OptionFlag.MiniHealth] = value;
		}
	}

	[Optionable("Container Grid", "Options", Default = true)]
	public bool ContainerGrid
	{
		get
		{
			return this[OptionFlag.ContainerGrid];
		}
		set
		{
			this[OptionFlag.ContainerGrid] = value;
		}
	}

	[Optionable("Smooth Movement", "Options", Default = true)]
	public bool SmoothWalk
	{
		get
		{
			return this[OptionFlag.SmoothWalk];
		}
		set
		{
			this[OptionFlag.SmoothWalk] = value;
		}
	}

	[Optionable("Key Passthrough", "Options", Default = true)]
	public bool KeyPassthrough
	{
		get
		{
			return this[OptionFlag.KeyPassthrough];
		}
		set
		{
			this[OptionFlag.KeyPassthrough] = value;
		}
	}

	[Optionable("Moongate Confirmation", "Options", Default = false)]
	public bool MoongateConfirmation
	{
		get
		{
			return this[OptionFlag.MoongateConfirmation];
		}
		set
		{
			this[OptionFlag.MoongateConfirmation] = value;
		}
	}

	[Optionable("Always Light", "Options", Default = false)]
	public bool AlwaysLight
	{
		get
		{
			return this[OptionFlag.AlwaysLight];
		}
		set
		{
			this[OptionFlag.AlwaysLight] = value;
		}
	}

	[Optionable("Hotkeys Enabled", "Options", Default = true)]
	public bool HotkeysEnabled
	{
		get
		{
			return this[OptionFlag.HotkeysEnabled];
		}
		set
		{
			this[OptionFlag.HotkeysEnabled] = value;
		}
	}

	[Optionable("Clear Hands Before Cast", "Options", Default = false)]
	public bool ClearHandsBeforeCast
	{
		get
		{
			return this[OptionFlag.ClearHandsBeforeCast];
		}
		set
		{
			this[OptionFlag.ClearHandsBeforeCast] = value;
		}
	}

	[Optionable("Clear Hands Before Potion", "Options", Default = false)]
	public bool ClearHandsBeforePot
	{
		get
		{
			return this[OptionFlag.ClearHandsBeforePot];
		}
		set
		{
			this[OptionFlag.ClearHandsBeforePot] = value;
		}
	}

	[Optionable("Hide Trees", "Options", Default = false)]
	public bool HideTrees
	{
		get
		{
			return this[OptionFlag.HideTrees];
		}
		set
		{
			this[OptionFlag.HideTrees] = value;
		}
	}

	[Optionable("Party Notifications", "Options", Default = true)]
	public bool PartyNotifications
	{
		get
		{
			return this[OptionFlag.PartyNotifications];
		}
		set
		{
			this[OptionFlag.PartyNotifications] = value;
		}
	}

	public bool this[OptionFlag flag]
	{
		get
		{
			return (this.m_Flags & flag) == flag;
		}
		set
		{
			if (value)
			{
				this.m_Flags |= flag;
			}
			else
			{
				this.m_Flags &= ~flag;
			}
			string text = null;
			switch (flag)
			{
			case OptionFlag.AlwaysRun:
				text = "Always run is";
				break;
			case OptionFlag.ContainerGrid:
				text = "Container grids are";
				break;
			case OptionFlag.IncomingNames:
				text = "Incoming names are";
				break;
			case OptionFlag.MiniHealth:
				text = "Health icons are";
				break;
			case OptionFlag.NotorietyHalos:
				text = "Notoriety halos are";
				break;
			case OptionFlag.QueueTargets:
				text = "Target queueing is";
				break;
			case OptionFlag.Screenshots:
				text = "Death screenshots are";
				break;
			case OptionFlag.SmoothWalk:
				text = "Smooth movement is";
				break;
			case OptionFlag.Scavenger:
				text = "Scavenging is";
				break;
			case OptionFlag.KeyPassthrough:
				text = "Key passthrough is";
				break;
			case OptionFlag.MoongateConfirmation:
				text = "Moongate confirmation is";
				break;
			case OptionFlag.HotkeysEnabled:
				text = "Hotkeys are";
				break;
			case OptionFlag.ClearHandsBeforeCast:
				text = "Clear hands before cast is";
				break;
			case OptionFlag.HideTrees:
				text = "Tree hiding is";
				break;
			case OptionFlag.PartyNotifications:
				text = "Party notifications are";
				break;
			}
			if (text != null)
			{
				Engine.AddTextMessage(string.Format("{0} now {1}.", text, value ? "enabled" : "disabled"));
			}
		}
	}

	[Optionable("House Level", "Options", Default = 1)]
	public int HouseLevel
	{
		get
		{
			return this.m_HouseLevel;
		}
		set
		{
			this.m_HouseLevel = Math.Min(Math.Max(1, value), 5);
			Map.Invalidate();
		}
	}

	[Optionable("Notoriety Query", "Options", Default = NotoQueryType.On)]
	public NotoQueryType NotorietyQuery
	{
		get
		{
			return this.m_NotoQuery;
		}
		set
		{
			this.m_NotoQuery = value;
			string text = "Notoriety query is";
			if (text != null)
			{
				Engine.AddTextMessage($"{text} now {value.ToString().ToLower()}.");
			}
		}
	}

	private static PersistableObject Construct()
	{
		return new Options();
	}

	public Options()
	{
		this.m_Flags = OptionFlag.Default;
		this.m_NotoQuery = NotoQueryType.On;
		this.m_HouseLevel = 1;
	}

	internal void ApplyState(OptionFlag flags, NotoQueryType notoQuery, int houseLevel)
	{
		this.m_Flags = flags;
		this.m_NotoQuery = notoQuery;
		this.HouseLevel = houseLevel;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		OptionFlag flags = this.m_Flags;
		op.SetInt32("flags", (int)flags);
		op.SetInt32("notoQuery", (int)this.m_NotoQuery);
		op.SetInt32("houseLevel", this.m_HouseLevel);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Flags = (OptionFlag)ip.GetInt32("flags");
		this.m_NotoQuery = (NotoQueryType)ip.GetInt32("notoQuery");
		this.m_HouseLevel = Math.Min(Math.Max(1, ip.GetInt32("houseLevel")), 5);
		this.m_Flags |= OptionFlag.HotkeysEnabled;
		this.m_Flags |= OptionFlag.PartyNotifications;
	}

	static Options()
	{
		Options.TypeCode = new PersistableType("options", Construct);
	}
}
