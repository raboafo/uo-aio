using System;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Automation;

internal class AutoBandage
{
	public static Thread _tAutoBandage;

	private static CancellationTokenSource _cts;

	private static CancellationToken _cancelToken;

	public static readonly ActionCallback Macro_Callback;

	public static void Initialize()
	{
	}

	private static void EnableAutoBandage()
	{
		if (AutoBandage._tAutoBandage == null || !AutoBandage._tAutoBandage.IsAlive)
		{
			AutoBandage._cts = new CancellationTokenSource();
			AutoBandage._cancelToken = AutoBandage._cts.Token;
			AutoBandage._tAutoBandage = new Thread(tAutoBandage);
			AutoBandage._tAutoBandage.IsBackground = true;
			AutoBandage._tAutoBandage.Start();
			Console.Print("Autobandage enabled", Console.MessageType.Success);
		}
	}

	private static void DisableAutoBandage()
	{
		if (AutoBandage._tAutoBandage.IsAlive)
		{
			AutoBandage._cts.Cancel();
		}
	}

	public static void tAutoBandage()
	{
		while (true)
		{
			try
			{
				AutoBandage._cancelToken.ThrowIfCancellationRequested();
				if (!GBandageTimer.Active && !World.Player.Flags.Equals(MobileFlag.Hidden) && !World.Player.IsDead && World.Player.CurrentHitPoints < World.Player.MaximumHitPoints && !TargetManager.IsActive && World.Player.Backpack.FindItems(new ItemIDValidator(3617, 3817)).Length != 0)
				{
					Engine.BandageSelf();
				}
			}
			catch (OperationCanceledException)
			{
				AutoBandage._cts.Dispose();
				Console.Print("Auto bandage disabled", Console.MessageType.Generic);
				break;
			}
			catch (Exception ex2)
			{
				Debug.Error(ex2);
				Console.Print("[AutoBandage] Exception: " + ex2.Message, Console.MessageType.Error);
			}
			Thread.Sleep(200);
		}
	}

	public static bool OnMacro(string args)
	{
		switch (args)
		{
		case "On":
			Options.AutoBandage = true;
			AutoBandage.EnableAutoBandage();
			return true;
		case "Off":
			Options.AutoBandage = false;
			AutoBandage.DisableAutoBandage();
			return true;
		default:
		{
			Options.AutoBandage = !Options.AutoBandage;
			bool autoBandage = Options.AutoBandage;
			if (autoBandage)
			{
				if (autoBandage)
				{
					AutoBandage.EnableAutoBandage();
				}
			}
			else
			{
				AutoBandage.DisableAutoBandage();
			}
			return true;
		}
		}
	}

	static AutoBandage()
	{
		AutoBandage.Macro_Callback = OnMacro;
	}
}
