namespace UOAIO.Targeting;

internal class FriendTargetHandler : ClientTargetHandler
{
	protected override bool OnTarget(Mobile mob)
	{
		FriendContext friendContext = new FriendContext(mob);
		if (mob.HasName)
		{
			friendContext.OnFinish();
		}
		else
		{
			friendContext.Dispatch();
		}
		return true;
	}

	public void OnCancel(TargetCancelType type)
	{
	}
}
