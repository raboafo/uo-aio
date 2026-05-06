namespace UOAIO.Targeting;

internal class BringToTargetHandler : ClientTargetHandler
{
	private int m_xOffset;

	private int m_yOffset;

	public BringToTargetHandler(int xOffset, int yOffset)
	{
		this.m_xOffset = xOffset;
		this.m_yOffset = yOffset;
	}

	protected override bool OnTarget(Item item)
	{
		Mobile player = World.Player;
		if (player != null)
		{
			Item backpack = player.Backpack;
			if (backpack != null)
			{
				if (item.InWorld)
				{
					return false;
				}
				int num = item.X + this.m_xOffset;
				int num2 = item.Y + this.m_yOffset;
				foreach (Item item2 in backpack.GetItems(new ItemIDValidator(item.ID)))
				{
					if (item2 != item)
					{
						if (item2.X != num || item2.Y != num2)
						{
							MoveContext moveContext = new MoveContext(item2, item2.Amount, item.Parent, clickFirst: false);
							moveContext.Locate(num, num2);
							moveContext.Enqueue();
						}
						num += this.m_xOffset;
						num2 += this.m_yOffset;
					}
				}
			}
		}
		return true;
	}
}
