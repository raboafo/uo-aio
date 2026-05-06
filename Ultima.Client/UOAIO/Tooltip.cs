namespace UOAIO;

public class Tooltip : ITooltip
{
	protected bool m_Big;

	protected string m_Text;

	protected Gump m_Gump;

	protected float m_Delay;

	private int m_WrapWidth;

	public float Delay
	{
		get
		{
			return this.m_Delay;
		}
		set
		{
			this.m_Delay = value;
		}
	}

	public bool Big
	{
		get
		{
			return this.m_Big;
		}
		set
		{
			if (this.m_Big != value)
			{
				this.m_Gump = null;
				this.m_Big = value;
			}
		}
	}

	public string Text
	{
		get
		{
			return this.m_Text;
		}
		set
		{
			if (this.m_Text != value)
			{
				this.m_Gump = null;
				this.m_Text = value;
			}
		}
	}

	public Tooltip(string Text)
		: this(Text, Big: false)
	{
	}

	public Tooltip(string Text, bool Big)
		: this(Text, Big, Big ? Engine.ScreenWidth : 100)
	{
	}

	public Tooltip(string Text, bool Big, int Width)
	{
		this.m_Text = Text;
		this.m_Big = Big;
		this.m_Gump = null;
		this.m_Delay = 1f;
		this.m_WrapWidth = Width;
	}

	public virtual Gump GetGump()
	{
		if (this.m_Gump != null)
		{
			return this.m_Gump;
		}
		if (this.m_Text == null || this.m_Text.Length <= 0)
		{
			return this.m_Gump = null;
		}
		this.m_Gump = new GAlphaBackground(0, 0, 100, 100);
		GWrappedLabel gWrappedLabel = new GWrappedLabel(this.m_Text, Engine.GetUniFont(1), Hues.Load(1153), 4, 4, this.m_WrapWidth);
		gWrappedLabel.X -= gWrappedLabel.Image.xMin;
		gWrappedLabel.Y -= gWrappedLabel.Image.yMin;
		this.m_Gump.Width = gWrappedLabel.Image.xMax - gWrappedLabel.Image.xMin + 9;
		this.m_Gump.Height = gWrappedLabel.Image.yMax - gWrappedLabel.Image.yMin + 9;
		this.m_Gump.Children.Add(gWrappedLabel);
		return this.m_Gump;
	}
}
