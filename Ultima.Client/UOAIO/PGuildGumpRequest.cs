namespace UOAIO;

internal class PGuildGumpRequest : Packet
{
	public PGuildGumpRequest()
		: base(215)
	{
		base.m_Stream.Write(World.Serial);
		base.m_Stream.Write((short)40);
	}
}
