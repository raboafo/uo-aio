namespace UOAIO;

public class GQuestionBackground : GBackground
{
	private int m_xLast = int.MinValue;

	private int m_yLast = int.MinValue;

	private GQuestionMenuEntry[] m_Entries;

	protected internal override void Draw(int x, int y)
	{
		base.Draw(x, y);
		if (this.m_xLast != x || this.m_yLast != y)
		{
			this.m_xLast = x;
			this.m_yLast = y;
			Clipper clipper = new Clipper(x + base.OffsetX, y + base.OffsetY, base.UseWidth, base.UseHeight);
			for (int i = 0; i < this.m_Entries.Length; i++)
			{
				this.m_Entries[i].Clipper = clipper;
			}
		}
	}

	protected internal override void OnMouseWheel(int delta)
	{
		base.m_Parent.OnMouseWheel(delta);
	}

	public GQuestionBackground(GQuestionMenuEntry[] entries, int width, int height, int x, int y)
		: base(3004, width, height, x, y, HasBorder: true)
	{
		this.m_Entries = entries;
	}
}
