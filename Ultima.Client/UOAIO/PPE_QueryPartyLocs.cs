namespace UOAIO;

internal class PPE_QueryPartyLocs : Packet
{
	public PPE_QueryPartyLocs()
		: base(240)
	{
		base.m_Stream.Write((byte)0);
	}
}
