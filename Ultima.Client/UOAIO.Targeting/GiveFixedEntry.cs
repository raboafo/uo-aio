namespace UOAIO.Targeting;

public sealed class GiveFixedEntry : GiveEntry
{
	private int m_Amount;

	public GiveFixedEntry(string name, int amount, params IItemValidator[] validators)
		: base(name, validators)
	{
		this.m_Amount = amount;
	}

	public override int GetAmount(int currentAmount)
	{
		return this.m_Amount;
	}
}
