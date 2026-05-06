using System.Collections.Generic;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Targeting;

public static class SmartAoE
{
	private static Thread _tSmartAoE;

	public static void Initialize()
	{
		SmartAoE._tSmartAoE = new Thread(tSmartMSCL);
		SmartAoE._tSmartAoE.IsBackground = true;
	}

	public static bool SmartMSCL_OnMacro(string args)
	{
		if (!SmartAoE._tSmartAoE.IsAlive)
		{
			SmartAoE._tSmartAoE = new Thread(tSmartMSCL);
			SmartAoE._tSmartAoE.IsBackground = true;
			SmartAoE._tSmartAoE.Start();
		}
		return true;
	}

	public static void tSmartMSCL()
	{
		int num = 10;
		Mobile player = World.Player;
		try
		{
			if (TargetManager.IsActive)
			{
				ServerTargetHandler serverTargetHandler = TargetManager.Active as ServerTargetHandler;
				if (serverTargetHandler.Action == TargetAction.MeteorSwarm || serverTargetHandler.Action == TargetAction.ChainLightning)
				{
					try
					{
						foreach (Mobile value in World.Mobiles.Values)
						{
							Thread.Sleep(5);
							player = World.Player;
							if (!value.Human || !value.Visible || value.IsDead || value.IsPet || value.IsGuarded || value.Player || player.DistanceTo(value.X, value.Y) <= 2 || value.Notoriety == Notoriety.Ally || value.IsFriend || player.Faction == value.Faction || ((player.Notoriety == Notoriety.Murderer || value.Notoriety == Notoriety.Innocent) && player.Notoriety != Notoriety.Murderer) || value.Notoriety == Notoriety.Vendor)
							{
								continue;
							}
							if (Actions.CanTargetMobile(player, value, num))
							{
								TargetManager.Target(value);
								return;
							}
							for (int num2 = 2; num2 >= -2; num2--)
							{
								for (int num3 = 2; num3 >= -2; num3--)
								{
									GroundTarget groundTarget = new GroundTarget(value.X + num2, value.Y + num3, Map.GetMatrix(Engine.m_World).GetLandTile(value.X + num2, value.Y + num3).z);
									if (Actions.CanTargetGround(player, groundTarget))
									{
										TargetManager.Target(groundTarget);
										return;
									}
								}
							}
							int num4 = 0;
							int num5 = 0;
							for (int num6 = 2; num6 >= -2; num6--)
							{
								for (int num7 = 2; num7 >= -2; num7--)
								{
									num4 = value.X + num6;
									num5 = value.Y + num7;
									MapPackage cache = Map.GetCache();
									List<ICell> list = cache.cells[num4 - cache.CellX, num5 - cache.CellY];
									for (int i = 0; i < list.Count; i++)
									{
										ICell cell = list[i];
										if (cell != null && cell is StaticItem)
										{
											StaticItem staticItem = (StaticItem)cell;
											if (player.DistanceTo(num4, num5) <= num && Map.LineOfSight(player, new Point3D(num4, num5, staticItem.Z)) && World.InRange(new GroundTarget(num4, num5, staticItem.Z)))
											{
												StaticTarget targeted = new StaticTarget(num4, num5, staticItem.Z, staticItem.m_ID, staticItem.m_RealID, staticItem.Hue);
												TargetManager.Target(targeted);
												return;
											}
										}
									}
								}
							}
							foreach (Item value2 in World.Items.Values)
							{
								if (value2.DistanceTo(value.X, value.Y) <= 2 && value2.DistanceTo(player.X, player.Y) <= num && Actions.CanTargetItem(player, value2, num, lineOfSight: true))
								{
									TargetManager.Target(value2);
									return;
								}
							}
						}
					}
					catch
					{
					}
				}
			}
		}
		catch
		{
		}
		Thread.Sleep(250);
	}
}
