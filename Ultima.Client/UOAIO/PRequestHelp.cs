namespace UOAIO;

internal class PRequestHelp : Packet
{
	public PRequestHelp()
		: base(155, 258)
	{
		base.m_Stream.Write(new byte[257]);
	}
}
