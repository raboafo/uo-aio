namespace UOAIO;

internal class PParty_SetCanLoot : Packet
{
	public PParty_SetCanLoot(bool val)
		: base(191)
	{
		base.m_Stream.Write((short)6);
		base.m_Stream.Write((byte)6);
		base.m_Stream.Write(val);
	}
}
