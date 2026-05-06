namespace UOAIO;

public class GQuestArrow : GTracker
{
	private static bool m_Active;

	private static int m_ArrowX;

	private static int m_ArrowY;

	protected internal override void Render(int X, int Y)
	{
		if (GQuestArrow.m_Active)
		{
			base.Render(X, Y, GQuestArrow.m_ArrowX, GQuestArrow.m_ArrowY);
		}
	}

	protected override string GetPluralString(string direction, int distance)
	{
		return "Target: " + distance + " tiles " + direction;
	}

	protected override string GetSingularString(string direction)
	{
		return "Target: 1 tile " + direction;
	}

	public static void Activate(int x, int y)
	{
		GQuestArrow.m_Active = true;
		GQuestArrow.m_ArrowX = x;
		GQuestArrow.m_ArrowY = y;
	}

	public static void Stop()
	{
		GQuestArrow.m_Active = false;
	}
}
