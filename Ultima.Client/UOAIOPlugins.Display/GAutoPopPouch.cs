using UOAIO;

namespace UOAIOPlugins.Display;

public class GAutoPopPouch : GLabel
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

	public GAutoPopPouch()
		: base("", Engine.DefaultFont, Engine.DefaultHue, 4, 4)
	{
		this.m_Hues = new IHue[2];
		this.m_Hues[0] = Hues.Load(38);
		this.m_Hues[1] = Hues.Load(1162);
	}

	protected internal override void Render(int X, int Y)
	{
		int num = 0;
		string text = "";
		num = ((Engine.m_Ingame && Options.AutoPopPouch) ? 1 : 0);
		if (num == 0)
		{
			text = "Off";
		}
		if (num == 1)
		{
			text = "On";
		}
		this.Hue = this.m_Hues[num];
		this.Text = string.Format("[Auto Pop Pouch: {0}", text + "]");
		base.Render(X, Y);
		Stats.Add(this);
	}
}
