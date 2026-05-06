using System.Text;

namespace UOAIO;

internal class PUnicodeSpeech : Packet
{
	public PUnicodeSpeech(string toSay, bool getKeywords, SpeechFormat speechFormat)
		: base(173)
	{
		SpeechEntry[] keywords = Speech.GetKeywords(toSay);
		byte b = speechFormat.MessageType;
		if (getKeywords && keywords.Length != 0)
		{
			b |= 0xC0;
		}
		base.m_Stream.Write(b);
		base.m_Stream.Write((short)speechFormat.Hue);
		base.m_Stream.Write((short)3);
		base.m_Stream.Write(Localization.Language, 4);
		if (!getKeywords || keywords.Length == 0)
		{
			base.m_Stream.WriteUnicode(toSay);
			base.m_Stream.Write((short)0);
			return;
		}
		base.m_Stream.Write((byte)(keywords.Length >> 4));
		int num = keywords.Length & 0xF;
		bool flag = false;
		int num2 = 0;
		while (num2 < keywords.Length)
		{
			SpeechEntry speechEntry = keywords[num2];
			int keywordID = speechEntry.m_KeywordID;
			if (flag)
			{
				base.m_Stream.Write((byte)(keywordID >> 4));
				num = keywordID & 0xF;
			}
			else
			{
				base.m_Stream.Write((byte)((num << 4) | ((keywordID >> 8) & 0xF)));
				base.m_Stream.Write((byte)keywordID);
			}
			num2++;
			flag = !flag;
		}
		if (!flag)
		{
			base.m_Stream.Write((byte)(num << 4));
		}
		base.m_Stream.Write(Encoding.UTF8.GetBytes(toSay));
		base.m_Stream.Write((byte)0);
	}
}
