using System;

namespace UOAIO.Targeting;

public sealed class GiveRatioEntry : GiveEntry
{
	private int m_Ratio;

	public GiveRatioEntry(string name, int ratio, params IItemValidator[] validators)
		: base(name, validators)
	{
		this.m_Ratio = ratio;
	}

	public override int GetAmount(int currentAmount)
	{
		return Math.Max(1, (currentAmount * this.m_Ratio + 50) / 100);
	}
}
