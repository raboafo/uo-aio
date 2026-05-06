namespace UOAIO;

public class PUnknownLogin : Packet
{
	public PUnknownLogin()
		: base(191)
	{
		base.m_Stream.Write((short)15);
		base.m_Stream.Write((byte)10);
		base.m_Stream.Write(319);
	}
}
