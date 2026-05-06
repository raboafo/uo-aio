using System;
using System.Collections.Generic;
using UOAIO.Profiles;

namespace UOAIO.Targeting;

public static class TargetManager
{
	private static ClientTargetHandler clientHandler;

	private static ServerTargetHandler serverHandler;

	private static object lastTarget;

	private static Mobile lastOffensive;

	private static Mobile lastDefensive;

	private static object queuedTarget;

	private static object smartSentinel;

	public static BaseTargetHandler Active
	{
		get
		{
			if (TargetManager.clientHandler != null)
			{
				return TargetManager.clientHandler;
			}
			return TargetManager.serverHandler;
		}
	}

	public static bool IsActive => TargetManager.Active != null;

	public static ClientTargetHandler Client
	{
		get
		{
			return TargetManager.clientHandler;
		}
		set
		{
			if (TargetManager.clientHandler != value)
			{
				if (TargetManager.clientHandler != null)
				{
					TargetManager.clientHandler.Cancel();
				}
				TargetManager.clientHandler = value;
			}
		}
	}

	public static ServerTargetHandler Server
	{
		get
		{
			return TargetManager.serverHandler;
		}
		set
		{
			if (TargetManager.serverHandler != value)
			{
				if (TargetManager.serverHandler != null)
				{
					TargetManager.serverHandler.Cancel(canceledByServer: true);
				}
				TargetManager.serverHandler = value;
			}
		}
	}

	public static int Depth
	{
		get
		{
			int num = 0;
			if (TargetManager.clientHandler != null)
			{
				num++;
			}
			if (TargetManager.serverHandler != null)
			{
				num++;
			}
			return num;
		}
	}

	public static object LastTarget
	{
		get
		{
			return TargetManager.lastTarget;
		}
		set
		{
			TargetManager.lastTarget = value;
		}
	}

	public static Mobile LastOffensiveTarget
	{
		get
		{
			return TargetManager.lastOffensive;
		}
		set
		{
			TargetManager.lastOffensive = value;
		}
	}

	public static Mobile LastDefensiveTarget
	{
		get
		{
			return TargetManager.lastDefensive;
		}
		set
		{
			TargetManager.lastDefensive = value;
		}
	}

	public static bool IsQueued => TargetManager.queuedTarget != null;

	public static void ProcessQueue()
	{
		ServerTargetHandler server = TargetManager.Server;
		if (server == null || TargetManager.queuedTarget == null)
		{
			return;
		}
		if (TargetManager.queuedTarget == TargetManager.smartSentinel)
		{
			if (server.IsOffensive)
			{
				server.Target(TargetManager.lastOffensive);
			}
			else if (server.IsDefensive)
			{
				server.Target(TargetManager.lastDefensive);
			}
			else
			{
				server.Target(TargetManager.lastTarget);
			}
		}
		else
		{
			server.Target(TargetManager.queuedTarget);
		}
		TargetManager.queuedTarget = null;
	}

	public static void ClearQueue()
	{
		World.Player?.AddTextMessage("", "- cleared target queue -", Engine.DefaultFont, Hues.Load(89), unremovable: true);
		TargetManager.queuedTarget = null;
	}

	public static void QueueSmart()
	{
		TargetManager.Queue(TargetManager.smartSentinel);
	}

	public static void Queue(object toTarget)
	{
		if (toTarget == null)
		{
			return;
		}
		if (TargetManager.Active != null)
		{
			if (toTarget == TargetManager.smartSentinel)
			{
				TargetManager.TargetSmart();
			}
			else
			{
				TargetManager.Target(toTarget);
			}
		}
		else if (TargetManager.queuedTarget != toTarget)
		{
			object obj = toTarget;
			string text;
			if (toTarget != TargetManager.smartSentinel)
			{
				text = ((toTarget != World.Player) ? "Target queued" : "Target self queued");
			}
			else
			{
				text = "Smart target queued";
				obj = World.Player;
			}
			if (obj is Mobile)
			{
				(obj as Mobile).AddTextMessage("", $"- {text.ToLowerInvariant()} -", Engine.DefaultFont, Hues.Load(89), unremovable: false);
			}
			else if (obj is Item)
			{
				(obj as Item).AddTextMessage("", $"- {text.ToLowerInvariant()} -", Engine.DefaultFont, Hues.Load(89), unremovable: false);
			}
			else
			{
				Engine.AddTextMessage($"{text}.", Engine.DefaultFont, Hues.Load(89));
			}
			TargetManager.queuedTarget = toTarget;
		}
	}

	public static bool IsAcquirable(Mobile me, Mobile mob)
	{
		return TargetManager.IsAcquirable(me, mob, isBola: false);
	}

	public static bool IsAcquirable(Mobile me, Mobile mob, bool isBola)
	{
		if (mob.IsFriend || mob.IsInParty)
		{
			return false;
		}
		if (mob.Body == 987)
		{
			return false;
		}
		if (isBola && !mob.IsMounted)
		{
			return false;
		}
		switch (mob.Notoriety)
		{
		case Notoriety.Innocent:
			if (me.Notoriety == Notoriety.Murderer)
			{
				return !mob.IsGuarded;
			}
			return false;
		default:
			return true;
		case Notoriety.Ally:
		case Notoriety.Vendor:
			return false;
		}
	}

	public static void Reacquire()
	{
		Mobile mobile = TargetManager.Acquire(null);
		if (mobile != null)
		{
			TargetManager.LastTarget = mobile;
			TargetManager.LastOffensiveTarget = mobile;
			mobile.AddTextMessage("", "Last target set.", Engine.DefaultFont, Hues.Load(89), unremovable: false);
			string identifier = mobile.Identifier;
			if (identifier != null)
			{
				Party.SendAutomatedMessage("Changing last target to {0}", identifier);
			}
		}
	}

	public static Mobile Acquire(Predicate<Mobile> validator)
	{
		Mobile player = World.Player;
		if (player != null)
		{
			using ScratchList<Mobile> scratchList = new ScratchList<Mobile>();
			List<Mobile> value = scratchList.Value;
			bool isBola = false;
			int range = (Engine.ServerFeatures.AOS ? 11 : 12);
			if (TargetManager.Active is ServerTargetHandler { Action: TargetAction.Bola })
			{
				isBola = true;
				range = 8;
			}
			foreach (Mobile value2 in World.Mobiles.Values)
			{
				if (value2.Visible && !value2.Player && !value2.IsDead && TargetManager.IsAcquirable(player, value2, isBola) && player.InRange(value2, range) && Map.LineOfSight(player, value2) && (validator == null || validator(value2)))
				{
					value.Add(value2);
				}
			}
			if (value.Count > 0)
			{
				value.Sort(TargetSorter.Comparer);
				return value[0];
			}
		}
		return null;
	}

	public static void TargetSelf()
	{
		Mobile player = World.Player;
		if (player != null)
		{
			TargetManager.Target(player);
		}
	}

	public static void TargetAcquire()
	{
		Mobile mobile = TargetManager.Acquire(null);
		if (mobile != null)
		{
			TargetManager.Target(mobile);
		}
	}

	public static void TargetSmart()
	{
		if (TargetManager.IsActive)
		{
			if (TargetManager.Active.IsOffensive)
			{
				TargetManager.Target(TargetManager.lastOffensive);
			}
			else if (TargetManager.Active.IsDefensive)
			{
				TargetManager.Target(TargetManager.lastDefensive);
			}
			else
			{
				TargetManager.Target(TargetManager.lastTarget);
			}
		}
		else if (Options.Current.QueueTargets)
		{
			TargetManager.Queue(TargetManager.smartSentinel);
		}
	}

	public static void Target(object targeted)
	{
		if (targeted == null)
		{
			return;
		}
		if (TargetManager.IsActive)
		{
			BaseTargetHandler active = TargetManager.Active;
			if ((active is ServerTargetHandler && !TargetManager.TargetIsInRange(targeted)) || !active.Target(targeted))
			{
				return;
			}
			if (active is ServerTargetHandler)
			{
				TargetManager.queuedTarget = null;
			}
			Mobile mobile = targeted as Mobile;
			if (mobile != null && mobile.Player)
			{
				return;
			}
			TargetManager.lastTarget = targeted;
			if (mobile != null)
			{
				if (active is AcquireTargetHandler && (mobile.IsFriend || mobile.IsInParty))
				{
					TargetManager.lastDefensive = mobile;
				}
				else if (active.IsOffensive)
				{
					TargetManager.lastOffensive = mobile;
				}
				else if (active.IsDefensive)
				{
					TargetManager.lastDefensive = mobile;
				}
			}
		}
		else if (Options.Current.QueueTargets)
		{
			TargetManager.Queue(targeted);
		}
	}

	public static bool TargetIsInRange()
	{
		return TargetManager.TargetIsInRange(TargetManager.LastTarget);
	}

	public static bool TargetIsInRange(object targeted)
	{
		if (targeted == null || TargetManager.Server == null)
		{
			return false;
		}
		TargetAction action = TargetManager.Server.Action;
		if (action == TargetAction.Unknown)
		{
			return true;
		}
		int num = 0;
		switch (action)
		{
		case TargetAction.Resurrection:
		case TargetAction.Bandage:
		case TargetAction.Stealing:
			num = 1;
			break;
		case TargetAction.Bola:
			num = 8;
			break;
		default:
			num = 12;
			break;
		}
		Mobile player = World.Player;
		if (player == null)
		{
			return false;
		}
		if (targeted is Item && !((Item)targeted).InWorld)
		{
			Agent worldRoot = ((Item)targeted).WorldRoot;
			if (worldRoot == null)
			{
				return false;
			}
			if (player.InRange(worldRoot.X, worldRoot.Y, num))
			{
				return true;
			}
		}
		else if (targeted is IPoint3D)
		{
			IPoint3D point3D = (IPoint3D)targeted;
			if (player.InRange(point3D.X, point3D.Y, num))
			{
				return true;
			}
		}
		return false;
	}

	static TargetManager()
	{
		TargetManager.smartSentinel = new object();
	}
}
