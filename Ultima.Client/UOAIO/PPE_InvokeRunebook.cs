namespace UOAIO;

internal class PPE_InvokeRunebook : Packet
{
	public PPE_InvokeRunebook(Item book, RuneInfo rune, bool recall)
		: base(240)
	{
		base.m_Stream.Write((byte)6);
		base.m_Stream.Write(book.Serial);
		base.m_Stream.Write((!recall) ? ((byte)1) : ((byte)0));
		base.m_Stream.Write((short)rune.Point.X);
		base.m_Stream.Write((short)rune.Point.Y);
		base.m_Stream.Write((byte)rune.Facet);
	}
}
