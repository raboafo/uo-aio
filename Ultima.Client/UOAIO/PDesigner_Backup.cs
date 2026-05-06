namespace UOAIO;

internal class PDesigner_Backup : Packet
{
	public PDesigner_Backup(Item house)
		: base(215)
	{
		base.m_Stream.Write(house.Serial);
		base.m_Stream.Write((short)2);
	}
}
