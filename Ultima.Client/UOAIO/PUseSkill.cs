namespace UOAIO;

internal class PUseSkill : Packet
{
	private static Skill m_LastSkill;

	public static void SendLast()
	{
		if (PUseSkill.m_LastSkill != null)
		{
			Network.Send(new PUseSkill(PUseSkill.m_LastSkill));
		}
	}

	public PUseSkill(Skill skill)
		: base(18)
	{
		PUseSkill.m_LastSkill = skill;
		base.m_Stream.Write((byte)36);
		base.m_Stream.Write($"{skill.ID} 0");
		base.m_Stream.Write((byte)0);
	}
}
