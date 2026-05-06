namespace UOAIO;

internal class PWrestleDisarm : Packet
{
	public PWrestleDisarm()
		: base(191)
	{
		base.m_Stream.Write((short)9);
	}
}
