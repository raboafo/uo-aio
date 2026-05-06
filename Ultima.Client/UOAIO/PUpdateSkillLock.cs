namespace UOAIO;

internal class PUpdateSkillLock : Packet
{
	public PUpdateSkillLock(Skill skill)
		: base(58)
	{
		base.m_Stream.Write((short)skill.ID);
		base.m_Stream.Write((byte)skill.Lock);
	}
}
