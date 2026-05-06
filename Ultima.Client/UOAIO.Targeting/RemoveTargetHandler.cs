namespace UOAIO.Targeting;

internal class RemoveTargetHandler : ClientTargetHandler
{
	protected override bool OnTarget(Item item)
	{
		World.Remove(item);
		return true;
	}

	protected override bool OnTarget(Mobile mob)
	{
		if (!mob.Player)
		{
			World.Remove(mob);
		}
		return true;
	}
}
