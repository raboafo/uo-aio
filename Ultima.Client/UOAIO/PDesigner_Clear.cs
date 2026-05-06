namespace UOAIO;

internal class PDesigner_Clear : Packet
{
	public PDesigner_Clear(Item house)
		: base(215)
	{
		base.m_Stream.Write(house.Serial);
		base.m_Stream.Write((short)16);
	}
}
