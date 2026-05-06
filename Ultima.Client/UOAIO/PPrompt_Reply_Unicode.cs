namespace UOAIO;

internal class PPrompt_Reply_Unicode : Packet
{
	public PPrompt_Reply_Unicode(int serial, int prompt, string message)
		: base(194)
	{
		base.m_Stream.Write(serial);
		base.m_Stream.Write(prompt);
		base.m_Stream.Write(1);
		base.m_Stream.Write(Localization.Language, 4);
		base.m_Stream.WriteUnicodeLE(message);
	}
}
