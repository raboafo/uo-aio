namespace UOAIO;

internal class PResurrectionResponse : Packet
{
	public PResurrectionResponse(bool ShouldResurrect)
		: base(44, 2)
	{
		base.m_Stream.Write((byte)(ShouldResurrect ? 1u : 2u));
	}
}
