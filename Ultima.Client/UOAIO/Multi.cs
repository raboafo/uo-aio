using System.Collections.Generic;
using Ultima.Data;

namespace UOAIO;

public class Multi
{
	private int m_MultiID;

	private int m_xMin;

	private int m_yMin;

	private int m_xMax;

	private int m_yMax;

	private List<MultiItem> m_List;

	private ushort[][] m_Radar;

	private sbyte[][] m_Inside;

	private sbyte[][] m_RunUO_Inside;

	public sbyte[][] Inside => this.m_Inside;

	public sbyte[][] RunUO_Inside => this.m_RunUO_Inside;

	public ushort[][] Radar => this.m_Radar;

	public List<MultiItem> List => this.m_List;

	public int MultiID => this.m_MultiID;

	public void UpdateRadar()
	{
		int num = this.m_xMax - this.m_xMin + 1;
		int num2 = this.m_yMax - this.m_yMin + 1;
		if (num <= 0 || num2 <= 0)
		{
			return;
		}
		int[][] array = new int[num2][];
		int[][] array2 = new int[num2][];
		this.m_Inside = new sbyte[num2][];
		this.m_RunUO_Inside = new sbyte[num2][];
		this.m_Radar = new ushort[num2][];
		for (int i = 0; i < num2; i++)
		{
			this.m_Radar[i] = new ushort[num];
			this.m_Inside[i] = new sbyte[num];
			this.m_RunUO_Inside[i] = new sbyte[num];
			array[i] = new int[num];
			array2[i] = new int[num];
			for (int j = 0; j < num; j++)
			{
				array[i][j] = int.MinValue;
			}
			for (int k = 0; k < num; k++)
			{
				array2[i][k] = int.MinValue;
			}
			for (int l = 0; l < num; l++)
			{
				this.m_Inside[i][l] = sbyte.MaxValue;
			}
			for (int m = 0; m < num; m++)
			{
				this.m_RunUO_Inside[i][m] = sbyte.MaxValue;
			}
		}
		for (int n = 0; n < this.m_List.Count; n++)
		{
			MultiItem multiItem = this.m_List[n];
			int num3 = multiItem.X - this.m_xMin;
			int num4 = multiItem.Y - this.m_yMin;
			if (num3 < 0 || num3 >= num || num4 < 0 || num4 >= num2)
			{
				continue;
			}
			int z = multiItem.Z;
			int num5 = z + Map.GetHeight(multiItem.ItemID);
			int num6 = array2[num4][num3];
			int num7 = array[num4][num3];
			int itemID = multiItem.ItemID;
			if ((num5 > num7 || (num5 == num7 && z > num6)) && itemID != 1 && itemID != 6038 && itemID != 8612 && itemID != 8600 && itemID != 8636 && itemID != 8601)
			{
				this.m_Radar[num4][num3] = multiItem.ItemID;
				array2[num4][num3] = z;
				array[num4][num3] = num5;
			}
			if (!Map.GetTileFlags(multiItem.ItemID)[TileFlag.Roof])
			{
				itemID = multiItem.ItemID;
				sbyte b = (sbyte)multiItem.Z;
				if (b < this.m_Inside[num4][num3])
				{
					this.m_Inside[num4][num3] = b;
				}
				if ((itemID < 2965 || itemID > 3086) && (itemID < 3139 || itemID > 3140) && b < this.m_RunUO_Inside[num4][num3])
				{
					this.m_RunUO_Inside[num4][num3] = b;
				}
			}
		}
	}

	public void GetBounds(out int xMin, out int yMin, out int xMax, out int yMax)
	{
		xMin = this.m_xMin;
		yMin = this.m_yMin;
		xMax = this.m_xMax;
		yMax = this.m_yMax;
	}

	public bool Remove(int x, int y, int z, int itemID)
	{
		if (x < this.m_xMin || y < this.m_yMin || x > this.m_xMax || y > this.m_yMax)
		{
			return false;
		}
		bool result = false;
		for (int i = 0; i < this.m_List.Count; i++)
		{
			MultiItem multiItem = this.m_List[i];
			if (multiItem.X == x && multiItem.Y == y && multiItem.Z == z && multiItem.ItemID == itemID)
			{
				this.m_List.RemoveAt(i--);
				result = true;
			}
		}
		return result;
	}

	public void Add(int itemID, int x, int y, int z)
	{
		if (x < this.m_xMin || y < this.m_yMin || x > this.m_xMax || y > this.m_yMax)
		{
			return;
		}
		for (int i = 0; i < this.m_List.Count; i++)
		{
			MultiItem multiItem = this.m_List[i];
			if (multiItem.X == x && multiItem.Y == y && multiItem.Z == z && Map.GetHeight(multiItem.ItemID) > 0 == Map.GetHeight(itemID) > 0)
			{
				this.m_List.RemoveAt(i--);
			}
		}
		MultiItem item = new MultiItem
		{
			Flags = 1,
			ItemID = (ushort)itemID,
			X = (short)x,
			Y = (short)y,
			Z = (short)z
		};
		this.m_List.Add(item);
	}

	public Multi(List<MultiItem> list)
	{
		this.m_MultiID = 0;
		int count = list.Count;
		int i = 0;
		this.m_xMin = 1000;
		this.m_yMin = 1000;
		this.m_xMax = -1000;
		this.m_yMax = -1000;
		for (; i < count; i++)
		{
			MultiItem multiItem = list[i];
			if (multiItem.X < this.m_xMin)
			{
				this.m_xMin = multiItem.X;
			}
			if (multiItem.Y < this.m_yMin)
			{
				this.m_yMin = multiItem.Y;
			}
			if (multiItem.X > this.m_xMax)
			{
				this.m_xMax = multiItem.X;
			}
			if (multiItem.Y > this.m_yMax)
			{
				this.m_yMax = multiItem.Y;
			}
		}
		this.m_List = list;
		this.UpdateRadar();
	}

	public Multi(int MultiID)
	{
		this.m_MultiID = MultiID;
		MultiItem[] array = Engine.Multis.Load(MultiID);
		int num = array.Length;
		int i = 0;
		this.m_xMin = 1000;
		this.m_yMin = 1000;
		this.m_xMax = -1000;
		this.m_yMax = -1000;
		for (; i < num; i++)
		{
			MultiItem multiItem = array[i];
			if (multiItem.X < this.m_xMin)
			{
				this.m_xMin = multiItem.X;
			}
			if (multiItem.Y < this.m_yMin)
			{
				this.m_yMin = multiItem.Y;
			}
			if (multiItem.X > this.m_xMax)
			{
				this.m_xMax = multiItem.X;
			}
			if (multiItem.Y > this.m_yMax)
			{
				this.m_yMax = multiItem.Y;
			}
		}
		this.m_List = new List<MultiItem>(array);
		this.UpdateRadar();
	}
}
