namespace UOAIO;

internal class PParty_AddMember : Packet
{
	public PParty_AddMember()
		: base(191)
	{
		base.m_Stream.Write((short)6);
		base.m_Stream.Write((byte)1);
		base.m_Stream.Write(0);
	}
}
