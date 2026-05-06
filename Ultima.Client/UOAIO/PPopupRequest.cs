namespace UOAIO;

internal class PPopupRequest : Packet
{
	public PPopupRequest(Mobile Target)
		: this(Target.Serial)
	{
	}

	public PPopupRequest(Item Target)
		: this(Target.Serial)
	{
	}

	public PPopupRequest(MobileCell Target)
		: this(Target.m_Mobile.Serial)
	{
	}

	protected PPopupRequest(int Serial)
		: base(191)
	{
		base.m_Stream.Write((short)19);
		base.m_Stream.Write(Serial);
	}
}
