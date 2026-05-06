namespace UOAIO;

public class PClientType : Packet
{
	public PClientType()
		: base(191)
	{
		ushort toWrite = 15;
		byte toWrite2 = 10;
		base.m_Stream.Write(toWrite);
		base.m_Stream.Write(toWrite2);
		base.m_Stream.Write(uint.MaxValue);
	}
}
