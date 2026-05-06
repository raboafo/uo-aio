namespace UOAIO;

internal class PBookPageChange : Packet
{
	public PBookPageChange(int serial, int page, string[] lines)
		: base(102)
	{
		base.m_Stream.Write(serial);
		base.m_Stream.Write((short)1);
		base.m_Stream.Write((short)(page + 1));
		base.m_Stream.Write((short)lines.Length);
		for (int i = 0; i < lines.Length; i++)
		{
			base.m_Stream.Write(lines[i]);
		}
	}
}
