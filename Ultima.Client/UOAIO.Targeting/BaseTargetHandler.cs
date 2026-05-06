namespace UOAIO.Targeting;

public abstract class BaseTargetHandler
{
	private bool hasActed;

	public virtual AggressionType Aggression => AggressionType.Neutral;

	public bool IsOffensive => this.Aggression == AggressionType.Offensive;

	public bool IsDefensive => this.Aggression == AggressionType.Defensive;

	public bool IsNeutral => this.Aggression == AggressionType.Neutral;

	public virtual void Clear()
	{
		if (TargetManager.Server == this)
		{
			TargetManager.Server = null;
		}
		if (TargetManager.Client == this)
		{
			TargetManager.Client = null;
		}
	}

	public virtual bool Target(object targeted)
	{
		if (!this.hasActed && this.OnTarget(targeted))
		{
			this.hasActed = true;
			this.Clear();
			return true;
		}
		return false;
	}

	protected virtual bool OnTarget(object targeted)
	{
		if (targeted is Mobile mob)
		{
			return this.OnTarget(mob);
		}
		if (targeted is Item item)
		{
			return this.OnTarget(item);
		}
		if (targeted is GroundTarget groundTarget)
		{
			return this.OnTarget(groundTarget);
		}
		if (targeted is StaticTarget staticTarget)
		{
			return this.OnTarget(staticTarget);
		}
		return false;
	}

	protected virtual bool OnTarget(Mobile mob)
	{
		return false;
	}

	protected virtual bool OnTarget(Item item)
	{
		return false;
	}

	protected virtual bool OnTarget(GroundTarget groundTarget)
	{
		return false;
	}

	protected virtual bool OnTarget(StaticTarget staticTarget)
	{
		return false;
	}

	public virtual void Cancel()
	{
		if (!this.hasActed)
		{
			this.hasActed = true;
			this.OnCancel();
			this.Clear();
		}
	}

	protected virtual void OnCancel()
	{
	}
}
