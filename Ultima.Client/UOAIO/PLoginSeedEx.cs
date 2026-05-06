using System;

namespace UOAIO;

public class PLoginSeedEx : Packet
{
	public PLoginSeedEx(uint seed)
		: base(239, 21)
	{
		Version version = Engine.GetVersion();
		base.m_Stream.Write(seed);
		base.m_Stream.Write(version.Major);
		base.m_Stream.Write(version.Minor);
		base.m_Stream.Write(version.Build);
		base.m_Stream.Write(version.Revision);
	}
}
