using System;
using System.Collections;

namespace UOAIO;

public sealed class ItemAmountSorter : IComparer
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
		Item item = x as Item;
		Item item2 = y as Item;
		if (item == null || item2 == null)
		{
			throw new ArgumentException();
		}
		return ((ushort)item.Amount).CompareTo((ushort)item2.Amount);
	}

	static ItemAmountSorter()
	{
		ItemAmountSorter.Comparer = new ItemAmountSorter();
	}
}
