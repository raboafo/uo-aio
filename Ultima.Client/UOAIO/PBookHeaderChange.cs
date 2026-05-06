namespace UOAIO;

internal class PBookHeaderChange : Packet
{
	public PBookHeaderChange(int serial, string title, string author)
		: base(212)
	{
		base.m_Stream.Write(serial);
		base.m_Stream.Write((byte)0);
		base.m_Stream.Write((byte)0);
		base.m_Stream.Write((short)0);
		base.m_Stream.Write((short)title.Length);
		base.m_Stream.Write(title, title.Length);
		base.m_Stream.Write((short)author.Length);
		base.m_Stream.Write(author, author.Length);
	}
}
