namespace UOAIO;

internal class PQuestionMenuResponse : Packet
{
	public PQuestionMenuResponse(int serial, int menuID, int index, int itemID, int hue)
		: base(125, 13)
	{
		base.m_Stream.Write(serial);
		base.m_Stream.Write((short)menuID);
		base.m_Stream.Write((short)(index + 1));
		base.m_Stream.Write((short)itemID);
		base.m_Stream.Write((short)hue);
	}
}
