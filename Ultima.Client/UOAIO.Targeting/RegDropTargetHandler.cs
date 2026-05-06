namespace UOAIO.Targeting;

internal class RegDropTargetHandler : ClientTargetHandler
{
	protected override bool OnTarget(Item item)
	{
		if (item.IsContainer)
		{
			Mobile player = World.Player;
			if (player != null)
			{
				Item backpack = player.Backpack;
				if (backpack != null)
				{
					foreach (Item item2 in backpack.GetItems(ReagentValidator.Validator))
					{
						if (item2.Parent != item)
						{
							new MoveContext(item2, item2.Amount, item, clickFirst: false).Enqueue();
						}
					}
				}
			}
			return true;
		}
		Engine.AddTextMessage("That is not a container.");
		return false;
	}
}
