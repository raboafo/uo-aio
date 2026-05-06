namespace UOAIO;

public class GPingDisplay : GLabel
{
	private IHue[] m_Hues;

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

	public GPingDisplay()
		: base("", Engine.DefaultFont, Engine.DefaultHue, 4, 4)
	{
		this.m_Hues = new IHue[7];
		this.m_Hues[0] = Hues.Load(68);
		this.m_Hues[1] = Hues.Load(63);
		this.m_Hues[2] = Hues.Load(58);
		this.m_Hues[3] = Hues.Load(53);
		this.m_Hues[4] = Hues.Load(48);
		this.m_Hues[5] = Hues.Load(43);
		this.m_Hues[6] = Hues.Load(38);
	}

	protected internal override void Render(int X, int Y)
	{
		if (Engine.m_Ingame && Renderer.DrawPing)
		{
			int ping = Engine.Ping;
			string arg = (ping / 5 * 5).ToString();
			int num = 0;
			num = (ping - 25) / 75;
			if (num < 0)
			{
				num = 0;
			}
			else if (num > 6)
			{
				num = 6;
			}
			if (ping < 5)
			{
				arg = "below 5";
			}
			else if (ping > 5000)
			{
				arg = "over 5000";
			}
			this.Hue = this.m_Hues[num];
			this.Text = $"Ping: {arg}";
			base.Render(X, Y);
			Stats.Add(this);
		}
	}
}
