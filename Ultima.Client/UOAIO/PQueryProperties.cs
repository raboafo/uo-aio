namespace UOAIO;

internal class PQueryProperties : Packet
{
	public PQueryProperties(int serial)
		: base(214)
	{
		base.m_Stream.Write(serial);
	}
}
