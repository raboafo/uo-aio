using System.Collections.Generic;
using System.Linq;

namespace UOAIO;

public class LayerComparer : IComparer<Item>
{
	private IDictionary<int, int> map;

	private static readonly int[][] maleDrawOrders;

	private static readonly int[][] dollDrawOrders;

	private static readonly LayerComparer[] maleComparers;

	public static readonly LayerComparer Paperdoll;

	public LayerComparer(int[] seq)
	{
		this.map = seq.Select((int n, int i) => new
		{
			Index = i,
			Layer = n
		}).ToDictionary(x => x.Layer, x => x.Index);
	}

	public static LayerComparer FromDirection(int dir)
	{
		return LayerComparer.FromDirection((MobileDirection)dir);
	}

	public static LayerComparer FromDirection(MobileDirection dir)
	{
		return LayerComparer.maleComparers[(uint)(dir & MobileDirection.Up)];
	}

	private int Sequence(Item item)
	{
		if (item.Layer != Layer.Bank)
		{
			if (!this.map.TryGetValue((int)item.Layer, out var value))
			{
				return 30;
			}
			return value;
		}
		return 0;
	}

	public int Compare(Item a, Item b)
	{
		return this.Sequence(a).CompareTo(this.Sequence(b));
	}

	static LayerComparer()
	{
		LayerComparer.maleDrawOrders = new int[8][]
		{
			new int[26]
			{
				29, 25, 5, 4, 3, 24, 13, 8, 9, 14,
				15, 19, 7, 23, 17, 22, 12, 10, 11, 16,
				18, 6, 1, 2, 21, 20
			},
			new int[26]
			{
				29, 25, 5, 4, 3, 24, 13, 8, 9, 14,
				15, 19, 7, 23, 17, 22, 12, 10, 11, 16,
				18, 6, 1, 21, 2, 20
			},
			new int[26]
			{
				29, 25, 5, 4, 3, 24, 13, 8, 9, 14,
				15, 19, 7, 23, 17, 22, 12, 10, 11, 16,
				18, 6, 1, 21, 20, 2
			},
			new int[26]
			{
				29, 25, 20, 5, 4, 3, 24, 13, 8, 9,
				14, 15, 19, 7, 23, 17, 22, 12, 10, 11,
				16, 18, 6, 1, 2, 21
			},
			new int[26]
			{
				29, 25, 5, 4, 3, 24, 13, 8, 9, 14,
				15, 19, 7, 23, 17, 22, 12, 10, 11, 16,
				18, 6, 1, 21, 20, 2
			},
			new int[26]
			{
				29, 25, 5, 4, 3, 24, 13, 8, 9, 14,
				15, 19, 7, 23, 17, 22, 12, 10, 11, 16,
				18, 6, 1, 21, 2, 20
			},
			new int[26]
			{
				29, 25, 5, 4, 3, 24, 13, 8, 9, 14,
				15, 19, 7, 23, 17, 22, 12, 10, 11, 16,
				18, 6, 1, 2, 21, 20
			},
			new int[26]
			{
				29, 25, 5, 4, 3, 24, 13, 8, 9, 14,
				15, 19, 7, 23, 17, 22, 12, 10, 11, 16,
				18, 6, 1, 2, 21, 20
			}
		};
		LayerComparer.dollDrawOrders = new int[3][]
		{
			new int[26]
			{
				29, 20, 5, 4, 3, 24, 13, 17, 8, 14,
				15, 19, 7, 23, 22, 12, 10, 11, 16, 18,
				6, 1, 2, 21, 9, 25
			},
			new int[26]
			{
				29, 20, 5, 4, 3, 24, 19, 13, 17, 8,
				14, 15, 7, 23, 22, 12, 10, 11, 16, 18,
				6, 1, 2, 21, 9, 25
			},
			new int[26]
			{
				29, 20, 13, 5, 4, 3, 24, 17, 8, 14,
				15, 19, 7, 23, 22, 12, 10, 11, 16, 18,
				6, 1, 2, 21, 9, 25
			}
		};
		LayerComparer.maleComparers = LayerComparer.maleDrawOrders.Select((int[] x) => new LayerComparer(x)).ToArray();
		LayerComparer.Paperdoll = new LayerComparer(LayerComparer.dollDrawOrders[1]);
	}
}
