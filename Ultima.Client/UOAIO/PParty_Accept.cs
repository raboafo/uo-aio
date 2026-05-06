namespace UOAIO;

internal class PParty_Accept : Packet
{
	public PParty_Accept(Mobile req)
		: base(191)
	{
		base.m_Stream.Write((short)6);
		base.m_Stream.Write((byte)8);
		base.m_Stream.Write(req.Serial);
	}
}
