namespace UOAIO;

public class GSellClear : GRegion
{
	private GSellGump m_Owner;

	public GSellClear(GSellGump owner)
		: base(169, 199, 55, 35)
	{
		this.m_Owner = owner;
	}

	protected internal override void OnSingleClick(int x, int y)
	{
		this.m_Owner.Clear();
	}
}
