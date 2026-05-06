namespace UOAIO;

internal class PDesigner_Sync : Packet
{
	public PDesigner_Sync(Item house)
		: base(215)
	{
		base.m_Stream.Write(house.Serial);
		base.m_Stream.Write((short)14);
	}
}
