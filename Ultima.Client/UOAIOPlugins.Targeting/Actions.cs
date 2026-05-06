using System;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Targeting;

public sealed class Actions
{
	public static int DistanceTo(int x1, int y1, int x2, int y2)
	{
		int num = x1 - x2;
		int num2 = y1 - y2;
		return (int)Math.Sqrt(num * num + num2 * num2);
	}

	public static bool ClearTarget_OnMacro(string args)
	{
		Actions.CancelServerTarget();
		Actions.CancelClientTarget();
		return true;
	}

	public static void CancelServerTarget()
	{
		if (TargetManager.IsActive && TargetManager.Server != null)
		{
			Network.Send(new PTarget_Cancel(TargetManager.Server));
		}
		TargetManager.Server = null;
	}

	public static void CancelClientTarget()
	{
		TargetManager.Client = null;
	}

	public static bool CanTargetMobile(Mobile mobile1, Mobile mobile2, int range)
	{
		return mobile1.InRange(mobile2, range) && mobile2.InRange(mobile1, range) && Map.LineOfSight(mobile1, mobile2);
	}

	public static bool CanTargetGround(Mobile player, GroundTarget tile)
	{
		return World.InRange(tile) && Map.LineOfSight(World.Player, tile);
	}

	public static bool CanTargetItem(Mobile player, Item item, int range, bool lineOfSight)
	{
		return item.InRange(player.X, player.Y, range) && (!lineOfSight || Map.LineOfSight(player, new Point3D(item.X, item.Y, item.Z)));
	}
}
