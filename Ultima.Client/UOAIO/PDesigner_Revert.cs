namespace UOAIO;

internal class PDesigner_Revert : Packet
{
	public PDesigner_Revert(Item house)
		: base(215)
	{
		base.m_Stream.Write(house.Serial);
		base.m_Stream.Write((short)26);
	}
}
