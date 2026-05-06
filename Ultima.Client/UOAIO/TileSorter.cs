using System;
using System.Collections.Generic;
using Ultima.Data;

namespace UOAIO;

public class TileSorter : IComparer<ICell>
{
	private static Type tLandTile;

	private static Type tDynamicItem;

	private static Type tStaticItem;

	private static Type tMobileCell;

	public static readonly IComparer<ICell> Comparer;

	private TileSorter()
	{
	}

	public int Compare(ICell x, ICell y)
	{
		this.GetStats(x, out var z, out var treshold, out var type, out var tiebreaker);
		this.GetStats(y, out var z2, out var treshold2, out var type2, out var tiebreaker2);
		z += treshold;
		z2 += treshold2;
		int num = z - z2;
		if (num == 0)
		{
			num = type - type2;
		}
		if (num == 0)
		{
			num = treshold - treshold2;
		}
		if (num == 0)
		{
			num = tiebreaker - tiebreaker2;
		}
		return num;
	}

	public void GetStats(object obj, out int z, out int treshold, out int type, out int tiebreaker)
	{
		if (obj is MobileCell)
		{
			MobileCell mobileCell = (MobileCell)obj;
			z = mobileCell.Z;
			treshold = 2;
			type = 3;
			if (mobileCell.m_Mobile.Player)
			{
				tiebreaker = 1073741824;
			}
			else
			{
				tiebreaker = mobileCell.Serial;
			}
		}
		else if (obj is LandTile)
		{
			LandTile landTile = (LandTile)obj;
			z = landTile.SortZ;
			treshold = 0;
			type = 0;
			tiebreaker = 0;
		}
		else if (obj is DynamicItem)
		{
			DynamicItem dynamicItem = (DynamicItem)obj;
			z = dynamicItem.Z;
			int num = ((!Map.m_ItemFlags[dynamicItem.ID][TileFlag.Background]) ? 1 : 0);
			treshold = ((dynamicItem.Height == 0) ? num : (num + 1));
			type = ((dynamicItem.ID == 8198) ? 4 : 2);
			tiebreaker = dynamicItem.Serial;
		}
		else if (obj is StaticItem)
		{
			StaticItem staticItem = (StaticItem)obj;
			z = staticItem.Z;
			int num2 = ((!Map.m_ItemFlags[staticItem.ID][TileFlag.Background]) ? 1 : 0);
			treshold = ((staticItem.Height == 0) ? num2 : (num2 + 1));
			type = 1;
			tiebreaker = staticItem.m_SortInfluence;
		}
		else
		{
			z = 0;
			treshold = 0;
			type = 0;
			tiebreaker = 0;
		}
	}

	static TileSorter()
	{
		TileSorter.tLandTile = typeof(LandTile);
		TileSorter.tDynamicItem = typeof(DynamicItem);
		TileSorter.tStaticItem = typeof(StaticItem);
		TileSorter.tMobileCell = typeof(MobileCell);
		TileSorter.Comparer = new TileSorter();
	}
}
