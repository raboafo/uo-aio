namespace UOAIO;

public class PCharacterSelect : Packet
{
	public PCharacterSelect(string name, int index, int address)
		: base(93, 73)
	{
		base.m_Stream.Write(3991793133u);
		base.m_Stream.Write(name, 30);
		base.m_Stream.Write((short)0);
		base.m_Stream.Write(31);
		base.m_Stream.Write(1);
		base.m_Stream.Write(24);
		base.m_Stream.Write("", 16);
		base.m_Stream.Write(index);
		base.m_Stream.Write(address);
	}
}
