namespace UOAIO;

internal class PUpdateStatLock : Packet
{
	public PUpdateStatLock(Stat stat)
		: base(191)
	{
		base.m_Stream.Write((short)26);
		base.m_Stream.Write((byte)stat.ID);
		base.m_Stream.Write((byte)stat.Lock);
	}
}
