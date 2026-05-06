namespace UOAIO;

internal class PCancelTrade : Packet
{
	public PCancelTrade(int Serial)
		: base(111)
	{
		base.m_Stream.Write((byte)1);
		base.m_Stream.Write(Serial);
		base.m_Stream.Write(0);
		base.m_Stream.Write(0);
		base.m_Stream.Write((byte)0);
	}
}
