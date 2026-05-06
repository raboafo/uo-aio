namespace UOAIO;

internal class PEquipItem : Packet
{
	public PEquipItem(Item toEquip, Mobile target)
		: base(19, 10)
	{
		base.m_Stream.Write(toEquip.Serial);
		base.m_Stream.Write(Map.GetQuality(toEquip.ID));
		base.m_Stream.Write(target.Serial);
	}
}
