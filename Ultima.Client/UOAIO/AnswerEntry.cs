namespace UOAIO;

public class AnswerEntry
{
	private int m_Index;

	private int m_ItemID;

	private int m_Hue;

	private string m_Text;

	public int Index => this.m_Index;

	public int ItemID => this.m_ItemID;

	public int Hue => this.m_Hue;

	public string Text => this.m_Text;

	public AnswerEntry(int index, int itemID, int hue, string text)
	{
		this.m_Index = index;
		this.m_ItemID = itemID;
		this.m_Hue = hue;
		this.m_Text = text;
	}
}
