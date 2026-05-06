namespace UOAIO;

internal class PParty_Decline : Packet
{
	public PParty_Decline(Mobile req)
		: base(191)
	{
		base.m_Stream.Write((short)6);
		base.m_Stream.Write((byte)9);
		base.m_Stream.Write(req.Serial);
	}
}
