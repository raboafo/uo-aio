using UOAIO.Targeting;

namespace UOAIO;

internal class PTarget_Spoof : Packet
{
	public PTarget_Spoof(int type, int tid, AggressionType flags, int serial, int x, int y, int z, int id)
		: base(108, 19)
	{
		base.m_Stream.Write((byte)type);
		base.m_Stream.Write(tid);
		base.m_Stream.Write((byte)flags);
		base.m_Stream.Write(serial);
		base.m_Stream.Write((short)x);
		base.m_Stream.Write((short)y);
		base.m_Stream.Write((short)z);
		base.m_Stream.Write((short)id);
	}
}
