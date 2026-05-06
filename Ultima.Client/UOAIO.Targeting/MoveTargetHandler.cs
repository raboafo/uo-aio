namespace UOAIO.Targeting;

internal class MoveTargetHandler : ClientTargetHandler
{
	private Item m_Item;

	private int? m_Amount;

	public MoveTargetHandler(int? amount)
	{
		this.m_Amount = amount;
	}

	protected override bool OnTarget(Item item)
	{
		if (this.m_Item == null)
		{
			this.m_Item = item;
			Engine.AddTextMessage("Target the destination container.");
			return false;
		}
		if (!item.IsContainer)
		{
			Engine.AddTextMessage("That is not a container.");
			return false;
		}
		Mobile player = World.Player;
		if (player != null)
		{
			Item backpack = player.Backpack;
			if (backpack != null)
			{
				int num = 0;
				foreach (Item item2 in backpack.GetItems(new ItemIDValidator(this.m_Item.ID)))
				{
					if (this.m_Amount.HasValue && num++ == this.m_Amount)
					{
						break;
					}
					if (item2.Parent != item)
					{
						new MoveContext(item2, item2.Amount, item, clickFirst: false).Enqueue();
					}
				}
			}
		}
		return true;
	}
}
