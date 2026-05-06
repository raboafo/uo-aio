using Ultima.Data;

namespace UOAIO;

public static class ShadowManager
{
	private enum ShadowType
	{
		Tree,
		Foliage,
		Vegetation,
		Rock
	}

	private static int[] _shadowBits;

	public static bool HasShadow(int itemID)
	{
		if (ShadowManager._shadowBits == null)
		{
			ShadowManager._shadowBits = ShadowManager.CreateShadowBits();
		}
		itemID &= 0x3FFF;
		return (ShadowManager._shadowBits[itemID >> 5] & (1 << itemID)) != 0;
	}

	private static int[] CreateShadowBits()
	{
		int[] array = new int[512];
		ShadowManager.SetShadowBit(array, 3220, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3221, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3222, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3225, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3226, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3227, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3228, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3229, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3230, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3231, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3232, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3233, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3234, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3235, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3236, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3237, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3238, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3240, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3241, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3242, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3243, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3272, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3273, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3274, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3275, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3276, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3277, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3278, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3279, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3280, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3281, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3282, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3283, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3284, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3285, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3286, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3287, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3288, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3289, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3290, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3291, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3292, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3293, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3294, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3295, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3296, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3297, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3298, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3299, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3300, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3301, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3302, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3303, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3304, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3305, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3306, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3320, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3321, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3322, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3323, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3324, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3325, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3326, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3327, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3328, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3329, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3330, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3331, ShadowType.Foliage);
		ShadowManager.SetShadowBit(array, 3365, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3366, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3367, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3381, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3383, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3384, ShadowType.Tree);
		ShadowManager.SetShadowBit(array, 3391, ShadowType.Vegetation);
		ShadowManager.SetShadowBit(array, 3392, ShadowType.Vegetation);
		for (int i = 3393; i <= 3499; i++)
		{
			ShadowManager.SetShadowBit(array, i, Map.m_ItemFlags[i][TileFlag.Foliage] ? ShadowType.Foliage : ShadowType.Tree);
		}
		for (int j = 4789; j <= 4807; j++)
		{
			ShadowManager.SetShadowBit(array, j, Map.m_ItemFlags[j][TileFlag.Foliage] ? ShadowType.Foliage : ShadowType.Tree);
		}
		ShadowManager.SetShadowBit(array, 4945, ShadowType.Rock);
		ShadowManager.SetShadowBit(array, 4948, ShadowType.Rock);
		ShadowManager.SetShadowBit(array, 4950, ShadowType.Rock);
		ShadowManager.SetShadowBit(array, 4953, ShadowType.Rock);
		ShadowManager.SetShadowBit(array, 4955, ShadowType.Rock);
		ShadowManager.SetShadowBit(array, 4958, ShadowType.Rock);
		ShadowManager.SetShadowBit(array, 4959, ShadowType.Rock);
		ShadowManager.SetShadowBit(array, 4960, ShadowType.Rock);
		ShadowManager.SetShadowBit(array, 4962, ShadowType.Rock);
		for (int k = 6001; k <= 6012; k++)
		{
			ShadowManager.SetShadowBit(array, k, ShadowType.Rock);
		}
		ShadowManager.SetShadowBit(array, 7038, ShadowType.Tree);
		for (int l = 9325; l <= 9342; l++)
		{
			ShadowManager.SetShadowBit(array, l, Map.m_ItemFlags[l][TileFlag.Foliage] ? ShadowType.Foliage : ShadowType.Tree);
		}
		for (int m = 9965; m <= 9971; m++)
		{
			ShadowManager.SetShadowBit(array, m, Map.m_ItemFlags[m][TileFlag.Foliage] ? ShadowType.Foliage : ShadowType.Tree);
		}
		return array;
	}

	private static void SetShadowBit(int[] bits, int itemId, ShadowType type)
	{
		bits[itemId >> 5] |= 1 << itemId;
	}
}
