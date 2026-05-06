using System.Collections.Generic;

namespace UOAIO;

public class MiniHealthEntry
{
	public int m_X;

	public int m_Y;

	public Mobile m_Mobile;

	private static Queue<MiniHealthEntry> m_Pool;

	public static MiniHealthEntry PoolInstance(int x, int y, Mobile m)
	{
		if (MiniHealthEntry.m_Pool == null)
		{
			MiniHealthEntry.m_Pool = new Queue<MiniHealthEntry>();
		}
		if (MiniHealthEntry.m_Pool.Count > 0)
		{
			MiniHealthEntry miniHealthEntry = MiniHealthEntry.m_Pool.Dequeue();
			miniHealthEntry.m_X = x;
			miniHealthEntry.m_Y = y;
			miniHealthEntry.m_Mobile = m;
			return miniHealthEntry;
		}
		return new MiniHealthEntry(x, y, m);
	}

	private MiniHealthEntry(int x, int y, Mobile m)
	{
		this.m_X = x;
		this.m_Y = y;
		this.m_Mobile = m;
	}

	public void Dispose()
	{
		MiniHealthEntry.m_Pool.Enqueue(this);
	}
}
