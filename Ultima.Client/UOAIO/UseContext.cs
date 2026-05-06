namespace UOAIO;

internal class UseContext : ActionContext
{
	protected bool isManual;

	protected IEntity toUse;

	public IEntity ToUse => this.toUse;

	public bool IsManual => this.isManual;

	protected override bool IsReady => Engine.IsActionReady;

	public UseContext(IEntity toUse, bool isManual)
	{
		this.toUse = toUse;
		this.isManual = isManual;
	}

	public override void OnDispatch()
	{
		if (this.toUse == null)
		{
			return;
		}
		if (this.toUse is Item)
		{
			Item item = (Item)this.toUse;
			if (item.ID == 8901 || item.ID == 3643 || item.ID == 3834)
			{
				Network.Send(new PPE_QueryRunebookContent(item));
			}
		}
		Network.Send(new PUseRequest(this.toUse));
	}

	protected override bool CheckQueue()
	{
		if (this.toUse != null)
		{
			foreach (ActionContext item in ActionContext.Queued)
			{
				if (item is UseContext useContext && useContext.toUse == this.toUse)
				{
					return false;
				}
			}
		}
		return base.CheckQueue();
	}

	protected override int CompareTo(ActionContext cmp)
	{
		int num = base.CompareTo(cmp);
		if (num == 0 && this.isManual && !(cmp is UseContext { isManual: not false }))
		{
			num = -1;
		}
		return num;
	}
}
