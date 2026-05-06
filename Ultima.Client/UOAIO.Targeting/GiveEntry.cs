namespace UOAIO.Targeting;

public abstract class GiveEntry
{
	protected string m_Name;

	protected IItemValidator[] m_Validators;

	public string Name => this.m_Name;

	public IItemValidator[] Validators => this.m_Validators;

	public GiveEntry(string name, params IItemValidator[] validators)
	{
		this.m_Name = name;
		this.m_Validators = validators;
	}

	public abstract int GetAmount(int currentAmount);
}
