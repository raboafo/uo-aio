namespace UOAIO;

internal class PLogoutOk : Packet
{
	public PLogoutOk()
		: base(209, 2)
	{
		base.m_Stream.Write((byte)1);
	}
}
