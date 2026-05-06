namespace UOAIO;

internal class PTarget_ForcedCancel : Packet
{
	public PTarget_ForcedCancel(int targetID)
		: base(108, 19)
	{
		base.m_Stream.Write((byte)0);
		base.m_Stream.Write(targetID);
		base.m_Stream.Write((byte)0);
		base.m_Stream.Write(0);
		base.m_Stream.Write(-1);
		base.m_Stream.Write(0);
	}
}
