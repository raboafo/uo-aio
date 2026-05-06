using System;
using System.Collections.Generic;
using UOAIO.Profiles;

namespace UOAIO;

public static class ClientFormatEx
{
	private static bool _friendLocEnabled;

	private static string m_LastRecallRune;

	private static bool m_RecallActive;

	private static bool m_IsGateRecall;

	public static Queue<Item> runebooks;

	public static RuneInfoEx LastActiveRune;

	public static bool FriendLocEnabled => ClientFormatEx._friendLocEnabled;

	public static bool IsRecallActive
	{
		get
		{
			return ClientFormatEx.m_RecallActive;
		}
		set
		{
			ClientFormatEx.m_RecallActive = value;
		}
	}

	public static string LastRecallRune
	{
		get
		{
			return ClientFormatEx.m_LastRecallRune;
		}
		set
		{
			ClientFormatEx.m_LastRecallRune = value;
		}
	}

	public static bool IsGateRecall
	{
		get
		{
			return ClientFormatEx.m_IsGateRecall;
		}
		set
		{
			ClientFormatEx.m_IsGateRecall = value;
		}
	}

	public static void FriendLoc_OnCommand(CommandArgs args)
	{
		Engine.AddTextMessage($"You belong to {World.Player.Guild} guild");
		foreach (Character character in Player.Current.Friends.Characters)
		{
			Mobile mobile = character.Find();
			if (mobile != null)
			{
				GRadar.RegisterTrackable(mobile);
			}
		}
		ClientFormatEx._friendLocEnabled = true;
		Engine.AddTextMessage("Friend locations have been added to map!", Engine.DefaultFont, Hues.Load(68));
	}

	public static void OpenRunebooks(CommandArgs args)
	{
		foreach (Item item in World.Items.Values)
		{
			if ((item.ID == 8901 || item.ID == 3643 || item.ID == 3834) && item.Hue != 0)
			{
				new GenericContext(delegate
				{
					Network.Send(new PUseRequest(item));
				}).Enqueue();
			}
		}
	}

	public static void OpenRunebooks()
	{
		if (World.Player == null)
		{
			return;
		}
		if (ClientFormatEx.IsRecallActive || ClientFormatEx.runebooks.Count > 0)
		{
			Engine.AddTextMessage("Command OpenRunebooks already in progress!");
			return;
		}
		Player.Runes.Clear();
		foreach (Item item in World.Player.Backpack.Items)
		{
			if ((item.ID == 8901 || item.ID == 3643 || item.ID == 3834) && item.Hue > 0 && ClientFormatEx.ItemIsValid(item))
			{
				ClientFormatEx.runebooks.Enqueue(item);
			}
		}
		Engine.AddTextMessage($"OpenRunebooks: syncing {ClientFormatEx.runebooks.Count} runebooks");
		if (ClientFormatEx.runebooks.Count > 0)
		{
			Network.Send(new PUseRequest(ClientFormatEx.runebooks.Peek()));
		}
	}

	static ClientFormatEx()
	{
		ClientFormatEx.runebooks = new Queue<Item>();
	}

	public static void Recall(string runeName, bool isGate)
	{
		if (ClientFormatEx.IsRecallActive)
		{
			Engine.AddTextMessage("Recall is already in progress");
			return;
		}
		foreach (RuneInfoEx rune in Player.Runes)
		{
			if (string.Equals(rune.Name, runeName, StringComparison.OrdinalIgnoreCase))
			{
				ClientFormatEx.IsRecallActive = true;
				ClientFormatEx.IsGateRecall = isGate;
				ClientFormatEx.LastActiveRune = rune;
				Network.Send(new PUseRequest(rune.RunebookSerial));
				return;
			}
		}
		Engine.AddTextMessage("No rune with that name was found.");
	}

	public static bool ItemIsValid(Item item)
	{
		if (World.Player != null)
		{
			Item item2 = World.FindItem(item.Serial);
			if (item2 != null && (item.ID == 0 || item2.ID == item.ID))
			{
				return item2.InRange(World.Player, 1);
			}
		}
		return false;
	}

	public static void Recall(RuneInfoEx rune, bool isGate)
	{
		if (ClientFormatEx.IsRecallActive)
		{
			Engine.AddTextMessage("Recall is already in progress");
			return;
		}
		foreach (RuneInfoEx rune2 in Player.Runes)
		{
			if (rune.Equals(rune2))
			{
				ClientFormatEx.IsRecallActive = true;
				ClientFormatEx.IsGateRecall = isGate;
				ClientFormatEx.LastActiveRune = rune2;
				Network.Send(new PUseRequest(rune2.RunebookSerial));
				return;
			}
		}
		Engine.AddTextMessage("No rune with that name was found.");
	}
}
