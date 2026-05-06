namespace UOAIO;

internal class PProfileRequest : Packet
{
	public PProfileRequest(Mobile owner)
		: base(184)
	{
		base.m_Stream.Write((byte)0);
		base.m_Stream.Write(owner.Serial);
	}
}
