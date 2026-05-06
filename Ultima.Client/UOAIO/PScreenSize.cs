namespace UOAIO;

public class PScreenSize : Packet
{
	public PScreenSize()
		: base(191)
	{
		base.m_Stream.Write((short)5);
		base.m_Stream.Write(Engine.GameWidth);
		base.m_Stream.Write(33554343);
	}
}
