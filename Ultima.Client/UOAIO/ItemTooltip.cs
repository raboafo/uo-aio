namespace UOAIO;

public class ItemTooltip : ITooltip
{
	private Item m_Item;

	private Gump m_Gump;

	public Gump Gump
	{
		get
		{
			return this.m_Gump;
		}
		set
		{
			this.m_Gump = value;
		}
	}

	public float Delay
	{
		get
		{
			return 0.25f;
		}
		set
		{
		}
	}

	public ItemTooltip(Item item)
	{
		this.m_Item = item;
	}

	public Gump GetGump()
	{
		if (this.m_Gump != null)
		{
			return this.m_Gump;
		}
		if (this.m_Item.PropertyList == null)
		{
			this.m_Item.QueryProperties();
			return null;
		}
		return this.m_Gump = new GObjectProperties(1020000 + this.m_Item.ID, this.m_Item, this.m_Item.PropertyList);
	}
}
