using System.Collections.Generic;
using System.Threading;

namespace UOAIO;

public static class Compression
{
	private static short[] _sourceTable;

	public static UnpackLeaf m_Tree;

	public static UnpackCacheEntry[] m_CacheEntries;

	public static UnpackLeaf[] m_Leaves;

	public static byte[] m_OutputBuffer;

	public static int m_OutputIndex;

	private static bool m_UnpackCacheLoaded;

	unsafe static Compression()
	{
		Compression._sourceTable = new short[257]
		{
			2, 501, 278, 359, 1399, 86, 886, 615, 120, 1128,
			855, 2536, 5945, 3736, 342, 1879, 2280, 3496, 1657, 3672,
			1319, 1943, 1640, 3048, 4409, 3705, 2023, 297, 2472, 3560,
			2200, 1592, 726, 3129, 3944, 6441, 3306, 1448, 7530, 234,
			601, 2649, 2089, 15978, 4569, 153, 7401, 13290, 6745, 25,
			313, 806, 4313, 5273, 6553, 5433, 6617, 2681, 3113, 4857,
			7017, 1785, 6697, 5161, 165, 1688, 920, 6265, 1849, 7721,
			7737, 4393, 7209, 3177, 7641, 4649, 3433, 1577, 3833, 6905,
			1065, 1241, 5593, 5753, 1960, 553, 6809, 2521, 2585, 5673,
			2969, 1817, 8682, 490, 7225, 3818, 16106, 375, 7993, 2840,
			632, 1271, 2793, 4329, 40, 1047, 6681, 425, 1384, 2104,
			1143, 247, 2264, 1625, 1559, 1768, 823, 1431, 2601, 982,
			2360, 3689, 4697, 217, 5994, 7786, 15994, 5353, 1337, 1081,
			3225, 103, 872, 4249, 761, 8426, 11818, 12714, 10362, 14074,
			12202, 6186, 14970, 7322, 935, 408, 3898, 7482, 9706, 8105,
			16122, 12010, 11578, 11738, 13370, 14378, 1498, 7930, 10538, 1514,
			4841, 11498, 11114, 5337, 4890, 8666, 15083, 12779, 9195, 10266,
			5722, 1178, 11482, 14106, 20971, 8858, 9690, 4762, 2809, 747,
			17643, 3866, 9451, 21995, 13082, 3290, 1259, 14186, 17131, 4587,
			8730, 12314, 2713, 8986, 1003, 13914, 30187, 4122, 23275, 12826,
			12954, 3386, 12058, 16154, 15674, 3626, 2170, 3642, 4010, 5178,
			474, 7066, 666, 25579, 6778, 15466, 2074, 6170, 794, 10650,
			4634, 7962, 6891, 25835, 17387, 7914, 13803, 11834, 21483, 5099,
			14362, 29163, 15258, 5611, 56, 4522, 5882, 2346, 2922, 2458,
			12090, 8938, 15722, 7802, 15514, 26475, 9370, 10091, 7385, 538,
			5914, 1898, 31467, 7274, 3546, 567, 180
		};
		Compression.m_Leaves = new UnpackLeaf[513];
		int index = 0;
		Compression.m_Tree = new UnpackLeaf(index);
		Compression.m_Leaves[index++] = Compression.m_Tree;
		fixed (short* sourceTable = Compression._sourceTable)
		{
			short* ptr = sourceTable;
			for (short num = 0; num <= 256; num++)
			{
				UnpackLeaf unpackLeaf = Compression.m_Tree;
				int num2 = *(ptr++);
				int num3 = num2 & 0xF;
				int num4 = num2 >> 4;
				while (--num3 >= 0)
				{
					switch (num4 & 1)
					{
					case 0:
						if (unpackLeaf.m_Left == null)
						{
							unpackLeaf.m_Left = new UnpackLeaf(index);
							Compression.m_Leaves[index++] = unpackLeaf.m_Left;
						}
						unpackLeaf = unpackLeaf.m_Left;
						break;
					case 1:
						if (unpackLeaf.m_Right == null)
						{
							unpackLeaf.m_Right = new UnpackLeaf(index);
							Compression.m_Leaves[index++] = unpackLeaf.m_Right;
						}
						unpackLeaf = unpackLeaf.m_Right;
						break;
					}
					num4 >>= 1;
				}
				unpackLeaf.m_Value = num;
			}
		}
		Compression._sourceTable = null;
	}

	private static void Thread_LoadUnpackCache(object state)
	{
		ManualResetEvent manualResetEvent = (ManualResetEvent)state;
		Compression.LoadUnpackCache();
		manualResetEvent.Set();
	}

	public unsafe static void LoadUnpackCache()
	{
		Compression.m_OutputBuffer = new byte[100725];
		Compression.m_CacheEntries = new UnpackCacheEntry[65536];
		fixed (UnpackCacheEntry* cacheEntries = Compression.m_CacheEntries)
		{
			UnpackCacheEntry* ptr = cacheEntries;
			fixed (byte* outputBuffer = Compression.m_OutputBuffer)
			{
				Stack<UnpackLeaf> stack = new Stack<UnpackLeaf>();
				stack.Push(Compression.m_Tree);
				while (stack.Count > 0)
				{
					UnpackLeaf unpackLeaf = stack.Pop();
					if (unpackLeaf.m_Left != null)
					{
						stack.Push(unpackLeaf.m_Left);
					}
					if (unpackLeaf.m_Right != null)
					{
						stack.Push(unpackLeaf.m_Right);
					}
					int[] array = new int[256];
					for (int i = 0; i < 256; i++)
					{
						byte* ptr2 = outputBuffer + Compression.m_OutputIndex;
						UnpackLeaf unpackLeaf2 = unpackLeaf;
						int num = 0;
						int num2 = 8;
						while (true)
						{
							if (--num2 >= 0)
							{
								switch ((i >> num2) & 1)
								{
								case 0:
									unpackLeaf2 = unpackLeaf2.m_Left;
									break;
								case 1:
									unpackLeaf2 = unpackLeaf2.m_Right;
									break;
								}
								if (unpackLeaf2 == null)
								{
									break;
								}
								if (unpackLeaf2.m_Value != -1)
								{
									switch (unpackLeaf2.m_Value >> 8)
									{
									case 0:
										ptr2[num++] = (byte)unpackLeaf2.m_Value;
										break;
									case 1:
										num2 = 0;
										break;
									}
									unpackLeaf2 = Compression.m_Tree;
								}
								continue;
							}
							ptr->m_NextIndex = unpackLeaf2.m_Index;
							ptr->m_ByteIndex = Compression.m_OutputIndex;
							ptr->m_ByteCount = num;
							array[i] = (int)(ptr - cacheEntries);
							ptr++;
							Compression.m_OutputIndex += num;
							break;
						}
					}
					unpackLeaf.m_Cache = array;
				}
			}
		}
	}

	public static void CheckCache()
	{
		if (!Compression.m_UnpackCacheLoaded)
		{
			ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			ThreadPool.QueueUserWorkItem(Thread_LoadUnpackCache, manualResetEvent);
			do
			{
				Engine.DrawNow();
			}
			while (!manualResetEvent.WaitOne(10, exitContext: false));
			manualResetEvent.Close();
			Compression.m_UnpackCacheLoaded = true;
		}
	}
}
