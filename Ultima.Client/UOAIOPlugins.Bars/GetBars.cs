using System.Collections.Generic;
using UOAIO;

namespace UOAIOPlugins.Bars;

public static class GetBars
{
	public static List<IMobileStatus> bars;

	public static int x;

	public static int y;

	public static void ClearBars()
	{
		foreach (IMobileStatus bar in GetBars.bars)
		{
			if (!bar.Gump.Disposed)
			{
				bar.Gump.ManualClose();
			}
		}
		GetBars.bars.Clear();
		GetBars.x = Engine.GameX + 1030;
		GetBars.y = Engine.GameY;
	}

	public static void ClearBars_OnCommand(CommandArgs args)
	{
		if (GetBars.bars.Count > 0)
		{
			GetBars.ClearBars();
			Console.Print("Bars cleared.", Console.MessageType.Success);
		}
	}

	public static bool ClearBars_OnMacro(string args)
	{
		if (GetBars.bars.Count > 0)
		{
			GetBars.ClearBars();
			Console.Print("Bars cleared.", Console.MessageType.Success);
		}
		return true;
	}

	public static void DoGetBars(CommandArgs args)
	{
		List<Notoriety> list = new List<Notoriety>();
		if (args.Arguments.Length > 1)
		{
			if (args.Arguments[1].Contains("a"))
			{
				Console.Print("Grabbing all players bars.", Console.MessageType.Success);
				list.Add(Notoriety.Innocent);
				list.Add(Notoriety.Criminal);
				list.Add(Notoriety.Attackable);
				list.Add(Notoriety.Enemy);
				list.Add(Notoriety.Murderer);
			}
			else
			{
				if (args.Arguments[1].Contains("b"))
				{
					Console.Print("Grabbing all blue players bars.", Console.MessageType.Success);
					list.Add(Notoriety.Innocent);
				}
				if (args.Arguments[1].Contains("g"))
				{
					Console.Print("Grabbing all gray players bars.", Console.MessageType.Success);
					list.Add(Notoriety.Criminal);
					list.Add(Notoriety.Attackable);
				}
				if (args.Arguments[1].Contains("o"))
				{
					Console.Print("Grabbing all orange players bars.", Console.MessageType.Success);
					list.Add(Notoriety.Enemy);
				}
				if (args.Arguments[1].Contains("r"))
				{
					Console.Print("Grabbing all red players bars.", Console.MessageType.Success);
					list.Add(Notoriety.Murderer);
				}
			}
		}
		foreach (Mobile value in World.Mobiles.Values)
		{
			if (!World.Player.InRange(value.X, value.Y, 20))
			{
				continue;
			}
			if (value.Name.Length == 0)
			{
				value.QueryStats();
				continue;
			}
			foreach (Notoriety item in list)
			{
				if (value.Notoriety == item && value != World.Player && value.Notoriety != Notoriety.Vendor && !value.IsInParty && value.HumanOrGhost)
				{
					if (GetBars.y > 1024)
					{
						GetBars.x += 87;
						GetBars.y = 159;
					}
					IMobileStatus mobileStatus = new GPartyHealthBar(value, GetBars.x, GetBars.y);
					GetBars.bars.Add(mobileStatus);
					if (mobileStatus.Gump != null)
					{
						Gumps.Desktop.Children.Add(mobileStatus.Gump);
					}
					GetBars.y += 28;
					break;
				}
			}
		}
	}

	public static void GetBars_OnCommand(CommandArgs args)
	{
		if (GetBars.bars.Count > 0)
		{
			GetBars.ClearBars();
		}
		GetBars.DoGetBars(args);
	}

	static GetBars()
	{
		GetBars.bars = new List<IMobileStatus>();
		GetBars.x = Engine.GameX + 1030;
		GetBars.y = Engine.GameY;
	}
}
