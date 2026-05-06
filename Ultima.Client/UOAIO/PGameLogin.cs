namespace UOAIO;

internal class PGameLogin : Packet
{
	public PGameLogin(int AuthID, string un, string pw)
		: base(145, 65)
	{
		base.m_Stream.Write(AuthID);
		base.m_Stream.Write(un, 30);
		base.m_Stream.Write(pw, 30);
	}

	public PGameLogin(int AuthID, string un, string pw, string jwt)
		: base(145, 1065)
	{
		base.m_Stream.Write(AuthID);
		base.m_Stream.Write(un, 30);
		base.m_Stream.Write(pw, 30);
		base.m_Stream.Write(jwt, 1000);
	}
}
