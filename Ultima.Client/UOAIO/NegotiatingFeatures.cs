namespace UOAIO;

internal class NegotiatingFeatures : Packet
{
	public NegotiatingFeatures()
		: base(240, 16)
	{
		base.m_Stream.Write((byte)0);
		base.m_Stream.Write((byte)4);
		base.m_Stream.Write(byte.MaxValue);
	}
}
