using System;
using UOAIO.Profiles;

namespace UOAIO;

public static class Party
{
	private static PartyState m_State;

	private static Mobile m_Leader;

	private static Mobile[] m_Members;

	public static PartyState State
	{
		get
		{
			return Party.m_State;
		}
		set
		{
			Party.m_State = value;
			if (Party.m_State != PartyState.Joined)
			{
				Party.m_Members = new Mobile[0];
				Party.m_Leader = null;
			}
		}
	}

	public static bool IsLeader => Party.m_Leader != null && Party.m_Leader.Player;

	public static int Index => Array.IndexOf(Party.m_Members, World.Player);

	public static Mobile Leader
	{
		get
		{
			return Party.m_Leader;
		}
		set
		{
			Party.m_Leader = value;
			if (Party.m_Members.Length != 0)
			{
				Party.m_Members[0] = Party.m_Leader;
			}
		}
	}

	public static Mobile[] Members
	{
		get
		{
			return Party.m_Members;
		}
		set
		{
			Party.m_Members = value;
			Party.m_Leader = ((Party.m_Members.Length != 0) ? Party.m_Members[0] : null);
			Party.m_State = ((Party.m_Members.Length != 0 && Party.Index >= 0) ? PartyState.Joined : PartyState.Alone);
		}
	}

	public static void SendAutomatedMessage(string text)
	{
	}

	public static void SendAutomatedMessage(string format, params object[] args)
	{
		if (Preferences.Current.Options.PartyNotifications)
		{
			Party.SendAutomatedMessage(string.Format(format, args));
		}
	}

	public static bool CheckAutomatedAccept()
	{
		if (Party.State == PartyState.Joining)
		{
			Mobile player = World.Player;
			if (player != null && player.Guild != null)
			{
				Mobile leader = Party.Leader;
				if (leader != null && leader.Guild == player.Guild)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CheckAutomatedInvite(Mobile mob)
	{
		if (Party.State == PartyState.Joined && Party.IsLeader && Party.Members.Length < 10)
		{
			Mobile player = World.Player;
			if (player != null && player.Guild != null && mob != null && mob.Guild == player.Guild && !mob.IsInParty && mob.InRange(player, 8))
			{
				return true;
			}
		}
		return false;
	}

	static Party()
	{
		Party.m_Members = new Mobile[0];
	}
}
