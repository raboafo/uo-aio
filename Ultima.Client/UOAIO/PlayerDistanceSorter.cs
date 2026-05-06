using System;
using System.Collections;

namespace UOAIO;

public sealed class PlayerDistanceSorter : IComparer
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
		IPoint2D point2D = x as IPoint2D;
		IPoint2D point2D2 = y as IPoint2D;
		if (point2D == null || point2D2 == null)
		{
			throw new ArgumentException();
		}
		Mobile player = World.Player;
		return player.DistanceSqrt(point2D).CompareTo(player.DistanceSqrt(point2D2));
	}

	static PlayerDistanceSorter()
	{
		PlayerDistanceSorter.Comparer = new PlayerDistanceSorter();
	}
}
