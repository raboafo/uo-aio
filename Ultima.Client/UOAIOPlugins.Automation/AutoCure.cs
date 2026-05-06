using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

public sealed class AutoCure
{
	public static readonly ActionCallback Macro_Callback;

	private static Thread m_Thread;

	private static ServerTargetHandler serverTarget;

	private static Mobile self;

	private static bool OnMacro(string args)
	{
		Options.AutoCure = !Options.AutoCure;
		if (!Options.AutoCure)
		{
			AutoCure.Disable();
		}
		else
		{
			AutoCure.Enable();
		}
		return true;
	}

	private static void Disable()
	{
		if (AutoCure.m_Thread.IsAlive)
		{
			AutoCure.m_Thread.Abort();
		}
	}

	private static void Enable()
	{
		if (!AutoCure.m_Thread.IsAlive)
		{
			AutoCure.m_Thread = new Thread(DoCure)
			{
				IsBackground = true,
				Name = "VeritasUO Auto Cure Thread"
			};
			AutoCure.m_Thread.Start();
		}
	}

	private static void DoCure()
	{
		while (true)
		{
			if (!AutoCure.self.IsDead && Options.AutoCure)
			{
				AutoCure.UsePotion();
			}
		}
	}

	private static void UsePotion()
	{
		while (true)
		{
			if (!Options.AutoCure || AutoCure.self.CurrentHitPoints * 100 / AutoCure.self.MaximumHitPoints > 61 || !AutoCure.self.IsPoisoned)
			{
				continue;
			}
			if (!TargetManager.IsActive && AutoCure.self.CurrentHitPoints <= AutoCure.self.MaximumHitPoints - 21)
			{
				Engine.UsePotion(PotionType.Orange);
				Thread.Sleep(22);
				if (!AutoCure.self.IsPoisoned && AutoCure.self.CurrentHitPoints <= AutoCure.self.MaximumHitPoints - 21)
				{
					Engine.UsePotion(PotionType.Yellow);
				}
			}
			else if (TargetManager.IsActive && AutoCure.self.CurrentHitPoints <= 61)
			{
				Engine.UsePotion(PotionType.Orange);
				Thread.Sleep(22);
				if (!AutoCure.self.IsPoisoned && AutoCure.self.CurrentHitPoints <= AutoCure.self.MaximumHitPoints - 21)
				{
					Engine.UsePotion(PotionType.Yellow);
				}
			}
		}
	}

	static AutoCure()
	{
		AutoCure.Macro_Callback = OnMacro;
		AutoCure.m_Thread = new Thread(DoCure);
		AutoCure.self = World.Player;
	}
}
