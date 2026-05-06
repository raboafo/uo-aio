using UOAIO;

namespace UOAIOPlugins.Items;

public sealed class Scrolls
{
	public static int SpellNameToScrollID(string spell)
	{
		return spell switch
		{
			"Chain Lightning" => 8029, 
			"Energy Field" => 8030, 
			"Flame Strike" => 8031, 
			"Gate Travel" => 8032, 
			"Recall" => 8012, 
			"Mass Dispel" => 8034, 
			"Meteor Swarm" => 8035, 
			"Earthquake" => 8037, 
			"Resurrection" => 8039, 
			_ => -1, 
		};
	}

	public static bool UseScroll_OnAction(string args)
	{
		Item[] array = World.Player.Backpack.FindItems(new ItemIDValidator(Scrolls.SpellNameToScrollID(args)));
		int num = 0;
		if (num < array.Length)
		{
			array[num].Use();
			return true;
		}
		Spells.GetSpellByName(args).Cast();
		return true;
	}
}
