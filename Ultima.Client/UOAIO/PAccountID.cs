namespace UOAIO;

internal class PAccountID : Packet
{
	public PAccountID()
		: base(187, 7)
	{
		base.m_Stream.Write(World.Serial);
		base.m_Stream.Write(World.Serial);
	}
}
