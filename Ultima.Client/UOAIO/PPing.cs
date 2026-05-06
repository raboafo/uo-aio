namespace UOAIO;

internal class PPing : Packet
{
	public PPing(int PingID)
		: base(115, 2)
	{
		base.m_Stream.Write((byte)PingID);
	}
}
