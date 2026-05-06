namespace UOAIO;

public class PWrestleStun : Packet
{
	public PWrestleStun()
		: base(191)
	{
		base.m_Stream.Write((short)10);
	}
}
