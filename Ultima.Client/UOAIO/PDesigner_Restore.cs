namespace UOAIO;

internal class PDesigner_Restore : Packet
{
	public PDesigner_Restore(Item house)
		: base(215)
	{
		base.m_Stream.Write(house.Serial);
		base.m_Stream.Write((short)3);
	}
}
