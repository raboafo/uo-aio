using System.Windows.Forms;

namespace UOAIO;

public class GItemList : GDragable
{
	private GHitspot m_Left;

	private GHitspot m_Right;

	private GLabel m_EntryLabel;

	private double m_xOffsetCap;

	private double m_xOffset;

	private int m_Serial;

	private int m_MenuID;

	private int m_xLast = -1234;

	private int m_yLast = -1234;

	public int Serial => this.m_Serial;

	public int MenuID => this.m_MenuID;

	public GLabel EntryLabel => this.m_EntryLabel;

	public double xOffset
	{
		get
		{
			return this.m_xOffset;
		}
		set
		{
			if (value > 0.0)
			{
				value = 0.0;
			}
			else if (value < this.m_xOffsetCap)
			{
				value = this.m_xOffsetCap;
			}
			this.m_xOffset = value;
			int num = (int)value;
			for (int i = 0; i < base.m_Children.Count; i++)
			{
				if (base.m_Children[i] is GItemListEntry)
				{
					((GItemListEntry)base.m_Children[i]).xOffset = num;
				}
			}
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Network.Send(new PQuestionMenuCancel(this.m_Serial, this.m_MenuID));
			Gumps.Destroy(this);
		}
	}

	public GItemList(int serial, int menuID, string question, AnswerEntry[] answers)
		: base(2320, 25, 25)
	{
		this.m_Serial = serial;
		this.m_MenuID = menuID;
		this.m_EntryLabel = new GLabel("", Engine.GetFont(1), Hues.Load(1887), 39, 106);
		base.m_Children.Add(this.m_EntryLabel);
		GLabel gLabel = new GLabel(question, Engine.GetFont(1), Hues.Load(1887), 39, 19);
		gLabel.Scissor(0, 0, 218, 11);
		base.m_Children.Add(gLabel);
		int num = 37;
		for (int i = 0; i < answers.Length; i++)
		{
			GItemListEntry gItemListEntry = new GItemListEntry(num, answers[i], this);
			base.m_Children.Add(gItemListEntry);
			num += gItemListEntry.Width;
		}
		num -= 37;
		if (num >= 222)
		{
			this.m_xOffsetCap = -(num - 222);
			this.m_Left = new GItemListScroller(23, this, 150);
			this.m_Right = new GItemListScroller(261, this, -150);
			base.m_Children.Add(this.m_Left);
			base.m_Children.Add(this.m_Right);
		}
	}

	protected internal override void Draw(int x, int y)
	{
		base.Draw(x, y);
		if (this.m_xLast == x && this.m_yLast == y)
		{
			return;
		}
		this.m_xLast = x;
		this.m_yLast = y;
		Clipper clipper = new Clipper(x + 37, y + 45, 222, 47);
		for (int i = 0; i < base.m_Children.Count; i++)
		{
			if (base.m_Children[i] is GItemListEntry)
			{
				((GItemListEntry)base.m_Children[i]).Clipper = clipper;
			}
		}
		this.m_EntryLabel.Scissor(new Clipper(x + 39, y + 106, 218, 11));
	}
}
