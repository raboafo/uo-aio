using System.Linq;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

public sealed class AutoStrPot
{
	private static readonly ItemIDValidator WhitePotion_Validator;

	public static readonly ActionCallback Macro_Callback;

	private static Thread m_Thread;

	private static bool OnMacro(string args)
	{
		Options.AutoStrPot = !Options.AutoStrPot;
		if (!Options.AutoStrPot)
		{
			AutoStrPot.Disable();
		}
		else
		{
			AutoStrPot.Enable();
		}
		return true;
	}

	private static void Disable()
	{
		if (AutoStrPot.m_Thread.IsAlive)
		{
			AutoStrPot.m_Thread.Abort();
		}
	}

	private static void Enable()
	{
		if (!AutoStrPot.m_Thread.IsAlive)
		{
			AutoStrPot.m_Thread = new Thread(DoStrPot)
			{
				IsBackground = true,
				Name = "VeritasUO Auto Strength Potion Thread"
			};
			AutoStrPot.m_Thread.Start();
		}
	}

	private static void DoStrPot()
	{
		while (true)
		{
			Thread.Sleep(250);
			Item item = World.Player.Backpack.FindItems(AutoStrPot.WhitePotion_Validator).FirstOrDefault();
			if (item != null && World.Player.MaximumHitPoints < 100 && !World.Player.IsPoisoned && !TargetManager.IsActive)
			{
				Engine.UsePotion(PotionType.White);
			}
		}
	}

	static AutoStrPot()
	{
		AutoStrPot.WhitePotion_Validator = new ItemIDValidator(3849);
		AutoStrPot.Macro_Callback = OnMacro;
		AutoStrPot.m_Thread = new Thread(DoStrPot);
	}
}
