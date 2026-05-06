using System;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

public sealed class AutoMeditate
{
	public static readonly ActionCallback Macro_Callback;

	private static DateTime dateTime;

	private static Thread m_Thread;

	private static bool OnMacro(string args)
	{
		Options.AutoMeditate = !Options.AutoMeditate;
		if (!Options.AutoMeditate)
		{
			AutoMeditate.Disable();
		}
		else
		{
			AutoMeditate.Enable();
		}
		return true;
	}

	private static void Disable()
	{
		if (AutoMeditate.m_Thread.IsAlive)
		{
			AutoMeditate.m_Thread.Abort();
		}
	}

	private static void Enable()
	{
		if (!AutoMeditate.m_Thread.IsAlive)
		{
			AutoMeditate.m_Thread = new Thread(DoAutoMed)
			{
				IsBackground = true,
				Name = "VeritasUO Auto Meditate Thread"
			};
			AutoMeditate.m_Thread.Start();
		}
	}

	private static void DoAutoMed()
	{
		while (true)
		{
			if (!World.Player.IsDead && World.Player.MaximumMana > World.Player.CurrentMana && TargetManager.Server == null && DateTime.Now - AutoMeditate.dateTime > TimeSpan.FromSeconds(11.0))
			{
				Engine.Skills[SkillName.Meditation].Use();
				AutoMeditate.dateTime = DateTime.Now;
			}
			Thread.Sleep(50);
		}
	}

	static AutoMeditate()
	{
		AutoMeditate.Macro_Callback = OnMacro;
		AutoMeditate.dateTime = DateTime.MinValue;
		AutoMeditate.m_Thread = new Thread(DoAutoMed);
	}
}
