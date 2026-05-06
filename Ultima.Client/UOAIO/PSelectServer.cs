namespace UOAIO;

internal class PSelectServer : Packet
{
	public PSelectServer(short id)
		: base(160, 3)
	{
		base.m_Stream.Write(id);
	}
}
