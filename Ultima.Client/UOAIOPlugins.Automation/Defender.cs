using System.Collections.Generic;
using UOAIO;
using UOAIO.Events;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

public class Defender
{
	public static int healValue;

	public static ServerTargetHandler serverTarget;

	public static Mobile self;

	public static List<Mobile> HighlightedMobiles;

	public static Mobile prioityTarget;

	public static readonly ActionCallback Macro_Callback;

	public static readonly CommandCallback Command_Callback;

	public static readonly CommandCallback HealRangeCommand_Callback;

	private static bool _isTicking;

	public static int HealValue
	{
		get
		{
			return Player.Current.AutomationOptions.AutoHealValue;
		}
		set
		{
			Player.Current.AutomationOptions.AutoHealValue = value;
		}
	}

	public static int HealRange
	{
		get
		{
			return Player.Current.AutomationOptions.AutoHealRange;
		}
		set
		{
			Player.Current.AutomationOptions.AutoHealRange = value;
		}
	}

	public static void Initialize()
	{
	}

	public static void ChangeHealValue(CommandArgs args)
	{
		int @int = args.GetInt32(0);
		if (@int <= 0 || @int >= 100)
		{
			Console.Print("Invalid value: specify a between from 0 and 100", Console.MessageType.Error);
		}
		else
		{
			Defender.HealValue = @int;
		}
	}

	private static void EnableDefender()
	{
		PacketHandlers.EventBus.Subscribe<TargetEvent>(handleTargetEvent);
	}

	private static void DisableDefender()
	{
		if (Defender._isTicking)
		{
			Engine.EventBus.Unsubscribe<EngineTickEvent>(handleEngineTickEvent);
			Defender._isTicking = false;
		}
		PacketHandlers.EventBus.Unsubscribe<TargetEvent>(handleTargetEvent);
	}

	private static void tDefender()
	{
	}

	static Defender()
	{
		Defender.healValue = 62;
		Defender.self = World.Player;
		Defender.HighlightedMobiles = new List<Mobile>();
		Defender.prioityTarget = World.Player;
		Defender.Macro_Callback = OnMacro;
		Defender.Command_Callback = ChangeHealValue;
		Defender.HealRangeCommand_Callback = ChangeHealRange;
	}

	public static bool OnMacro(string args)
	{
		switch (args)
		{
		case "On":
			Options.AutoHeal = true;
			Defender.EnableDefender();
			return true;
		case "Off":
			Options.AutoHeal = false;
			Defender.DisableDefender();
			return true;
		default:
			Options.AutoHeal = !Options.AutoHeal;
			if (Options.AutoHeal)
			{
				Defender.EnableDefender();
			}
			else
			{
				Defender.DisableDefender();
			}
			return true;
		}
	}

	public static void ChangeHealRange(CommandArgs args)
	{
		int @int = args.GetInt32(0);
		if (@int < 6 || @int > 18)
		{
			Console.Print("Invalid value: specify a value from 6 to 18", Console.MessageType.Error);
		}
		else
		{
			Defender.HealRange = @int;
		}
	}

	private static void handleTargetEvent(TargetEvent e)
	{
		if ((Defender.serverTarget = TargetManager.Server) != null)
		{
			TargetAction action = TargetManager.Server.Action;
			if ((action == TargetAction.Heal || action == TargetAction.Cure || action == TargetAction.GreaterHeal) && !Defender.DoHeal())
			{
				Engine.EventBus.Subscribe<EngineTickEvent>(handleEngineTickEvent);
				Defender._isTicking = true;
			}
		}
	}

	private static bool Heal(TargetAction action, Mobile mobile)
	{
		if (mobile != Defender.self && (!mobile.Visible || mobile.IsDead || !Map.LineOfSight(Defender.self, mobile)))
		{
			return false;
		}
		switch (action)
		{
		case TargetAction.Heal:
			if (mobile.CurrentHitPoints * 100 / mobile.MaximumHitPoints < 92 && !mobile.IsPoisoned)
			{
				mobile.OnTarget();
				return true;
			}
			return false;
		case TargetAction.Cure:
			if (mobile.IsPoisoned)
			{
				mobile.OnTarget();
				return true;
			}
			return false;
		case TargetAction.GreaterHeal:
			if (mobile.CurrentHitPoints <= Defender.HealValue && !mobile.IsPoisoned)
			{
				mobile.OnTarget();
				return true;
			}
			return false;
		default:
			return false;
		}
	}

	private static bool Heal(Mobile mobile)
	{
		if (mobile.Visible && !mobile.IsDead && !mobile.IsPoisoned && mobile.CurrentHitPoints * 100 / mobile.MaximumHitPoints <= Defender.HealValue && Defender.self.InRange(mobile, Defender.HealRange) && Map.LineOfSight(Defender.self, mobile))
		{
			mobile.OnTarget();
			return true;
		}
		return false;
	}

	public static bool DefendPlayer()
	{
		if ((Defender.serverTarget = TargetManager.Server) != null)
		{
			return Defender.Heal(Defender.serverTarget.Action, Defender.self);
		}
		return false;
	}

	public static bool DefendParty()
	{
		if ((Defender.serverTarget = TargetManager.Server) != null)
		{
			TargetAction action = Defender.serverTarget.Action;
			Mobile[] members = Party.Members;
			foreach (Mobile mobile in members)
			{
				if (Defender.Heal(action, mobile))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool DefendFriends()
	{
		if ((Defender.serverTarget = TargetManager.Server) != null)
		{
			TargetAction action = Defender.serverTarget.Action;
			foreach (Character character in Player.Current.Friends.Characters)
			{
				Mobile mobile = character.Find();
				if (mobile != null && Defender.Heal(action, mobile))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public static bool DefendGuild()
	{
		if ((Defender.serverTarget = TargetManager.Server) != null)
		{
			TargetAction action = Defender.serverTarget.Action;
			Mobile[] members = Guild.Members;
			foreach (Mobile mobile in members)
			{
				if (Defender.Heal(action, mobile))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool DoHeal()
	{
		if (!Defender.DefendPlayer() && !Defender.DefendGuild() && (Party.State == PartyState.Alone || !Defender.DefendParty()))
		{
			return Defender.DefendFriends();
		}
		return true;
	}

	private static void handleEngineTickEvent(EngineTickEvent e)
	{
		if (World.Player.IsDead || TargetManager.Server == null)
		{
			Engine.EventBus.Unsubscribe<EngineTickEvent>(handleEngineTickEvent);
			Defender._isTicking = false;
		}
		TargetAction action = TargetManager.Server.Action;
		if (action != TargetAction.Heal && action != TargetAction.Cure && action != TargetAction.GreaterHeal)
		{
			Engine.EventBus.Unsubscribe<EngineTickEvent>(handleEngineTickEvent);
			Defender._isTicking = false;
		}
		if (Defender.DoHeal())
		{
			Engine.EventBus.Unsubscribe<EngineTickEvent>(handleEngineTickEvent);
			Defender._isTicking = false;
		}
	}
}
