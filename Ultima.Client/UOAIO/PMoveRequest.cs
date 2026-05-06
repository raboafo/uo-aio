using System;

namespace UOAIO;

internal class PMoveRequest : Packet
{
	public PMoveRequest(int dir, int seq, uint key, int x, int y, int z, TimeSpan speed)
		: base(2, 7)
	{
		base.m_Stream.Write((byte)dir);
		base.m_Stream.Write((byte)seq);
		base.m_Stream.Write(key);
		PacketHandlers.AddSequence(seq, x, y, z, speed);
	}
}
