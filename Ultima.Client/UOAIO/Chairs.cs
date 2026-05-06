using System.Collections.Generic;

namespace UOAIO;

public static class Chairs
{
	public class ChairData
	{
		public readonly int ItemID;

		public readonly MobileDirection Facing;

		public readonly ChairType ChairType;

		public readonly int SittingPixelOffset;

		public static ChairData Null;

		public ChairData(int itemID, MobileDirection facing, ChairType chairType)
		{
			this.ItemID = itemID;
			this.Facing = facing;
			this.ChairType = chairType;
			this.SittingPixelOffset -= 32;
		}

		public MobileDirection GetSittingFacing(MobileDirection inFacing)
		{
			if (this.ChairType == ChairType.SingleFacing)
			{
				return this.Facing;
			}
			inFacing = DirectionHelper.GetCardinal(inFacing);
			if (inFacing == this.Facing)
			{
				return this.Facing;
			}
			if (this.ChairType == ChairType.ReversibleFacing)
			{
				if (DirectionHelper.Reverse(inFacing) == this.Facing)
				{
					return inFacing;
				}
			}
			else if (this.ChairType == ChairType.AnyFacing)
			{
				return inFacing;
			}
			return this.Facing;
		}

		static ChairData()
		{
			ChairData.Null = new ChairData(0, MobileDirection.ValueMask, ChairType.AnyFacing);
		}
	}

	public enum ChairType
	{
		SingleFacing,
		ReversibleFacing,
		AnyFacing
	}

	private static Dictionary<int, ChairData> m_Chairs;

	static Chairs()
	{
		Chairs.m_Chairs = new Dictionary<int, ChairData>();
		Chairs.AddChairData(1113, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(1114, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(1115, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(1116, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(2602, MobileDirection.South, ChairType.AnyFacing);
		Chairs.AddChairData(2603, MobileDirection.South, ChairType.AnyFacing);
		Chairs.AddChairData(2860, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(2861, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(2862, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(2863, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(2864, MobileDirection.West, ChairType.SingleFacing);
		Chairs.AddChairData(2865, MobileDirection.North, ChairType.SingleFacing);
		Chairs.AddChairData(2866, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(2867, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(2894, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(2895, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(2896, MobileDirection.North, ChairType.SingleFacing);
		Chairs.AddChairData(2897, MobileDirection.West, ChairType.SingleFacing);
		Chairs.AddChairData(2898, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(2899, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(2900, MobileDirection.North, ChairType.SingleFacing);
		Chairs.AddChairData(2901, MobileDirection.West, ChairType.SingleFacing);
		Chairs.AddChairData(2902, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(2903, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(2904, MobileDirection.West, ChairType.SingleFacing);
		Chairs.AddChairData(2905, MobileDirection.North, ChairType.SingleFacing);
		Chairs.AddChairData(2906, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(2907, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(2908, MobileDirection.North, ChairType.SingleFacing);
		Chairs.AddChairData(2909, MobileDirection.West, ChairType.SingleFacing);
		Chairs.AddChairData(2910, MobileDirection.South, ChairType.AnyFacing);
		Chairs.AddChairData(2911, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(2912, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(2913, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(2914, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(2915, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(2916, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(2917, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(2918, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(2919, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(2920, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(2921, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(2922, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(2961, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(2962, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(2963, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(2964, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(4169, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(4170, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(4604, MobileDirection.South, ChairType.AnyFacing);
		Chairs.AddChairData(4615, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(4616, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(4617, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(4618, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(4619, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(4620, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(4632, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(4633, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(4634, MobileDirection.North, ChairType.SingleFacing);
		Chairs.AddChairData(4635, MobileDirection.West, ChairType.SingleFacing);
		Chairs.AddChairData(7623, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(7624, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(7625, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(7626, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(7627, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(7628, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(7629, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(7630, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(7631, MobileDirection.East, ChairType.ReversibleFacing);
		Chairs.AddChairData(7632, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(7633, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(7634, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(11747, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(11748, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(11749, MobileDirection.West, ChairType.SingleFacing);
		Chairs.AddChairData(11750, MobileDirection.North, ChairType.SingleFacing);
		Chairs.AddChairData(11755, MobileDirection.North, ChairType.SingleFacing);
		Chairs.AddChairData(11756, MobileDirection.South, ChairType.SingleFacing);
		Chairs.AddChairData(11757, MobileDirection.East, ChairType.SingleFacing);
		Chairs.AddChairData(11758, MobileDirection.West, ChairType.SingleFacing);
		Chairs.AddChairData(15871, MobileDirection.South, ChairType.ReversibleFacing);
		Chairs.AddChairData(15872, MobileDirection.East, ChairType.ReversibleFacing);
	}

	public static void AddChairData(int itemID, MobileDirection direction, ChairType chairType)
	{
		if (Chairs.m_Chairs.ContainsKey(itemID))
		{
			Chairs.m_Chairs.Remove(itemID);
		}
		Chairs.m_Chairs.Add(itemID, new ChairData(itemID, direction, chairType));
	}

	public static bool CheckItemAsChair(int itemID, out ChairData value)
	{
		if (Chairs.m_Chairs.TryGetValue(itemID, out value))
		{
			return true;
		}
		value = ChairData.Null;
		return false;
	}
}
