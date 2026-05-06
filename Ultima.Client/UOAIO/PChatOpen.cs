namespace UOAIO;

internal class PChatOpen : Packet
{
	public PChatOpen(string un)
		: base(181, 64)
	{
		base.m_Stream.Write((byte)1);
		if (un.Length > 31)
		{
			un = un.Substring(0, 31);
		}
		else if (un.Length < 31)
		{
			un += new string('\0', 31 - un.Length);
		}
		base.m_Stream.WriteUnicode(un);
	}
}
