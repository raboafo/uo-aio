using System.Collections.Generic;

namespace UOAIO;

public sealed class TargetSorter : IComparer<Mobile>
{
	public static readonly IComparer<Mobile> Comparer;

	public int Compare(Mobile x, Mobile y)
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
		int num = -x.Human.CompareTo(y.Human);
		if (num == 0 && num == 0)
		{
			Mobile player = World.Player;
			if (player != null)
			{
				num = x.DistanceSqrt(player).CompareTo(y.DistanceSqrt(player));
			}
		}
		return num;
	}

	static TargetSorter()
	{
		TargetSorter.Comparer = new TargetSorter();
	}
}
