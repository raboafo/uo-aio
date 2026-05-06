namespace UOAIO;

public class POpenPaperdoll : Packet
{
	public POpenPaperdoll()
		: this(World.Serial)
	{
	}

	public POpenPaperdoll(int serial)
		: base(6, 5)
	{
		base.m_Stream.Write(serial | int.MinValue);
	}
}
