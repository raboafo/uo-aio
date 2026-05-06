namespace UOAIO;

public class Stats
{
	private static int m_yOffset;

	public static int yOffset => Stats.m_yOffset;

	public static void Reset()
	{
		Stats.m_yOffset = 4;
	}

	public static void Add(Gump g)
	{
		Stats.m_yOffset += g.Height + 2;
	}
}
