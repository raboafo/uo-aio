namespace UOAIO;

internal class PPE_QueryRunebookContentEx : Packet
{
	public PPE_QueryRunebookContentEx(Item item, byte b)
		: base(240)
	{
		base.m_Stream.Write(b);
		base.m_Stream.Write(item.Serial);
	}
}
