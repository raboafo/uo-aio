using UOAIO.Targeting;

namespace UOAIO;

public class PTarget_Cancel : Packet
{
	public PTarget_Cancel(ServerTargetHandler handler)
		: base(108, 19)
	{
		base.m_Stream.Write((byte)0);
		base.m_Stream.Write(handler.TargetID);
		base.m_Stream.Write((byte)handler.Aggression);
		base.m_Stream.Write(0);
		base.m_Stream.Write(-1);
		base.m_Stream.Write(0);
	}
}
