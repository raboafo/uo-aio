namespace UOAIO;

internal class PUseRequest : Packet
{
	private static IEntity m_Last;

	public static IEntity Last
	{
		get
		{
			return PUseRequest.m_Last;
		}
		set
		{
			PUseRequest.m_Last = value;
		}
	}

	public static void SendLast()
	{
		if (PUseRequest.m_Last != null)
		{
			if (PUseRequest.m_Last is Item)
			{
				(PUseRequest.m_Last as Item).Use();
			}
			else
			{
				Network.Send(new PUseRequest(PUseRequest.m_Last));
			}
		}
	}

	public PUseRequest(IEntity e)
		: base(6, 5)
	{
		base.m_Stream.Write(e.Serial);
	}

	public PUseRequest(int serial)
		: base(6, 5)
	{
		base.m_Stream.Write(serial);
	}
}
