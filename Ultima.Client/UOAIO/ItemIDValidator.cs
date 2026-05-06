namespace UOAIO;

public class ItemIDValidator : IItemValidator
{
	private int[] m_List;

	private IItemValidator m_Parent;

	public int[] List => this.m_List;

	public ItemIDValidator(params int[] list)
		: this(null, list)
	{
	}

	public ItemIDValidator(IItemValidator parent, params int[] list)
	{
		this.m_Parent = parent;
		this.m_List = list;
	}

	public bool IsValid(Item check)
	{
		if (this.m_Parent != null && !this.m_Parent.IsValid(check))
		{
			return false;
		}
		if (this.m_List == null || this.m_List.Length == 0)
		{
			return false;
		}
		int iD = check.ID;
		for (int i = 0; i < this.m_List.Length; i++)
		{
			if (this.m_List[i] == iD)
			{
				return true;
			}
		}
		return false;
	}
}
