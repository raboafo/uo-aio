using System;
using System.Collections;
using System.Linq;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

public sealed class AutoPopPouch
{
	public static readonly ActionCallback Macro_Callback;

	private static ServerTargetHandler serverTargetHandler;

	private static readonly ItemIDValidator MagicTrapPouch_Validator;

	private static Thread m_Thread;

	private static ArrayList _Journal;

	private static int _Start;

	private static CancellationTokenSource _cts;

	private static CancellationToken _cancelToken;

	private static bool OnMacro(string args)
	{
		Options.AutoPopPouch = !Options.AutoPopPouch;
		if (!Options.AutoPopPouch)
		{
			AutoPopPouch.Disable();
		}
		else
		{
			AutoPopPouch.Enable();
		}
		return true;
	}

	private static void Disable()
	{
		if (AutoPopPouch.m_Thread.IsAlive)
		{
			AutoPopPouch._cts.Cancel();
		}
	}

	private static void Enable()
	{
		if (AutoPopPouch.m_Thread == null || !AutoPopPouch.m_Thread.IsAlive)
		{
			AutoPopPouch._cts = new CancellationTokenSource();
			AutoPopPouch._cancelToken = AutoPopPouch._cts.Token;
			AutoPopPouch.m_Thread = new Thread(DoUseOnce);
			AutoPopPouch.m_Thread.IsBackground = true;
			AutoPopPouch.m_Thread.Start();
			Console.Print("Auto pop pouch enabled", Console.MessageType.Success);
		}
	}

	private static void DoUseOnce()
	{
		while (true)
		{
			try
			{
				AutoPopPouch._cancelToken.ThrowIfCancellationRequested();
				Thread.Sleep(100);
				if (TargetManager.IsActive)
				{
					if (AutoPopPouch.serverTargetHandler != null && AutoPopPouch.serverTargetHandler.Action != TargetAction.Weaken && AutoPopPouch.serverTargetHandler.Action == TargetAction.MagicArrow && AutoPopPouch.RecentJournalEntry("You cannot move!"))
					{
						Thread.Sleep(275);
						AutoPopPouch._Start = Engine.m_Journal.Count;
						TargetManager.TargetSelf();
						Thread.Sleep(1000);
						DateTime now = DateTime.Now;
						while (!AutoPopPouch.RecentJournalEntry("You can move!") && !World.Player.IsDead && DateTime.Now - now < TimeSpan.FromSeconds(5.0))
						{
							AutoPopPouch._cancelToken.ThrowIfCancellationRequested();
							Engine.AutoUse();
							Thread.Sleep(500);
						}
						AutoPopPouch._Start = Engine.m_Journal.Count;
					}
				}
				else
				{
					if (!AutoPopPouch.RecentJournalEntry("You cannot move!"))
					{
						continue;
					}
					Thread.Sleep(300);
					AutoPopPouch._Start = Engine.m_Journal.Count;
					DateTime now2 = DateTime.Now;
					while (!AutoPopPouch.RecentJournalEntry("You can move!") && !World.Player.IsDead && DateTime.Now - now2 < TimeSpan.FromSeconds(5.0))
					{
						AutoPopPouch._cancelToken.ThrowIfCancellationRequested();
						Item item = World.Player.Backpack.FindItems(AutoPopPouch.MagicTrapPouch_Validator).FirstOrDefault();
						if (item != null && item.Hue == 1159)
						{
							Engine.commandEntered("[trappouch");
							Thread.Sleep(500);
						}
					}
					AutoPopPouch._Start = Engine.m_Journal.Count;
					continue;
				}
			}
			catch (OperationCanceledException)
			{
				AutoPopPouch._cts.Dispose();
				Console.Print("Auto pop pouch disabled", Console.MessageType.Generic);
				break;
			}
			catch (Exception ex2)
			{
				Debug.Error(ex2);
				Console.Print("[AutoPopPouch] " + ex2.Message, Console.MessageType.Error);
			}
		}
	}

	private static bool RecentJournalEntry(string entry)
	{
		AutoPopPouch._Journal.Clear();
		if (Engine.m_Journal.Count < 1)
		{
			return false;
		}
		for (int i = AutoPopPouch._Start; i < Engine.m_Journal.Count; i++)
		{
			AutoPopPouch._Journal.Add(Engine.m_Journal[i]);
		}
		if (AutoPopPouch._Journal.Count == 0)
		{
			return false;
		}
		for (int j = 0; j < AutoPopPouch._Journal.Count; j++)
		{
			JournalEntry journalEntry = (JournalEntry)AutoPopPouch._Journal[j];
			if (journalEntry != null && journalEntry.Text == entry)
			{
				return true;
			}
		}
		return false;
	}

	static AutoPopPouch()
	{
		AutoPopPouch.Macro_Callback = OnMacro;
		AutoPopPouch.serverTargetHandler = TargetManager.Active as ServerTargetHandler;
		AutoPopPouch.MagicTrapPouch_Validator = new ItemIDValidator(3705);
		AutoPopPouch.m_Thread = new Thread(DoUseOnce);
		AutoPopPouch._Journal = new ArrayList();
	}
}
