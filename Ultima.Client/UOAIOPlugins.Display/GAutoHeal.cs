using UOAIO;
using UOAIOPlugins.Automation;

namespace UOAIOPlugins.Display;

public class GAutoHeal : GLabel
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

	public GAutoHeal()
		: base("", Engine.DefaultFont, Engine.DefaultHue, 4, 4)
	{
		this.m_Hues = new IHue[2];
		this.m_Hues[0] = Hues.Load(38);
		this.m_Hues[1] = Hues.Load(1162);
	}

	protected internal override void Render(int X, int Y)
	{
		string text = "";
		int num = ((Engine.m_Ingame && Options.AutoHeal) ? 1 : 0);
		if (num == 0)
		{
			text = "Off";
		}
		if (num == 1)
		{
			text = "On";
		}
		this.Hue = this.m_Hues[num];
		string format = "[Auto Heal: {0}";
		string text2 = text;
		this.Text = string.Format(format, text2 + " Heal Value: " + Defender.HealValue + " Range: " + Defender.HealRange + "]");
		base.Render(X, Y);
		Stats.Add(this);
	}
}
