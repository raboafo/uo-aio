using System;

namespace UOAIO;

public abstract class GTracker : GLabel
{
	private static IHue[] m_Hues;

	private static string[] m_DirectionStrings;

	private int m_xLast;

	private int m_yLast;

	public override int Y => Stats.yOffset;

	static GTracker()
	{
		GTracker.m_DirectionStrings = new string[8] { "north-west", "north", "north-east", "east", "south-east", "south", "south-west", "west" };
		GTracker.m_Hues = new IHue[7];
		GTracker.m_Hues[0] = Hues.Load(68);
		GTracker.m_Hues[1] = Hues.Load(63);
		GTracker.m_Hues[2] = Hues.Load(58);
		GTracker.m_Hues[3] = Hues.Load(53);
		GTracker.m_Hues[4] = Hues.Load(48);
		GTracker.m_Hues[5] = Hues.Load(43);
		GTracker.m_Hues[6] = Hues.Load(38);
	}

	public GTracker()
		: base("", Engine.DefaultFont, Engine.DefaultHue, 4, 4)
	{
		this.m_xLast = (this.m_yLast = int.MinValue);
	}

	protected abstract string GetPluralString(string direction, int distance);

	protected abstract string GetSingularString(string direction);

	protected void Render(int X, int Y, int xTarget, int yTarget)
	{
		Mobile player = World.Player;
		if (player == null)
		{
			return;
		}
		if (this.m_xLast != xTarget || this.m_yLast != yTarget)
		{
			Direction direction = Engine.GetDirection(player.X, player.Y, xTarget, yTarget);
			string direction2 = GTracker.m_DirectionStrings[(uint)(Direction.West & direction)];
			int num = Math.Abs(xTarget - player.X);
			int num2 = Math.Abs(yTarget - player.Y);
			int num3 = ((num <= num2) ? (num + (num2 - num)) : (num2 + (num - num2)));
			int num4 = (num3 - 2) / 2;
			if (num4 < 0)
			{
				num4 = 0;
			}
			else if (num4 > 6)
			{
				num4 = 6;
			}
			string text = ((num3 == 1) ? this.GetSingularString(direction2) : this.GetPluralString(direction2, num3));
			this.Hue = GTracker.m_Hues[num4];
			this.Text = text;
		}
		base.Render(X, Y);
		Stats.Add(this);
	}
}
