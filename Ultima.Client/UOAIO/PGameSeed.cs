using System.IO;

namespace UOAIO;

internal class PGameSeed : Packet
{
	public PGameSeed(uint gameSeed)
		: base(0x00, 4)
	{
		base.m_Encode = false;
		base.m_Stream.Seek(0L, SeekOrigin.Begin);
		base.m_Stream.Write(gameSeed);
	}
}
