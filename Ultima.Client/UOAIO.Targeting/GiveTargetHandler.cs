using System;

namespace UOAIO.Targeting;

internal class GiveTargetHandler : ClientTargetHandler
{
	private GiveEntry m_Entry;

	private int m_DesiredAmount;

	public GiveTargetHandler(GiveEntry entry, int desiredAmount)
	{
		this.m_Entry = entry;
		this.m_DesiredAmount = desiredAmount;
	}

	protected override bool OnTarget(Mobile mob)
	{
		this.GiveTo(mob);
		return true;
	}

	protected override bool OnTarget(Item item)
	{
		if (!item.IsContainer)
		{
			return false;
		}
		this.GiveTo(item);
		return true;
	}

	private void GiveTo(IEntity recipient)
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		Item backpack = player.Backpack;
		if (backpack == null)
		{
			return;
		}
		int desiredAmount = this.m_DesiredAmount;
		IItemValidator[] validators = this.m_Entry.Validators;
		Item[][] array = new Item[validators.Length][];
		int[] array2 = new int[validators.Length];
		for (int i = 0; i < validators.Length; i++)
		{
			array[i] = backpack.FindItems(validators[i]);
			int num = 0;
			for (int j = 0; j < array[i].Length; j++)
			{
				num += Math.Max(1, array[i][j].Amount);
			}
			int num2 = ((desiredAmount != -1) ? desiredAmount : this.m_Entry.GetAmount(num));
			if (num2 > num)
			{
				Engine.AddTextMessage("You do not have enough to give them.");
				return;
			}
			array2[i] = num2;
		}
		for (int k = 0; k < validators.Length; k++)
		{
			Item[] array3 = array[k];
			int num3 = array2[k];
			for (int l = 0; l < array3.Length; l++)
			{
				if (num3 <= 0)
				{
					break;
				}
				Item item = array3[l];
				int num4 = Math.Min(num3, Math.Max(1, item.Amount));
				new MoveContext(item, num4, recipient, clickFirst: false).Enqueue();
				num3 -= num4;
			}
		}
	}
}
