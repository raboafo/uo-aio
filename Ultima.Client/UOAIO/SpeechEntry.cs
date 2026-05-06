using System;

namespace UOAIO;

public class SpeechEntry : IComparable
{
	public string[] m_Keywords;

	public short m_KeywordID;

	public SpeechEntry(int idKeyword, string keyword)
	{
		this.m_KeywordID = (short)idKeyword;
		this.m_Keywords = keyword.Split('*');
	}

	public int CompareTo(object x)
	{
		if (x == null || x.GetType() != typeof(SpeechEntry))
		{
			return -1;
		}
		if (x == this)
		{
			return 0;
		}
		SpeechEntry speechEntry = (SpeechEntry)x;
		if (this.m_KeywordID < speechEntry.m_KeywordID)
		{
			return -1;
		}
		if (this.m_KeywordID > speechEntry.m_KeywordID)
		{
			return 1;
		}
		return 0;
	}
}
