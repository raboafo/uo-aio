using System.Windows.Forms;

namespace UOAIO;

public class NextPageBtn : GClickable
{
	private GBook m_Book;

	public NextPageBtn(GBook book)
		: base(356, 0, 512)
	{
		this.m_Book = book;
	}

	protected internal override void OnSingleClick(int x, int y)
	{
		this.m_Book.NextPage_OnClick();
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Right)
		{
			this.m_Book.ManualClose();
		}
	}
}
