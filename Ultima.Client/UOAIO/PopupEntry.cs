namespace UOAIO;

public class PopupEntry
{
	private int m_EntryID;

	private string m_Text;

	private int m_Flags;

	public int EntryID => this.m_EntryID;

	public string Text => this.m_Text;

	public int Flags => this.m_Flags;

	public PopupEntry(int EntryID, int Cliloc, int Flags)
	{
		this.m_EntryID = EntryID;
		this.m_Flags = Flags;
		this.m_Text = Localization.GetString(Cliloc);
		if ((Flags & -34) != 0)
		{
			this.m_Text = $"0x{Flags:X4} {this.m_Text}";
		}
	}
}
