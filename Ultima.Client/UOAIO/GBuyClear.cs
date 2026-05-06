namespace UOAIO;

public class GBuyClear : GRegion
{
	private GBuyGump m_Owner;

	public GBuyClear(GBuyGump owner)
		: base(169, 199, 55, 35)
	{
		this.m_Owner = owner;
	}

	protected internal override void OnSingleClick(int x, int y)
	{
		this.m_Owner.Clear();
	}
}
