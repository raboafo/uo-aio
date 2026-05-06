namespace UOAIO;

internal class PDesigner_Level : Packet
{
	public PDesigner_Level(Item house, int level)
		: base(215)
	{
		base.m_Stream.Write(house.Serial);
		base.m_Stream.Write((short)18);
		base.m_Stream.WriteEncoded(level);
	}
}
