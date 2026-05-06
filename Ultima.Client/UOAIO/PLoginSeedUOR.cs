using System;
using System.IO;

namespace UOAIO;

internal class PLoginSeedUOR : Packet
{
	public PLoginSeedUOR()
		: base(0, 25)
	{
		Version version = Engine.GetVersion();
		base.m_Encode = false;
		base.m_Stream.Seek(0L, SeekOrigin.Begin);
		byte toWrite = 239;
		base.m_Stream.Write(toWrite);
		base.m_Stream.Write(2212936458u);
		base.m_Stream.Write(version.Major);
		base.m_Stream.Write(version.Minor);
		base.m_Stream.Write(version.Build);
		base.m_Stream.Write(version.Revision);
		base.m_Stream.Write(2404003452u);
	}
}
