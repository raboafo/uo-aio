namespace UOAIO.Targeting;

internal class DragToBagTargetHandler : ClientTargetHandler
{
	private bool clickFirst;

	public DragToBagTargetHandler(bool clickFirst)
	{
		this.clickFirst = clickFirst;
	}

	protected override bool OnTarget(Item item)
	{
		Mobile player = World.Player;
		if (player != null)
		{
			Item backpack = player.Backpack;
			if (backpack != null)
			{
				new MoveContext(item, item.Amount, backpack, this.clickFirst).Enqueue();
			}
		}
		return true;
	}
}
