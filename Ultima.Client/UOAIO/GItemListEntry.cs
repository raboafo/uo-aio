using System.Windows.Forms;

namespace UOAIO;

public class GItemListEntry : Gump
{
	private AnswerEntry m_Entry;

	private bool m_Draw;

	private IHue m_Hue;

	private Texture m_Image;

	private int m_Width;

	private int m_Height;

	private Clipper m_Clipper;

	private int m_xOffset;

	private int m_ImageOffsetX;

	private int m_ImageOffsetY;

	private GItemList m_Owner;

	public override int X => base.m_X + this.m_xOffset;

	public override int Width => this.m_Width;

	public override int Height => this.m_Height;

	public Clipper Clipper
	{
		get
		{
			return this.m_Clipper;
		}
		set
		{
			this.m_Clipper = value;
		}
	}

	public int xOffset
	{
		get
		{
			return this.m_xOffset;
		}
		set
		{
			this.m_xOffset = value;
		}
	}

	public GItemListEntry(int x, AnswerEntry entry, GItemList owner)
		: base(x, 45)
	{
		this.m_Entry = entry;
		this.m_Owner = owner;
		int num = entry.Hue;
		if (num > 0)
		{
			num++;
		}
		this.m_Hue = Hues.GetItemHue(entry.ItemID, num);
		this.m_Image = this.m_Hue.GetItem(entry.ItemID);
		if (this.m_Image != null && !this.m_Image.IsEmpty())
		{
			this.m_Draw = true;
			this.m_Height = 47;
			int num2 = this.m_Image.xMax - this.m_Image.xMin + 1;
			this.m_Width = 47;
			if (num2 > this.m_Width)
			{
				this.m_Width = num2;
			}
			this.m_ImageOffsetX = (this.m_Width - (this.m_Image.xMax - this.m_Image.xMin + 1)) / 2 - this.m_Image.xMin;
			this.m_ImageOffsetY = (this.m_Height - (this.m_Image.yMax - this.m_Image.yMin + 1)) / 2 - this.m_Image.yMin;
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		this.m_Owner.BringToTop();
	}

	protected internal override void OnDoubleClick(int x, int y)
	{
		Network.Send(new PQuestionMenuResponse(this.m_Owner.Serial, this.m_Owner.MenuID, this.m_Entry.Index, this.m_Entry.ItemID, this.m_Entry.Hue));
		Gumps.Destroy(this.m_Owner);
	}

	protected internal override void OnMouseEnter(int x, int y, MouseButtons mb)
	{
		this.m_Image = Hues.Load(32821).GetItem(this.m_Entry.ItemID);
		this.m_Draw = this.m_Image != null && !this.m_Image.IsEmpty();
		this.m_Owner.EntryLabel.Text = this.m_Entry.Text;
	}

	protected internal override void OnMouseLeave()
	{
		this.m_Image = this.m_Hue.GetItem(this.m_Entry.ItemID);
		this.m_Draw = this.m_Image != null && !this.m_Image.IsEmpty();
		this.m_Owner.EntryLabel.Text = "";
	}

	protected internal override void Draw(int x, int y)
	{
		if (this.m_Draw && this.m_Clipper != null)
		{
			this.m_Image.DrawClipped(x + this.m_ImageOffsetX, y + this.m_ImageOffsetY, this.m_Clipper);
		}
	}

	protected internal override bool HitTest(int x, int y)
	{
		return this.m_Draw && this.m_Clipper != null && this.m_Clipper.Evaluate(base.PointToScreen(new Point(x, y)));
	}
}
