namespace UOAIO;

internal class PRenameMobile : Packet
{
	public PRenameMobile(int Serial, string Name)
		: base(117, 35)
	{
		base.m_Stream.Write(Serial);
		base.m_Stream.Write(Name, 30);
	}
}
