namespace UOAIO;

internal class PMultiTarget_Response : Packet
{
	public PMultiTarget_Response(int targetID, int x, int y, int z, int id)
		: base(108, 19)
	{
		base.m_Stream.Write((byte)1);
		base.m_Stream.Write(targetID);
		base.m_Stream.Write((byte)0);
		base.m_Stream.Write(0);
		base.m_Stream.Write((short)x);
		base.m_Stream.Write((short)y);
		base.m_Stream.Write((short)z);
		base.m_Stream.Write((short)id);
	}
}
