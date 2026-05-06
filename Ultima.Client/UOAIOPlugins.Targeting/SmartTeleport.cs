using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Targeting;

public sealed class SmartTeleport
{
	public static readonly ActionCallback Macro_Callback;

	private static bool OnMacro(string args)
	{
		if (TargetManager.IsActive && TargetManager.Active is ServerTargetHandler { Action: TargetAction.Teleport })
		{
			GroundTarget groundTarget = SmartTeleport.FindTileNearby(World.Player);
			if (groundTarget != null)
			{
				TargetManager.Target(groundTarget);
			}
		}
		return true;
	}

	private static GroundTarget FindTileNearby(Mobile player)
	{
		int x = player.X;
		int y = player.Y;
		int z = player.Z;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int num4 = 12; num4 >= 12; num4--)
		{
			num = x;
			num2 = y;
			switch ((Direction)player.Direction)
			{
			case Direction.Up:
				num2--;
				break;
			case Direction.North:
				num++;
				num2--;
				break;
			case Direction.Right:
				num++;
				break;
			case Direction.East:
				num++;
				num2++;
				break;
			case Direction.Down:
				num2++;
				break;
			case Direction.South:
				num--;
				num2++;
				break;
			case Direction.Left:
				num--;
				break;
			case Direction.West:
				num--;
				num2--;
				break;
			}
			num3 = Map.GetMatrix(Engine.m_World).GetLandTile(num, num2).z;
			GroundTarget groundTarget = new GroundTarget(num, num2, num3);
			if (World.InRange(groundTarget) && Map.LineOfSight(World.Player, groundTarget))
			{
				return groundTarget;
			}
		}
		return null;
	}

	static SmartTeleport()
	{
		SmartTeleport.Macro_Callback = OnMacro;
	}
}
