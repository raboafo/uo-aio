using System.Collections;

namespace UOAIO;

public sealed class HealSorter : IComparer
{
	public static readonly IComparer Comparer;

	public int Compare(object x, object y)
	{
		if (x == null && y == null)
		{
			return 0;
		}
		if (x == null)
		{
			return -1;
		}
		if (y == null)
		{
			return 1;
		}
		Mobile mobile = x as Mobile;
		Mobile mobile2 = y as Mobile;
		int num = ((mobile.MaximumHitPoints <= 0) ? 100 : (mobile.CurrentHitPoints * 100 / mobile.MaximumHitPoints));
		int num2 = ((mobile2.MaximumHitPoints <= 0) ? 100 : (mobile2.CurrentHitPoints * 100 / mobile2.MaximumHitPoints));
		return num - num2;
	}

	static HealSorter()
	{
		HealSorter.Comparer = new HealSorter();
	}
}
