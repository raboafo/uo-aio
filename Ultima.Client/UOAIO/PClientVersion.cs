namespace UOAIO;

public class PClientVersion : Packet
{
	public PClientVersion(string version)
		: base(189)
	{
		base.m_Stream.Write(version);
		base.m_Stream.Write((byte)0);
	}
}
