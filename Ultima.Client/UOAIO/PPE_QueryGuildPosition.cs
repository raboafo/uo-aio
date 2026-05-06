namespace UOAIO;

public class PPE_QueryGuildPosition : Packet
{
	public PPE_QueryGuildPosition()
		: base(240)
	{
		byte toWrite = 1;
		base.m_Stream.Write(toWrite);
		base.m_Stream.Write(toWrite: true);
	}
}
