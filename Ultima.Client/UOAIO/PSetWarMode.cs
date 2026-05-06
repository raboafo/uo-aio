namespace UOAIO;

internal class PSetWarMode : Packet
{
	public PSetWarMode(bool warMode, short unk1, byte unk2)
		: base(114, 5)
	{
		base.m_Stream.Write(warMode);
		base.m_Stream.Write(unk1);
		base.m_Stream.Write(unk2);
	}
}
