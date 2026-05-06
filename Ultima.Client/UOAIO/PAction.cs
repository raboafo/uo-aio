namespace UOAIO;

internal class PAction : Packet
{
	public PAction(string Action)
		: base(18)
	{
		base.m_Stream.Write((byte)199);
		base.m_Stream.Write(Action);
		base.m_Stream.Write((byte)0);
	}
}
