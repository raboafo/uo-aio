using System;
using Ultima.Data;
using UOAIO.Profiles;

namespace UOAIO.Targeting;

public class ServerTargetHandler : BaseTargetHandler
{
	protected DateTime startTime;

	protected int targetID;

	protected bool canceledByServer;

	protected bool allowGround;

	protected AggressionType aggressionType;

	protected TargetAction action;

	public override AggressionType Aggression => this.aggressionType;

	public DateTime StartTime => this.startTime;

	public int TargetID => this.targetID;

	public bool AllowGround => this.allowGround;

	public TargetAction Action
	{
		get
		{
			return this.action;
		}
		set
		{
			this.action = value;
		}
	}

	public ServerTargetHandler(int targetID, bool allowGround, AggressionType aggressionType)
	{
		this.startTime = DateTime.Now;
		this.targetID = targetID;
		this.allowGround = allowGround;
		this.aggressionType = aggressionType;
	}

	protected override bool OnTarget(object targeted)
	{
		if (base.OnTarget(targeted))
		{
			return true;
		}
		return false;
	}

	protected override bool OnTarget(Mobile mob)
	{
		if (this.CheckQuery(mob))
		{
			Gumps.Desktop.Children.Add(new GCriminalTargetQuery(mob, this));
			return true;
		}
		if (this.ShouldBlock(mob))
		{
			return false;
		}
		this.Announce(mob);
		this.Dispatch(0, mob.Serial, mob.X, mob.Y, mob.Z, mob.Body & 0x3FFF);
		return true;
	}

	protected virtual bool CheckQuery(Mobile mob)
	{
		if (mob.Player)
		{
			return false;
		}
		Mobile player = World.Player;
		switch (Options.Current.NotorietyQuery)
		{
		default:
			return false;
		case NotoQueryType.Smart:
			if (mob.IsGuarded || player.IsGuarded)
			{
				break;
			}
			return false;
		case NotoQueryType.On:
			break;
		}
		if (base.IsOffensive)
		{
			return mob.Notoriety == Notoriety.Innocent;
		}
		if (base.IsDefensive && !Options.Current.SiegeRuleset)
		{
			return mob.Notoriety == Notoriety.Criminal || mob.Notoriety == Notoriety.Murderer;
		}
		return false;
	}

	protected virtual bool ShouldBlock(Mobile mob)
	{
		switch (this.action)
		{
		case TargetAction.Cure:
			if (Options.Current.ProtectCures && !mob.IsPoisoned)
			{
				return true;
			}
			break;
		case TargetAction.Poison:
			if (Options.Current.ProtectPoisons && mob.IsPoisoned)
			{
				return true;
			}
			break;
		case TargetAction.Heal:
		case TargetAction.GreaterHeal:
			if (Options.Current.ProtectHeals && mob.IsPoisoned)
			{
				return true;
			}
			break;
		case TargetAction.Bandage:
			if (Options.Current.ProtectBandages && GBandageTimer.Active)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public virtual void Announce(Mobile mob)
	{
		string actionName = this.GetActionName(mob);
		if (actionName != null)
		{
			string targetName = this.GetTargetName(mob);
			if (targetName != null)
			{
				Party.SendAutomatedMessage("Targeting {0} with {1}", targetName, actionName);
			}
		}
	}

	protected virtual string GetActionName(Mobile mob)
	{
		return TargetActions.GetName(this.action);
	}

	protected virtual string GetTargetName(Mobile mob)
	{
		if (mob.Player)
		{
			return "myself";
		}
		string identifier = mob.Identifier;
		if (identifier != null)
		{
			return identifier;
		}
		if (mob.HumanOrGhost)
		{
			return "someone";
		}
		return null;
	}

	protected override bool OnTarget(Item item)
	{
		int z = (item.InWorld ? item.Z : 0);
		this.Dispatch(0, item.Serial, item.X, item.Y, z, item.ID);
		return true;
	}

	protected override bool OnTarget(GroundTarget groundTarget)
	{
		if (this.allowGround)
		{
			this.Dispatch(1, 0, groundTarget.X, groundTarget.Y, groundTarget.Z, 0);
		}
		return this.allowGround;
	}

	protected unsafe override bool OnTarget(StaticTarget staticTarget)
	{
		ItemData* itemDataPointer = Map.GetItemDataPointer((ItemId)staticTarget.RealID);
		int num = staticTarget.Z;
		if ((itemDataPointer->Flags & TileFlag.Surface) != 0)
		{
			num += itemDataPointer->Height;
		}
		this.Dispatch(1, 0, staticTarget.X, staticTarget.Y, num, staticTarget.RealID);
		return true;
	}

	protected virtual void Dispatch(int type, int serial, int x, int y, int z, int id)
	{
		Network.Send(new PTarget_Response(type, this, serial, x, y, z, id));
	}

	public void Cancel(bool canceledByServer)
	{
		this.canceledByServer = canceledByServer;
		this.Cancel();
	}

	protected override void OnCancel()
	{
		if (!this.canceledByServer)
		{
			Network.Send(new PTarget_Cancel(this));
		}
	}
}
