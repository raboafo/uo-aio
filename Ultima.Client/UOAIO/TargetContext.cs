using UOAIO.Targeting;

namespace UOAIO;

internal class TargetContext : UseContext
{
	protected object toTarget;

	protected ServerTargetHandler lastHandler;

	public virtual bool Spoof => false;

	public virtual AggressionType SpoofFlags => AggressionType.Neutral;

	public TargetContext(object toTarget)
		: base(null, isManual: false)
	{
		this.toTarget = toTarget;
	}

	public override void OnDispatch()
	{
		base.OnDispatch();
		if (this.Spoof)
		{
			PTarget_Spoof p = null;
			if (this.toTarget == null)
			{
				p = new PTarget_Spoof(0, 12648430, this.SpoofFlags, 0, -1, -1, 0, 0);
			}
			else if (this.toTarget is Mobile)
			{
				Mobile mobile = this.toTarget as Mobile;
				p = new PTarget_Spoof(0, 12648430, this.SpoofFlags, mobile.Serial, mobile.X, mobile.Y, mobile.Z, mobile.Body);
			}
			else if (this.toTarget is Item)
			{
				Item item = this.toTarget as Item;
				p = new PTarget_Spoof(0, 12648430, this.SpoofFlags, item.Serial, item.X, item.Y, item.Z, item.ID);
			}
			else if (this.toTarget is GroundTarget)
			{
				GroundTarget groundTarget = this.toTarget as GroundTarget;
				p = new PTarget_Spoof(1, 12648430, this.SpoofFlags, 0, groundTarget.X, groundTarget.Y, groundTarget.Z, 0);
			}
			else if (this.toTarget is StaticTarget)
			{
				StaticTarget staticTarget = this.toTarget as StaticTarget;
				p = new PTarget_Spoof(1, 12648430, this.SpoofFlags, 0, staticTarget.X, staticTarget.Y, staticTarget.Z, staticTarget.RealID);
			}
			Network.Send(p);
		}
	}

	public override void OnBegin()
	{
		this.lastHandler = TargetManager.Server;
	}

	public override void OnFinish()
	{
		ServerTargetHandler server = TargetManager.Server;
		if (server != null && server != this.lastHandler)
		{
			this.OnSuccess(server);
		}
	}

	public virtual void OnSuccess(ServerTargetHandler targetHandler)
	{
		if (!this.Spoof)
		{
			if (this.toTarget == null)
			{
				targetHandler.Cancel();
			}
			else
			{
				targetHandler.Target(this.toTarget);
			}
		}
		else
		{
			targetHandler.Clear();
		}
	}
}
