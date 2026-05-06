namespace UOAIO;

internal class PDropItem : Packet
{
	public PDropItem(int Serial, int X, int Y, int Z, int DestSerial)
		: base(8, 14)
	{
		base.m_Stream.Write(Serial);
		base.m_Stream.Write((short)X);
		base.m_Stream.Write((short)Y);
		base.m_Stream.Write((sbyte)Z);
		base.m_Stream.Write((sbyte)(-1));
		base.m_Stream.Write(DestSerial);
	}
}
