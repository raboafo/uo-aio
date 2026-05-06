namespace UOAIO;

internal class PAttackRequest : Packet
{
	public PAttackRequest(Mobile Target)
		: this(Target.Serial)
	{
	}

	public PAttackRequest(MobileCell Target)
		: this(Target.m_Mobile.Serial)
	{
	}

	protected PAttackRequest(int Serial)
		: base(5, 5)
	{
		base.m_Stream.Write(Serial);
	}
}
