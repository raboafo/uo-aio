using System;

namespace UOAIO;

internal class PAccount : Packet
{
	public PAccount(string account, string password)
		: base(0x80, 62)
	{
		base.m_Encode = false;
		base.m_Stream.Write(account, 30);
		base.m_Stream.Write(password, 30);
		base.m_Stream.Write(byte.MaxValue);
	}
}
