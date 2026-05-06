namespace UOAIO;

public class ItemLoader : ILoader
{
	private int m_ItemID;

	public ItemLoader(int ItemID)
	{
		if (ItemID < 0)
		{
			this.m_ItemID = ItemID;
		}
		else
		{
			this.m_ItemID = 1;
		}
	}

	public void Load()
	{
		Hues.Default.GetItem(this.m_ItemID);
	}
}
