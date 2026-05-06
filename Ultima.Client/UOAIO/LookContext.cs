namespace UOAIO;

internal class LookContext : ActionContext
{
	protected IEntity toObserve;

	public IEntity ToObserve => this.toObserve;

	public LookContext(IEntity toObserve)
	{
		this.toObserve = toObserve;
	}

	public override void OnDispatch()
	{
		if (this.toObserve != null)
		{
			Network.Send(new PLookRequest(this.toObserve));
		}
	}

	protected override bool CheckQueue()
	{
		if (this.toObserve != null)
		{
			foreach (ActionContext item in ActionContext.Queued)
			{
				if (item is LookContext lookContext && lookContext.toObserve == this.toObserve)
				{
					return false;
				}
			}
		}
		return base.CheckQueue();
	}
}
