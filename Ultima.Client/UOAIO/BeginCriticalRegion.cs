namespace UOAIO;

internal class BeginCriticalRegion : Packet
{
	public BeginCriticalRegion()
		: base(115, 2)
	{
		base.m_Stream.Write((byte)0);
	}
}
