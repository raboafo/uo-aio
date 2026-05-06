using System.Collections;

namespace UOAIO;

public class PUpdateRange : Packet
{
	private static Queue m_Queue;

	public static Queue Queue => PUpdateRange.m_Queue;

	public static void Dispatch(object state)
	{
		PUpdateRange.m_Queue.Enqueue(state);
		Network.Send(new PUpdateRange(18));
	}

	private PUpdateRange(int range)
		: base(200, 2)
	{
		World.Range = range;
		base.m_Stream.Write((byte)range);
	}

	static PUpdateRange()
	{
		PUpdateRange.m_Queue = new Queue();
	}
}
