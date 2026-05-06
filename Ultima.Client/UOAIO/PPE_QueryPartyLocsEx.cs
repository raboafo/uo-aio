namespace UOAIO;

internal class PPE_QueryPartyLocsEx : Packet
{
	public PPE_QueryPartyLocsEx()
		: base(240)
	{
		base.m_Stream.Write((byte)0);
	}
}
