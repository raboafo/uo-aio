namespace UOAIO;

internal class PSetActiveAbility : Packet
{
	public PSetActiveAbility(int index)
		: base(215)
	{
		base.m_Stream.Write(World.Serial);
		base.m_Stream.Write((short)25);
		base.m_Stream.Write(0);
		base.m_Stream.Write((byte)index);
		base.m_Stream.Write((byte)7);
	}
}
