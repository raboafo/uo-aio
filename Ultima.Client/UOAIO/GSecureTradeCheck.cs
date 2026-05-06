namespace UOAIO;

public class GSecureTradeCheck : GButtonNew
{
	private Item m_Item;

	private bool m_Checked;

	private GSecureTradeCheck m_Partner;

	public bool Checked
	{
		get
		{
			return this.m_Checked;
		}
		set
		{
			if (this.m_Checked != value)
			{
				this.m_Checked = value;
				if (this.m_Checked)
				{
					base.m_GumpID[0] = 2153;
					base.m_GumpID[1] = 2154;
					base.m_GumpID[2] = 2154;
				}
				else
				{
					base.m_GumpID[0] = 2151;
					base.m_GumpID[1] = 2152;
					base.m_GumpID[2] = 2152;
				}
				base.Invalidate();
			}
		}
	}

	public GSecureTradeCheck(int x, int y, Item item, GSecureTradeCheck partner)
		: base(2151, 2152, 2152, x, y)
	{
		this.m_Item = item;
		this.m_Partner = partner;
		base.Enabled = this.m_Item != null;
	}

	protected override void OnClicked()
	{
		if (this.m_Item != null)
		{
			Network.Send(new PCheckTrade(this.m_Item, !this.m_Checked, this.m_Partner.m_Checked));
		}
	}
}
