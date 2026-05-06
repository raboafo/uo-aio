using System.Collections.Generic;
using UOAIO;

namespace Ultima.Client;

internal sealed class WandRepository
{
	private static readonly Dictionary<Item, WandInformation> table;

	public static void Set(Item item, WandInformation? value)
	{
		if (value.HasValue)
		{
			WandRepository.Store(item, value.Value);
		}
		else
		{
			WandRepository.Delete(item);
		}
	}

	public static void Store(Item item, WandInformation value)
	{
		WandRepository.table[item] = value;
	}

	public static void Delete(Item item)
	{
		WandRepository.table.Remove(item);
	}

	public static bool Retrieve(Item item, out WandInformation value)
	{
		return WandRepository.table.TryGetValue(item, out value);
	}

	public static Item Find(WandEffect effect)
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return null;
		}
		Item item = player.FindEquip(Layer.OneHanded);
		if (item != null && WandRepository.Retrieve(item, out var value) && value.Effect == effect && value.Charges > 0)
		{
			item.Look();
			return item;
		}
		foreach (KeyValuePair<Item, WandInformation> item2 in WandRepository.table)
		{
			if (item2.Value.Effect == effect && item2.Value.Charges > 0 && item2.Key.IsChildOf(player))
			{
				item2.Key.Look();
				return item2.Key;
			}
		}
		return null;
	}

	static WandRepository()
	{
		WandRepository.table = new Dictionary<Item, WandInformation>();
	}
}
