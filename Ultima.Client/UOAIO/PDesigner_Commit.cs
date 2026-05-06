namespace UOAIO;

internal class PDesigner_Commit : Packet
{
	public PDesigner_Commit(Item house)
		: base(215)
	{
		base.m_Stream.Write(house.Serial);
		base.m_Stream.Write((short)4);
	}
}
