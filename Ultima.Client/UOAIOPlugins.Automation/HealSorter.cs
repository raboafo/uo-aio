using System.Collections.Generic;
using UOAIO;

namespace UOAIOPlugins.Automation;

public class HealSorter : IComparer<Mobile>
{
	public static readonly IComparer<Mobile> Comparer;

	public int Compare(Mobile first, Mobile second)
	{
		if (first == null && second == null)
		{
			return 0;
		}
		if (first == null)
		{
			return -1;
		}
		if (second == null)
		{
			return 1;
		}
		if (first.CurrentHitPoints * 100 / first.MaximumHitPoints > second.CurrentHitPoints * 100 / second.MaximumHitPoints)
		{
			return 1;
		}
		if (first.CurrentHitPoints * 100 / first.MaximumHitPoints < second.CurrentHitPoints * 100 / second.MaximumHitPoints)
		{
			return -1;
		}
		return 0;
	}

	static HealSorter()
	{
		HealSorter.Comparer = new HealSorter();
	}
}
