namespace UOAIO;

internal class PParty_RemoveMember : Packet
{
	public PParty_RemoveMember()
		: base(191)
	{
		base.m_Stream.Write((short)6);
		base.m_Stream.Write((byte)2);
		base.m_Stream.Write(0);
	}

	public PParty_RemoveMember(int serial)
		: base(191)
	{
		base.m_Stream.Write((short)6);
		base.m_Stream.Write((byte)2);
		base.m_Stream.Write(serial);
	}
}
