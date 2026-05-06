namespace UOAIO;

internal class PVirtueItemTrigger : Packet
{
	public PVirtueItemTrigger(GServerGump owner, int gumpID)
		: base(177)
	{
		base.m_Stream.Write(owner.Serial);
		base.m_Stream.Write(461);
		base.m_Stream.Write(gumpID);
	}
}
