namespace UOAIO;

internal class PSpeech : Packet
{
	public PSpeech(string toSay, SpeechFormat speechFormat)
		: base(3)
	{
		base.m_Stream.Write(speechFormat.MessageType);
		base.m_Stream.Write((short)speechFormat.Hue);
		base.m_Stream.Write((short)3);
		base.m_Stream.Write(toSay);
		base.m_Stream.Write((byte)0);
	}
}
