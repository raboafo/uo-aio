namespace UOAIO;

internal class PPE_QueryRunebookContent : Packet
{
	public PPE_QueryRunebookContent(Item item)
		: base(240)
	{
		byte toWrite = 2;
		base.m_Stream.Write(toWrite);
		base.m_Stream.Write(item.Serial);
	}
}
