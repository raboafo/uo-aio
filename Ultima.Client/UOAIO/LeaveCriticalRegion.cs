namespace UOAIO;

internal class LeaveCriticalRegion : Packet
{
	public LeaveCriticalRegion()
		: base(115, 2)
	{
		base.m_Stream.Write((byte)1);
	}
}
