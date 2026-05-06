using System.Windows.Forms;

namespace UOAIO;

public class ItemButtonGump : GItemArt
{
	public ItemButtonGump(int x, int y, int itemID)
		: base(x, y, itemID)
	{
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return base.m_Draw && base.m_Image.HitTest(X, Y);
	}

	protected internal override void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
		base.Hue = Hues.Load(32821);
	}

	protected internal override void OnMouseLeave()
	{
		base.Hue = Hues.Default;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Left)
		{
			this.OnClick();
		}
	}

	public virtual void OnClick()
	{
	}
}
