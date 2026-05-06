namespace UOAIO;

internal class POpenSpellbook : Packet
{
	public POpenSpellbook(int num)
		: base(18)
	{
		base.m_Stream.Write((byte)67);
		base.m_Stream.Write(num.ToString());
		base.m_Stream.Write((byte)0);
	}
}
