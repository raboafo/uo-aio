using System.Collections.Generic;

namespace System;

public static class EnumerableEx
{
	public static void ForEach<T>(this IEnumerable<T> source, Action<T> thunk)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (thunk == null)
		{
			throw new ArgumentNullException("thunk");
		}
		foreach (T item in source)
		{
			thunk(item);
		}
	}

	public static IEnumerable<int> Interval(int min, int max)
	{
		int val = min;
		while (val <= max)
		{
			yield return val;
			int num = val + 1;
			val = num;
		}
	}
}
