using System;
using System.Collections.Generic;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

public sealed class AutoHeal
{
	public sealed class Defend
	{
		private static double _Numb;

		private static double _Ping;

		private static TargetAction _Action;

		public static void DefendPlayer()
		{
			if ((AutoHeal._ServerTarget = TargetManager.Server) == null)
			{
				return;
			}
			Defend._Action = AutoHeal._ServerTarget.Action;
			if (false)
			{
				return;
			}
			switch (Defend._Action)
			{
			case TargetAction.Heal:
				if (AutoHeal._Self.CurrentHitPoints * 100 / AutoHeal._Self.MaximumHitPoints < 92 && !AutoHeal._Self.IsPoisoned)
				{
					AutoHeal._Self.OnTarget();
				}
				break;
			case TargetAction.Cure:
				if (AutoHeal._Self.IsPoisoned)
				{
					Thread.Sleep(Defend.CureRandom());
					AutoHeal._Self.OnTarget();
				}
				break;
			case TargetAction.GreaterHeal:
				Thread.Sleep(100);
				if (AutoHeal._Self.CurrentHitPoints <= AutoHeal._HealValue && !AutoHeal._Self.IsPoisoned)
				{
					AutoHeal._Self.OnTarget();
				}
				break;
			}
		}

		public static void DefendParty()
		{
			bool flag;
			if ((AutoHeal._ServerTarget = TargetManager.Server) != null)
			{
				Defend._Action = AutoHeal._ServerTarget.Action;
				flag = false;
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				return;
			}
			switch (Defend._Action)
			{
			case TargetAction.GreaterHeal:
			{
				Thread.Sleep(100);
				Mobile[] members2 = Party.Members;
				Array.Sort(members2, HealSorter.Comparer);
				for (int j = 0; j < members2.Length; j++)
				{
					if (members2[j].Visible && !members2[j].IsDead && !members2[j].IsPoisoned && members2[j].CurrentHitPoints * 100 / members2[j].MaximumHitPoints <= AutoHeal._HealValue && AutoHeal._Self.InRange(members2[j], 12) && Map.LineOfSight(AutoHeal._Self, members2[j]))
					{
						members2[j].OnTarget();
						break;
					}
				}
				break;
			}
			case TargetAction.Cure:
			{
				Mobile[] members = Party.Members;
				foreach (Mobile mobile in members)
				{
					if (mobile.Visible && !mobile.IsDead && mobile.IsPoisoned && AutoHeal._Self.InRange(mobile, 12) && Map.LineOfSight(AutoHeal._Self, mobile))
					{
						_ = mobile != TargetManager.LastDefensiveTarget;
						mobile.OnTarget();
						break;
					}
				}
				break;
			}
			}
		}

		private static int CureRandom()
		{
			Random random = new Random();
			return random.Next(30, 200);
		}

		static Defend()
		{
			Defend._Numb = 0.4875;
			Defend._Ping = (double)Engine.Ping * 0.0005;
		}
	}

	public static readonly ActionCallback Macro_Callback;

	public static readonly CommandCallback Command_Callback;

	private static Thread m_Thread;

	private static Mobile _Self;

	private static Mobile _PriorityMobile;

	private static ServerTargetHandler _ServerTarget;

	private static List<Mobile> HighlightedMobiles;

	private static int _HealValue;

	public static int HealValue => AutoHeal._HealValue;

	private static bool OnMacro(string args)
	{
		Options.AutoHeal = !Options.AutoHeal;
		if (!Options.AutoHeal)
		{
			AutoHeal.Disable();
		}
		else
		{
			AutoHeal.Enable();
		}
		return true;
	}

	private static void Disable()
	{
		if (AutoHeal.m_Thread.IsAlive)
		{
			AutoHeal.m_Thread.Abort();
		}
	}

	private static void Enable()
	{
		if (!AutoHeal.m_Thread.IsAlive)
		{
			AutoHeal.m_Thread = new Thread(DoAutoHeal)
			{
				IsBackground = true,
				Name = "VeritasUO Auto Heal Thread"
			};
			AutoHeal.m_Thread.Start();
		}
	}

	private static void DoAutoHeal()
	{
		while (true)
		{
			if (!AutoHeal._Self.IsDead)
			{
				Defend.DefendPlayer();
				if (Party.State != PartyState.Alone)
				{
					Defend.DefendParty();
				}
			}
		}
	}

	private static void ChangeHealValue(CommandArgs args)
	{
		AutoHeal._HealValue = args.GetInt32(0);
	}

	static AutoHeal()
	{
		AutoHeal.Macro_Callback = OnMacro;
		AutoHeal.Command_Callback = ChangeHealValue;
		AutoHeal.m_Thread = new Thread(DoAutoHeal);
		AutoHeal._Self = World.Player;
		AutoHeal._PriorityMobile = World.Player;
		AutoHeal.HighlightedMobiles = new List<Mobile>();
		AutoHeal._HealValue = 62;
	}
}
