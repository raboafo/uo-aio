namespace UOAIO;

public class GSellAccept : GRegion
{
	private GSellGump m_Owner;

	public GSellAccept(GSellGump owner)
		: base(30, 193, 63, 42)
	{
		this.m_Owner = owner;
	}

	protected internal override void OnSingleClick(int x, int y)
	{
		this.m_Owner.Accept();
	}
}
