namespace UOAIO;

public class GQuestionMenuEntry : Gump
{
	private int m_Width;

	private int m_Height;

	private int m_yBase;

	private GRadioButton m_Radio;

	private GLabel m_Label;

	private AnswerEntry m_Answer;

	public AnswerEntry Answer => this.m_Answer;

	public GRadioButton Radio => this.m_Radio;

	public Clipper Clipper
	{
		set
		{
			this.m_Radio.Clipper = value;
			this.m_Label.Clipper = value;
		}
	}

	public int yBase => this.m_yBase;

	public override int Width => this.m_Width;

	public override int Height => this.m_Height;

	public GQuestionMenuEntry(int x, int y, int xWidth, AnswerEntry a)
		: base(x, y)
	{
		this.m_yBase = y;
		this.m_Answer = a;
		this.m_Radio = new GRadioButton(210, 211, initialState: false, 0, 0, 0);
		this.m_Label = new GWrappedLabel(a.Text, Engine.GetFont(1), Hues.Load(1109), this.m_Radio.Width + 4, 5, xWidth - this.m_Radio.Width - 4);
		this.m_Width = xWidth;
		this.m_Height = this.m_Radio.Height;
		if (this.m_Label.Y + this.m_Label.Height > this.m_Height)
		{
			this.m_Height = this.m_Label.Y + this.m_Label.Height;
		}
		base.m_Children.Add(this.m_Radio);
		base.m_Children.Add(this.m_Label);
	}
}
