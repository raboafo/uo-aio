using System;
using System.Collections;

namespace UOAIO;

public sealed class NameSorter : IComparer
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
		if (mobile == null || mobile2 == null)
		{
			throw new ArgumentException();
		}
		bool human = mobile.Human;
		bool human2 = mobile2.Human;
		if (human && !human2)
		{
			return -1;
		}
		if (human2 && !human)
		{
			return 1;
		}
		return mobile.Serial.CompareTo(mobile2.Serial);
	}

	static NameSorter()
	{
		NameSorter.Comparer = new NameSorter();
	}
}
