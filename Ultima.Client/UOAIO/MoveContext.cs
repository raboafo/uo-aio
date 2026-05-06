namespace UOAIO;

internal class MoveContext : ActionContext
{
	protected Item pickUp;

	protected int amount;

	protected int x = -1;

	protected int y = -1;

	protected IEntity dropTo;

	protected bool liftFailed;

	protected bool clickFirst;

	protected override bool IsReady => Engine.IsActionReady;

	public MoveContext Locate(int x, int y)
	{
		this.x = x;
		this.y = y;
		return this;
	}

	protected override bool CheckQueue()
	{
		foreach (ActionContext item in ActionContext.Queued)
		{
			if (item is MoveContext moveContext && moveContext.pickUp == this.pickUp && moveContext.dropTo == this.dropTo && this is EquipContext == moveContext is EquipContext)
			{
				return false;
			}
		}
		return base.CheckQueue();
	}

	public MoveContext(Item pickUp, int amount, IEntity dropTo, bool clickFirst)
	{
		this.pickUp = pickUp;
		this.amount = amount;
		this.dropTo = dropTo;
		this.clickFirst = clickFirst;
	}

	public void OnLiftFailed()
	{
		this.liftFailed = true;
	}

	public override void OnBegin()
	{
		base.OnBegin();
		this.liftFailed = false;
	}

	public bool TryEnqueue()
	{
		if (this.pickUp == this.dropTo)
		{
			return false;
		}
		if (this.pickUp.Parent == this.dropTo && (this.x == -1 || this.pickUp.X == this.x) && (this.y == -1 || this.pickUp.Y == this.y))
		{
			return false;
		}
		return base.Enqueue();
	}

	public override void OnDispatch()
	{
		if (this.clickFirst)
		{
			Network.Send(new PLookRequest(this.pickUp));
		}
		Network.Send(new PPickupItem(this.pickUp, this.amount));
	}

	public override void OnFinish()
	{
		base.OnFinish();
		if (!this.liftFailed)
		{
			this.SendDropPacket();
		}
		else if (!this.ShouldRepeat)
		{
			Engine.AddTextMessage("Item movement failed.", Engine.DefaultFont, Hues.Load(38));
		}
	}

	protected virtual void SendDropPacket()
	{
		Network.Send(new PDropItem(this.pickUp.Serial, (short)this.x, (short)this.y, 0, this.dropTo.Serial));
	}
}
