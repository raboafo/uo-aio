using System;
using System.Collections;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

public sealed class AutoStun
{
	public static readonly ActionCallback Macro_Callback;

	private static bool _Flag;

	private static int _Start;

	private static ArrayList _Journal;

	private static Thread m_Thread;

	private static bool OnMacro(string args)
	{
		Options.AutoStun = !Options.AutoStun;
		if (!Options.AutoStun)
		{
			AutoStun.Disable();
		}
		else
		{
			AutoStun.Enable();
		}
		return true;
	}

	private static void Disable()
	{
		if (AutoStun.m_Thread.IsAlive)
		{
			AutoStun.m_Thread.Abort();
		}
	}

	private static void Enable()
	{
		if (!AutoStun.m_Thread.IsAlive)
		{
			AutoStun.m_Thread = new Thread(DoAutoStun)
			{
				IsBackground = true,
				Name = "VeritasUO Auto Stun Thread"
			};
			AutoStun.m_Thread.Start();
		}
	}

	private static void DoAutoStun()
	{
		AutoStun._Flag = true;
		DateTime dateTime = DateTime.Now - TimeSpan.FromSeconds(5.0);
		while (true)
		{
			try
			{
				Thread.Sleep(250);
				if (AutoStun._Flag)
				{
					Network.Send(new PWrestleStun());
					AutoStun._Flag = false;
				}
				bool flag = AutoStun.RecentJournalEntry("You successfully stun your opponent!");
				bool flag2 = AutoStun.RecentJournalEntry("You decide to not try to stun anyone.");
				if (flag || flag2)
				{
					if (flag)
					{
						dateTime = DateTime.Now;
					}
					AutoStun._Start = Engine.m_Journal.Count;
					Thread.Sleep(500);
					Network.Send(new PWrestleStun());
					DateTime now = DateTime.Now;
					while (!AutoStun.RecentJournalEntry("You get yourself ready to stun your opponent.") && DateTime.Now - now > TimeSpan.FromSeconds(5.0))
					{
						AutoStun._Start = Engine.m_Journal.Count;
						Thread.Sleep(500);
						Network.Send(new PWrestleStun());
						now = DateTime.Now;
					}
					AutoStun._Start = Engine.m_Journal.Count;
				}
				if (World.Player.CurrentStamina < 15 && DateTime.Now - dateTime > TimeSpan.FromSeconds(8.0) && !TargetManager.IsActive)
				{
					Engine.UsePotion(PotionType.Red);
				}
			}
			catch (Exception)
			{
			}
		}
	}

	private static bool RecentJournalEntry(string entry)
	{
		try
		{
			AutoStun._Journal.Clear();
			if (Engine.m_Journal.Count < 1)
			{
				return false;
			}
			for (int i = AutoStun._Start; i < Engine.m_Journal.Count; i++)
			{
				AutoStun._Journal.Add(Engine.m_Journal[i]);
			}
			if (AutoStun._Journal.Count == 0)
			{
				return false;
			}
			for (int j = 0; j < AutoStun._Journal.Count; j++)
			{
				if (((JournalEntry)AutoStun._Journal[j]).Text == entry)
				{
					return true;
				}
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	static AutoStun()
	{
		AutoStun.Macro_Callback = OnMacro;
		AutoStun._Journal = new ArrayList();
		AutoStun.m_Thread = new Thread(DoAutoStun);
	}
}
