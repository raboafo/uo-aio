namespace UOAIO;

internal class PSelectHueResponse : Packet
{
	public PSelectHueResponse(int Serial, short Relay, short Hue)
		: base(149, 9)
	{
		base.m_Stream.Write(Serial);
		base.m_Stream.Write(Relay);
		base.m_Stream.Write(Hue);
	}
}
