using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;
using UOAIOPlugins.Targeting;

namespace UOAIOPlugins.Automation;

public static class AutoExploPot
{
	private static Thread _thread;

	private static ArrayList _journal;

	private static int _jAutoExplo;

	private static DateTime _dtLastUsed;

	public static readonly ActionCallback Macro_Callback;

	public static bool OnMacro(string args)
	{
		if (AutoExploPot._thread == null || !AutoExploPot._thread.IsAlive)
		{
			AutoExploPot._thread = new Thread(DoAutoExploPot);
			AutoExploPot._thread.IsBackground = true;
			AutoExploPot._thread.Start();
		}
		return true;
	}

	private static void TryToTarget(Mobile target)
	{
		try
		{
			Mobile player = World.Player;
			Thread.Sleep(300);
			if (Actions.CanTargetMobile(player, target, 11))
			{
				TargetManager.Target(target);
				return;
			}
			for (int num = 10; num >= -10; num--)
			{
				if (num >= 3 || num <= -3)
				{
					for (int num2 = 10; num2 >= -10; num2--)
					{
						if (num2 >= 3 || num2 <= -3)
						{
							int num3 = player.X + num;
							int num4 = player.Y + num2;
							MapPackage cache = Map.GetCache();
							List<ICell> list = cache.cells[num3 - cache.CellX, num4 - cache.CellY];
							for (int i = 0; i < list.Count; i++)
							{
								ICell cell = list[i];
								if (cell != null && cell is StaticItem)
								{
									StaticItem staticItem = (StaticItem)cell;
									if (player.DistanceTo(num3, num4) <= 10 && Map.LineOfSight(player, new Point3D(num3, num4, staticItem.Z)) && World.InRange(new GroundTarget(num3, num4, staticItem.Z)))
									{
										object targeted = new StaticTarget(num3, num4, staticItem.Z, staticItem.m_ID, staticItem.m_RealID, staticItem.Hue);
										object lastTarget = TargetManager.LastTarget;
										TargetManager.Target(targeted);
										TargetManager.LastTarget = lastTarget;
										return;
									}
								}
							}
						}
					}
				}
			}
			for (int num5 = 10; num5 >= -10; num5--)
			{
				if (num5 >= 3 || num5 <= -3)
				{
					for (int num6 = 10; num6 >= -10; num6--)
					{
						if (num6 >= 3 || num6 <= -3)
						{
							GroundTarget groundTarget = new GroundTarget(player.X + num5, player.Y + num6, Map.GetMatrix(Engine.m_World).GetLandTile(player.X + num5, player.Y + num6).z);
							if (Actions.CanTargetGround(player, groundTarget))
							{
								object lastTarget2 = TargetManager.LastTarget;
								TargetManager.Target(groundTarget);
								TargetManager.LastTarget = lastTarget2;
								return;
							}
						}
					}
				}
			}
			foreach (Item value in World.Items.Values)
			{
				int num7 = value.DistanceTo(player.X, player.Y);
				if (num7 > 2 && num7 < 10 && Actions.CanTargetItem(player, value, 10, lineOfSight: true))
				{
					object lastTarget3 = TargetManager.LastTarget;
					TargetManager.Target(value);
					TargetManager.LastTarget = lastTarget3;
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Error(ex);
			Console.Print(ex.Message, Console.MessageType.Error);
		}
	}

	private static void DoAutoExploPot()
	{
		try
		{
			if (TargetManager.LastTarget == null || DateTime.Now - AutoExploPot._dtLastUsed < TimeSpan.FromSeconds(5.5) || !(TargetManager.LastTarget is Mobile))
			{
				return;
			}
			Mobile target = (Mobile)TargetManager.LastTarget;
			Mobile player = World.Player;
			if (player.Ghost)
			{
				Console.Print("You cannot use a purple potion at this time.", Console.MessageType.Error);
				return;
			}
			AutoExploPot._jAutoExplo = Engine.m_Journal.Count;
			Actions.CancelClientTarget();
			Actions.CancelServerTarget();
			if (!Engine.UsePotion(PotionType.Purple))
			{
				Console.Print("You do not have any purple potions!", Console.MessageType.Error);
				return;
			}
			AutoExploPot._dtLastUsed = DateTime.Now;
			DateTime now = DateTime.Now;
			while (!AutoExploPot.CheckJournal($"{player.Name}: 2"))
			{
				AutoExploPot._jAutoExplo = Engine.m_Journal.Count;
				Thread.Sleep(10);
				if (DateTime.Now - now > TimeSpan.FromSeconds(5.0))
				{
					Console.Print("Never saw potion timer.  Lag?", Console.MessageType.Error);
					return;
				}
			}
			if (Engine.Ping / 5 * 5 > 500)
			{
				Console.Print("Smart Toss: Warning! Your ping is greater than 500!", Console.MessageType.Warning);
				AutoExploPot.TryToTarget(target);
			}
			else
			{
				Thread.Sleep(500 - Engine.Ping / 5 * 5);
				AutoExploPot.TryToTarget(target);
			}
		}
		catch (Exception ex)
		{
			Debug.Error(ex);
			Console.Print(ex.Message, Console.MessageType.Error);
		}
	}

	public static bool CheckJournal(string entry)
	{
		AutoExploPot._journal.Clear();
		if (Engine.m_Journal.Count < 1)
		{
			return false;
		}
		for (int i = AutoExploPot._jAutoExplo; i < Engine.m_Journal.Count; i++)
		{
			AutoExploPot._journal.Add(Engine.m_Journal[i]);
		}
		if (AutoExploPot._journal.Count == 0)
		{
			return false;
		}
		for (int j = 0; j < AutoExploPot._journal.Count; j++)
		{
			if (((JournalEntry)AutoExploPot._journal[j]).Text == entry)
			{
				return true;
			}
		}
		return false;
	}

	static AutoExploPot()
	{
		AutoExploPot._journal = new ArrayList();
		AutoExploPot.Macro_Callback = OnMacro;
	}
}
