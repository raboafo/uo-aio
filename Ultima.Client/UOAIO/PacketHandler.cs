namespace UOAIO;

internal class PacketHandler
{
	private PacketCallback m_Callback;

	private int m_PacketID;

	private int m_Length;

	private int m_Count;

	public PacketCallback Callback => this.m_Callback;

	public int PacketID => this.m_PacketID;

	public int Length
	{
		get
		{
			return this.m_Length;
		}
		set
		{
			this.m_Length = value;
		}
	}

	public int Count => this.m_Count;

	public PacketHandler(int packetID, int length, PacketCallback callback)
	{
		this.m_Callback = callback;
		this.m_PacketID = packetID;
		this.m_Length = length;
	}

	public void Handle(PacketReader pvSrc)
	{
		this.m_Callback(pvSrc);
		this.m_Count++;
	}
}
