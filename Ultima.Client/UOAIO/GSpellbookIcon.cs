using System.Windows.Forms;

namespace UOAIO;

public class GSpellbookIcon : GClickable
{
	private Item m_Container;

	private Gump m_Book;

	public GSpellbookIcon(Gump book, Item container)
		: base(container.BookIconX, container.BookIconY, Spells.GetBookIcon(container.ID))
	{
		this.m_Book = book;
		this.m_Container = container;
		this.m_Container.OpenSB = true;
		base.m_CanDrag = true;
		base.m_QuickDrag = false;
		base.m_GUID = $"Spellbook Icon #{container.Serial}";
	}

	protected override void OnDoubleClicked()
	{
		this.m_Container.BookIconX = base.m_X;
		this.m_Container.BookIconY = base.m_Y;
		int x = this.m_Book.X;
		int y = this.m_Book.Y;
		Gumps.Destroy(this);
		Gumps.Destroy(this.m_Book);
		Gump gump = Spells.OpenSpellbook(this.m_Container);
		gump.X = this.m_Book.X;
		gump.Y = this.m_Book.Y;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			this.m_Container.BookIconX = base.m_X;
			this.m_Container.BookIconY = base.m_Y;
			this.m_Container.OpenSB = false;
			Gumps.Destroy(this);
			Gumps.Destroy(this.m_Book);
			Engine.CancelClick();
		}
	}
}
