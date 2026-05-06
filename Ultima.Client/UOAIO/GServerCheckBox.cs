using System.Windows.Forms;

namespace UOAIO;

public class GServerCheckBox : GCheckBox, IRelayedSwitch
{
	private GServerGump m_Owner;

	private int m_RelayID;

	int IRelayedSwitch.RelayID => this.m_RelayID;

	bool IRelayedSwitch.Active => base.Checked;

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		this.m_Owner.BringToTop();
		base.OnMouseDown(x, y, mb);
	}

	public GServerCheckBox(GServerGump owner, LayoutEntry le)
		: base(le[2], le[3], le[4] != 0, le[0], le[1])
	{
		this.m_Owner = owner;
		this.m_RelayID = le[5];
	}
}
