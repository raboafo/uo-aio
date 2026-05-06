namespace UOAIO;

public struct Reagent
{
	private string m_Name;

	private int m_ItemID;

	public string Name => this.m_Name;

	public int ItemID => this.m_ItemID;

	public Reagent(string Name)
	{
		this.m_Name = Name;
		this.m_ItemID = Spells.GetReagent(Name).ItemID;
	}

	public Reagent(string Name, int ItemID)
	{
		this.m_Name = Name;
		this.m_ItemID = ItemID;
	}
}
