using System.IO;

namespace UOAIO;

internal class PLoginSeed : Packet
{
	public PLoginSeed(uint loginSeed)
		: base(0, 4)
	{
		base.m_Encode = false;
		base.m_Stream.Seek(0L, SeekOrigin.Begin);
		base.m_Stream.Write(loginSeed);
	}
}
