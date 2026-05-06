namespace UOAIO.Targeting;

internal class IgnoreTargetHandler : ClientTargetHandler
{
	protected override bool OnTarget(Mobile mob)
	{
		IgnoreContext ignoreContext = new IgnoreContext(mob);
		if (mob.HasName)
		{
			ignoreContext.OnFinish();
		}
		else
		{
			ignoreContext.Dispatch();
		}
		return true;
	}

	public void OnCancel(TargetCancelType type)
	{
	}
}
