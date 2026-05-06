using System.Collections;

namespace UOAIO;

public class EquipSorter : IComparer
{
	public const int LAYER_ONEHANDED = 1;

	public const int LAYER_TWOHANDED = 2;

	public const int LAYER_SHOES = 3;

	public const int LAYER_PANTS = 4;

	public const int LAYER_SHIRT = 5;

	public const int LAYER_HELM = 6;

	public const int LAYER_GLOVES = 7;

	public const int LAYER_RING = 8;

	public const int LAYER_NECK = 10;

	public const int LAYER_HAIR = 11;

	public const int LAYER_WAIST = 12;

	public const int LAYER_TORSOINNER = 13;

	public const int LAYER_BRACELET = 14;

	public const int LAYER_FACIALHAIR = 16;

	public const int LAYER_TORSOMIDDLE = 17;

	public const int LAYER_EARRINGS = 18;

	public const int LAYER_ARMS = 19;

	public const int LAYER_BACK = 20;

	public const int LAYER_BACKPACK = 21;

	public const int LAYER_TORSOOUTER = 22;

	public const int LAYER_LEGSOUTER = 23;

	public const int LAYER_LEGSINNER = 24;

	public const int LAYER_MOUNT = 25;

	public const int LAYER_NPCBUYRES = 26;

	public const int LAYER_NPCBUYNORES = 27;

	public const int LAYER_NPCSELL = 28;

	public const int LAYER_PCBANK = 29;

	public int[] m_Table;

	public bool IsValidLayer(Layer layer)
	{
		return this.m_Table[(uint)layer] != -1;
	}

	public int Compare(object x, object y)
	{
		int num = this.m_Table[(uint)((Item)x).Layer];
		int num2 = this.m_Table[(uint)((Item)y).Layer];
		if (num < num2)
		{
			return -1;
		}
		if (num > num2)
		{
			return 1;
		}
		return 0;
	}
}
