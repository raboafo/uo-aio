namespace UOAIO;

internal class PQueryCustomHouse : Packet
{
	public PQueryCustomHouse(int serial)
		: base(191)
	{
		base.m_Stream.Write((short)30);
		base.m_Stream.Write(serial);
	}
}
