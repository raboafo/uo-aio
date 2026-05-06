namespace UOAIO;

internal class PResyncRequest : Packet
{
	public PResyncRequest()
		: base(34, 3)
	{
		base.m_Stream.Write((short)0);
	}
}
