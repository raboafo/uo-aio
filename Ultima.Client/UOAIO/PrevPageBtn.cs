using System.Windows.Forms;

namespace UOAIO;

public class PrevPageBtn : GClickable
{
	private GBook m_Book;

	public PrevPageBtn(GBook book)
		: base(0, 0, 511)
	{
		this.m_Book = book;
	}

	protected internal override void OnSingleClick(int x, int y)
	{
		this.m_Book.PrevPage_OnClick();
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Right)
		{
			this.m_Book.ManualClose();
		}
	}
}
