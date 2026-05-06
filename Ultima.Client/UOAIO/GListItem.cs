using System.Windows.Forms;

namespace UOAIO;

public class GListItem : GTextButton
{
	private int m_Index;

	private GListBox m_Owner;

	public int Index => this.m_Index;

	public GListItem(string Text, int Index, GListBox Owner)
		: base(Text, Owner.Font, Owner.HRegular, Owner.HOver, Owner.OffsetX, Owner.OffsetY, null)
	{
		this.m_Index = Index;
		this.m_Owner = Owner;
		this.Layout();
	}

	public void Layout()
	{
		base.m_Visible = this.m_Index >= this.m_Owner.StartIndex && this.m_Index < this.m_Owner.StartIndex + this.m_Owner.ItemCount;
		base.m_Y = this.m_Owner.OffsetY + (this.m_Index - this.m_Owner.StartIndex) * 18;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		this.m_Owner.OnListItemClick(this);
	}
}
