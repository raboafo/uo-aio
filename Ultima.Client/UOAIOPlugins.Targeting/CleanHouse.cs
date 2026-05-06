using System;
using System.Collections;
using System.Threading;
using UOAIO;
using UOAIO.Targeting;

namespace UOAIOPlugins.Targeting;

public sealed class CleanHouse
{
	public static readonly ActionCallback Macro_Callback;

	private static Thread m_Thread;

	private static bool OnMacro(string args)
	{
		if (CleanHouse.m_Thread == null)
		{
			CleanHouse.m_Thread = new Thread(DoCleanHouse);
		}
		if (!CleanHouse.m_Thread.IsAlive)
		{
			CleanHouse.m_Thread = new Thread(DoCleanHouse)
			{
				IsBackground = true,
				Name = "VeritasUO Clean House Thread"
			};
			CleanHouse.m_Thread.Start();
		}
		return true;
	}

	private static void DoCleanHouse()
	{
		Actions.CancelClientTarget();
		Actions.CancelServerTarget();
		if (World.Player != null)
		{
			for (int i = 0; i < Engine.Multis.Items.Count; i++)
			{
				Item item = Engine.Multis.Items[i];
				if (!item.InWorld || !item.IsMulti)
				{
					continue;
				}
				CustomMultiEntry customMulti = CustomMultiLoader.GetCustomMulti(item.Serial, item.Revision);
				Multi multi = null;
				if (customMulti != null)
				{
					multi = customMulti.Multi;
				}
				if (multi == null)
				{
					multi = item.Multi;
				}
				if (multi == null || !Engine.Multis.RunUO_IsInside(item, multi, World.Player.X, World.Player.Y, World.Player.Z))
				{
					continue;
				}
				ArrayList dataStore = Engine.GetDataStore();
				foreach (Mobile value in World.Mobiles.Values)
				{
					if ((int)(value.Notoriety - 3) <= 3 && value != World.Player && !value.Flags[MobileFlag.YellowHits] && !value.IsFriend && value.Visible && World.InRange(value))
					{
						int xReal = value.XReal;
						int yReal = value.YReal;
						int zReal = value.ZReal;
						if (Engine.Multis.RunUO_IsInside(item, multi, xReal, yReal, zReal))
						{
							dataStore.Add(value);
						}
					}
				}
				if (dataStore.Count > 0)
				{
					for (int j = 0; j < dataStore.Count; j++)
					{
						Mobile targeted = (Mobile)dataStore[j];
						SpeechFormat.Find("I ban thee").OnSpeech("I ban thee");
						DateTime now = DateTime.Now;
						while (!TargetManager.IsActive)
						{
							if (DateTime.Now - now > TimeSpan.FromMilliseconds(500.0))
							{
								return;
							}
							Thread.Sleep(10);
						}
						TargetManager.Target(targeted);
						Actions.CancelServerTarget();
						Actions.CancelClientTarget();
					}
				}
				Engine.ReleaseDataStore(dataStore);
				return;
			}
		}
		CleanHouse.m_Thread.Abort();
	}

	static CleanHouse()
	{
		CleanHouse.Macro_Callback = OnMacro;
		CleanHouse.m_Thread = new Thread(DoCleanHouse);
	}
}
