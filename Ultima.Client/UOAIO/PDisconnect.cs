namespace UOAIO;

internal class PDisconnect : Packet
{
	public PDisconnect()
		: base(1, 5)
	{
		base.m_Stream.Write(-1);
	}
}
