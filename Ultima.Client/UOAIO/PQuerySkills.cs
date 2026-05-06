namespace UOAIO;

public class PQuerySkills : Packet
{
	public PQuerySkills()
		: base(52, 10)
	{
		base.m_Stream.Write(-303174163);
		base.m_Stream.Write((byte)5);
		base.m_Stream.Write(World.Serial);
	}
}
