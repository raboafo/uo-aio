namespace UOAIO;

public class PSetLanguage : Packet
{
	public PSetLanguage()
		: base(191)
	{
		base.m_Stream.Write((short)11);
		base.m_Stream.Write(Localization.Language, 4);
	}
}
