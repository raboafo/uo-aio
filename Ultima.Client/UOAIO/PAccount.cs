using System;

namespace UOAIO;

internal class PAccount : Packet
{
	public PAccount(string un, string pw)
		: base(128)
	{
		base.m_Encode = false;
		base.m_Stream.Write(un, 30);
		base.m_Stream.Write(pw, 30);
		base.m_Stream.Write(byte.MaxValue);
	}

	public PAccount(string un, string pw, string jwt)
		: base(128, 1062)
	{
		ushort toWrite = 255;
		base.m_Encode = false;
		base.m_Stream.Write(un, 30);
		base.m_Stream.Write(pw, 30);
		base.m_Stream.Write(jwt);
		base.m_Stream.Write(toWrite);
		int val = 1000 - jwt.Length - 1;
		val = Math.Max(0, val);
		base.m_Stream.Write(new byte[val]);
	}
}
