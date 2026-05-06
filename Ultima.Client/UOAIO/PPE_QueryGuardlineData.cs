namespace UOAIO;

internal class PPE_QueryGuardlineData : Packet
{
	public PPE_QueryGuardlineData()
		: base(240)
	{
		base.m_Stream.Write((byte)4);
	}
}
