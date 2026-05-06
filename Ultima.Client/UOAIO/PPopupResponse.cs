namespace UOAIO;

internal class PPopupResponse : Packet
{
	public PPopupResponse(object o, int EntryID)
		: base(191)
	{
		base.m_Stream.Write((short)21);
		base.m_Stream.Write((o is Item) ? ((Item)o).Serial : ((Mobile)o).Serial);
		base.m_Stream.Write((short)EntryID);
	}
}
