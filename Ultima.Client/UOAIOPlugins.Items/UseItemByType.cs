using UOAIO;

namespace UOAIOPlugins.Items;

public sealed class UseItemByType
{
	public static void UseItem(int[] itemIDs)
	{
		Engine.FindItem(itemIDs)?.Use();
	}

	public static int Items(string items)
	{
		return items switch
		{
			"Storms Eye" => 3967, 
			"Urn" => 9246, 
			"Blood Rose" => 74, 
			"Gem of Emp" => 7955, 
			"Clarity Potion" => 3628, 
			_ => -1, 
		};
	}

	public static bool UseItemByType_OnAction(string args)
	{
		Item[] array = World.Player.Backpack.FindItems(new ItemIDValidator(UseItemByType.Items(args)));
		int num = 0;
		if (num < array.Length)
		{
			array[num].Use();
			return true;
		}
		return true;
	}
}
