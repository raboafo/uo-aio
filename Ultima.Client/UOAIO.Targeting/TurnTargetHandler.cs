using System;

namespace UOAIO.Targeting;

internal class TurnTargetHandler : ClientTargetHandler
{
	protected override bool OnTarget(object targeted)
	{
		if (targeted is IPoint3D point3D)
		{
			Mobile player = World.Player;
			if (player != null)
			{
				Direction direction = Engine.GetDirection(player.X, player.Y, point3D.X, point3D.Y);
				int walkDirection = Engine.GetWalkDirection(direction);
				walkDirection &= 7;
				if ((player.Direction & 7) != walkDirection)
				{
					player.Direction = (byte)walkDirection;
					Engine.SendMovementRequest(walkDirection, player.X, player.Y, player.Z, TimeSpan.FromSeconds(0.1));
				}
			}
		}
		return true;
	}
}
