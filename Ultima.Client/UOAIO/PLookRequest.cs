namespace UOAIO;

internal class PLookRequest : Packet
{
	public PLookRequest(IEntity e)
		: base(9, 5)
	{
		base.m_Stream.Write(e.Serial);
	}
}
