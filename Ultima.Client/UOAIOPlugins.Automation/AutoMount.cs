using System;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

public sealed class AutoMount
{
	public static readonly ActionCallback Macro_Callback;

	private static DateTime dateTime;

	private static Thread m_Thread;

	private static bool OnMacro(string args)
	{
		Options.AutoMount = !Options.AutoMount;
		if (!Options.AutoMount)
		{
			AutoMount.Disable();
		}
		else
		{
			AutoMount.Enable();
		}
		return true;
	}

	private static void Disable()
	{
		if (AutoMount.m_Thread.IsAlive)
		{
			AutoMount.m_Thread.Abort();
		}
	}

	private static void Enable()
	{
		if (!AutoMount.m_Thread.IsAlive)
		{
			AutoMount.m_Thread = new Thread(DoAutoMount)
			{
				IsBackground = true,
				Name = "VeritasUO Auto Mount Thread"
			};
			AutoMount.m_Thread.Start();
		}
	}

	private static void DoAutoMount()
	{
		while (true)
		{
			if (!World.Player.IsDead && !World.Player.IsMounted && ((TargetManager.Server == null) ? TargetAction.Unknown : TargetManager.Server.Action) != TargetAction.GreaterHeal)
			{
				if (World.Player.FollowersCur >= 1)
				{
					Engine.commandEntered("all follow me");
					Engine.commandEntered("all come");
					Thread.Sleep(500);
				}
				Engine.Remount();
			}
			Thread.Sleep(50);
		}
	}

	static AutoMount()
	{
		AutoMount.Macro_Callback = OnMacro;
		AutoMount.dateTime = DateTime.MinValue;
		AutoMount.m_Thread = new Thread(DoAutoMount);
	}
}
