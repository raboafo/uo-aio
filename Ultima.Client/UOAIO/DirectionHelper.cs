using System;
using SharpDX;

namespace UOAIO;

public static class DirectionHelper
{
	public static MobileDirection DirectionFromPoints(Point from, Point to)
	{
		return DirectionHelper.DirectionFromVectors(new Vector2(from.X, from.Y), new Vector2(to.X, to.Y));
	}

	public static MobileDirection DirectionFromVectors(Vector2 fromPosition, Vector2 toPosition)
	{
		double num = Math.Atan2(toPosition.Y - fromPosition.Y, toPosition.X - fromPosition.X);
		if (num < 0.0)
		{
			num = Math.PI + (Math.PI + num);
		}
		double num2 = Math.PI / 4.0;
		double num3 = Math.PI / 8.0;
		int num4 = int.MaxValue;
		for (int i = 0; i < 8; i++)
		{
			if (num >= num3 && num <= num3 + num2)
			{
				num4 = i + 1;
				break;
			}
			num3 += num2;
		}
		if (num4 == int.MaxValue)
		{
			num4 = 0;
		}
		num4 = ((num4 >= 7) ? (num4 - 7) : (num4 + 1));
		return (MobileDirection)num4;
	}

	public static MobileDirection GetCardinal(MobileDirection inDirection)
	{
		return inDirection & MobileDirection.West;
	}

	public static MobileDirection Reverse(MobileDirection inDirection)
	{
		return (inDirection + 4) & MobileDirection.Up;
	}
}
