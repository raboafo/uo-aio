namespace UOAIO;

internal class PVirtueTrigger : Packet
{
	public PVirtueTrigger(Mobile m)
		: base(177)
	{
		base.m_Stream.Write(World.Serial);
		base.m_Stream.Write(461);
		base.m_Stream.Write(1);
		base.m_Stream.Write(1);
		base.m_Stream.Write(m.Serial);
	}
}
