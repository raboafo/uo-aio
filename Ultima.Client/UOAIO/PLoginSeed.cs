using System;

namespace UOAIO;

public class PLoginSeed : Packet
{
	public PLoginSeed(uint seed, Version version)
		: base(0xEF, 21)
	{
		base.m_Stream.Write(seed);
		base.m_Stream.Write(version.Major);
		base.m_Stream.Write(version.Minor);
		base.m_Stream.Write(version.Build);
		base.m_Stream.Write(version.Revision);
	}
}
