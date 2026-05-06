using System.Linq;

namespace UOAIO;

public static class Guild
{
	private static Mobile[] m_Members;

	public static Mobile[] Members
	{
		get
		{
			return Guild.m_Members;
		}
		set
		{
			Guild.m_Members = value;
		}
	}

	static Guild()
	{
		Guild.m_Members = new Mobile[0];
	}

	public static bool IsMember(Mobile mobile)
	{
		return Guild.m_Members.Contains(mobile);
	}
}
