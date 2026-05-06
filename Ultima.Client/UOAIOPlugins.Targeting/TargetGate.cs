using UOAIO;

namespace UOAIOPlugins.Targeting;

internal class TargetGate
{
	public static Mobile self;

	public static bool TargetGate_OnMacro(string args)
	{
		ItemIDValidator parent = new ItemIDValidator(3948);
		PlayerDistanceValidator validator = new PlayerDistanceValidator(parent, 12);
		Item item = World.FindItem(validator);
		if (item != null && Map.LineOfSight(World.Player, item))
		{
			item.OnTarget();
		}
		return true;
	}

	static TargetGate()
	{
		TargetGate.self = World.Player;
	}
}
