namespace UOAIO;

internal class POpenDoor : Packet
{
	public POpenDoor()
		: base(18)
	{
		base.m_Stream.Write((byte)88);
		base.m_Stream.Write((byte)0);
	}
}
