using System.Windows.Forms;

namespace UOAIO;

public class GServerButton : GButtonNew
{
	private int m_Type;

	private int m_Param;

	private int m_RelayID;

	private GServerGump m_Owner;

	public int RelayID => this.m_RelayID;

	public GServerButton(GServerGump owner, LayoutEntry le)
		: base(le[2], le[2], le[3], le[0], le[1])
	{
		this.m_Owner = owner;
		this.m_Type = le[4];
		this.m_Param = le[5];
		this.m_RelayID = le[6];
	}

	protected internal override bool HitTest(int x, int y)
	{
		return true;
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		this.m_Owner.BringToTop();
		base.OnMouseDown(x, y, mb);
	}

	protected override void OnClicked()
	{
		switch (this.m_Type)
		{
		case 0:
			this.m_Owner.Page = this.m_Param;
			break;
		case 1:
		case 4:
			if (this.m_RelayID != 0)
			{
				GServerGump.SetCachedLocation(this.m_Owner.DialogID, this.m_Owner.X, this.m_Owner.Y);
			}
			Network.Send(new PGumpButton(this.m_Owner, this.m_RelayID));
			Gumps.Destroy(base.m_Parent);
			break;
		case 2:
		case 3:
			break;
		}
	}
}
