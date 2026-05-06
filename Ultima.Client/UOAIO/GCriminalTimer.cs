using System;

namespace UOAIO;

public class GCriminalTimer : GLabel
{
	private static bool m_Active;

	private static DateTime m_Start;

	public static bool Active => GCriminalTimer.m_Active;

	public override int Y => Stats.yOffset;

	public static void Start()
	{
		GCriminalTimer.m_Active = true;
		GCriminalTimer.m_Start = DateTime.Now;
	}

	public static void Stop()
	{
		GCriminalTimer.m_Active = false;
	}

	public GCriminalTimer()
		: base("", Engine.DefaultFont, Hues.Load(58), 4, 4)
	{
	}

	protected internal override void Render(int X, int Y)
	{
		if (GCriminalTimer.m_Active && Engine.m_Ingame)
		{
			TimeSpan timeSpan = DateTime.Now - GCriminalTimer.m_Start;
			if (timeSpan >= TimeSpan.FromSeconds(300.0))
			{
				GCriminalTimer.m_Active = false;
				return;
			}
			this.Text = $"Criminal Timer: {(int)timeSpan.TotalSeconds} seconds elapsed";
			base.Render(X, Y);
			Stats.Add(this);
		}
	}
}
