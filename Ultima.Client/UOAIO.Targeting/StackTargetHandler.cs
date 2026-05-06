namespace UOAIO.Targeting;

internal class StackTargetHandler : ClientTargetHandler
{
	protected override bool OnTarget(Item item)
	{
		Mobile player = World.Player;
		if (player != null)
		{
			Item backpack = player.Backpack;
			if (backpack != null)
			{
				foreach (Item item2 in backpack.GetItems(new ItemIDValidator(item.ID)))
				{
					if (item2 != item)
					{
						MoveContext moveContext = new MoveContext(item2, item2.Amount, item, clickFirst: false);
						moveContext.Locate(item.X, item.Y);
						moveContext.Enqueue();
					}
				}
			}
		}
		return true;
	}
}
