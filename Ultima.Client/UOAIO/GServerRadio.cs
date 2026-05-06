using System.Windows.Forms;

namespace UOAIO;

public class GServerRadio : GRadioButton, IRelayedSwitch
{
	private GServerGump m_Owner;

	private int m_RelayID;

	int IRelayedSwitch.RelayID => this.m_RelayID;

	bool IRelayedSwitch.Active => base.State;

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		this.m_Owner.BringToTop();
		base.OnMouseDown(x, y, mb);
	}

	public GServerRadio(GServerGump owner, LayoutEntry le)
		: this(owner, le, 0)
	{
	}

	public GServerRadio(GServerGump owner, LayoutEntry le, int group)
		: base(le[2], le[3], le[4] != 0, le[0], le[1], group)
	{
		this.m_Owner = owner;
		this.m_RelayID = le[5];
	}
}
