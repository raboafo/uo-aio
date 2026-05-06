using System;

namespace UOAIO;

public class GBandageTimer : GLabel
{
	private static bool m_Active;

	private static DateTime m_Start;

	public static bool Active => GBandageTimer.m_Active;

	public override int Y
	{
		get
		{
			return Stats.yOffset;
		}
		set
		{
		}
	}

	public static void Start()
	{
		GBandageTimer.m_Active = true;
		GBandageTimer.m_Start = DateTime.Now;
	}

	public static void Stop()
	{
		GBandageTimer.m_Active = false;
	}

	public GBandageTimer()
		: base("", Engine.DefaultFont, Engine.DefaultHue, 4, 4)
	{
	}

	protected internal override void Render(int X, int Y)
	{
		if (GBandageTimer.m_Active && Engine.m_Ingame)
		{
			TimeSpan timeSpan = DateTime.Now - GBandageTimer.m_Start;
			if (timeSpan >= TimeSpan.FromSeconds(20.0))
			{
				GBandageTimer.m_Active = false;
				return;
			}
			this.Text = $"Bandage: {(int)timeSpan.TotalSeconds} seconds elapsed";
			base.Render(X, Y);
			Stats.Add(this);
		}
	}
}
