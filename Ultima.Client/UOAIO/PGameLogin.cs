namespace UOAIO;

internal class PGameLogin : Packet
{
	public PGameLogin(int AuthID, string account, string password)
		: base(145, 65)
	{
		base.m_Stream.Write(AuthID);
		base.m_Stream.Write(account, 30);
		base.m_Stream.Write(password, 30);
	}
}