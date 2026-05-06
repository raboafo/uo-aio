namespace UOAIO;

public class GBuyAccept : GRegion
{
	private GBuyGump m_Owner;

	public GBuyAccept(GBuyGump owner)
		: base(30, 193, 63, 42)
	{
		this.m_Owner = owner;
	}

	protected internal override void OnSingleClick(int x, int y)
	{
		this.m_Owner.Accept();
	}
}
