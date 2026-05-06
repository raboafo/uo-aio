using System.Collections.Generic;
using System.IO;
using UOAIO.Profiles;

namespace UOAIO;

public class Multis
{
	private class MultiComparer : IComparer<Item>
	{
		public static readonly MultiComparer Instance;

		public int Compare(Item a, Item b)
		{
			if (a.Y > b.Y)
			{
				return 1;
			}
			if (a.Y < b.Y)
			{
				return -1;
			}
			if (a.X > b.X)
			{
				return 1;
			}
			if (a.X < b.X)
			{
				return -1;
			}
			return 0;
		}

		static MultiComparer()
		{
			MultiComparer.Instance = new MultiComparer();
		}
	}

	private MultiItem[][] m_Cache;

	private List<Item> m_Items;

	public List<Item> Items => this.m_Items;

	public bool RunUO_IsInside(Item item, Multi m, int px, int py, int pz)
	{
		m.GetBounds(out var xMin, out var yMin, out var xMax, out var yMax);
		int num = px - item.X;
		int num2 = py - item.Y;
		if (num >= xMin && num <= xMax && num2 >= yMin && num2 <= yMax)
		{
			if (item.Multi != m && num2 < yMax)
			{
				return true;
			}
			int num3 = num - xMin;
			int num4 = num2 - yMin;
			if (m.RunUO_Inside == null)
			{
				m.UpdateRadar();
			}
			int num5 = m.RunUO_Inside[num4][num3] + item.Z;
			if (pz == num5 || pz + 16 > num5)
			{
				return true;
			}
		}
		return false;
	}

	public bool RunUO_IsInside(int px, int py, int pz)
	{
		for (int i = 0; i < this.m_Items.Count; i++)
		{
			Item item = this.m_Items[i];
			if (item.InWorld && item.IsMulti)
			{
				CustomMultiEntry customMulti = CustomMultiLoader.GetCustomMulti(item.Serial, item.Revision);
				Multi multi = null;
				if (customMulti != null)
				{
					multi = customMulti.Multi;
				}
				if (multi == null)
				{
					multi = item.Multi;
				}
				if (multi != null && this.RunUO_IsInside(item, multi, px, py, pz))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Clear()
	{
		this.m_Items.Clear();
	}

	public Multis()
	{
		this.m_Items = new List<Item>();
		this.m_Cache = new MultiItem[8192][];
	}

	public void Sort()
	{
		this.m_Items.Sort(MultiComparer.Instance);
	}

	public MultiItem[] Load(int multiID)
	{
		multiID &= 0x1FFF;
		MultiItem[] array = this.m_Cache[multiID];
		if (array == null)
		{
			array = (this.m_Cache[multiID] = this.ReadFromDisk(multiID));
		}
		return array;
	}

	public void Dispose()
	{
		this.m_Cache = null;
		this.m_Items.Clear();
		this.m_Items = null;
	}

	public void Register(Item item)
	{
		if (!this.m_Items.Contains(item))
		{
			this.m_Items.Add(item);
			this.m_Items.Sort(MultiComparer.Instance);
		}
		Map.Invalidate();
		GRadar.Invalidate();
	}

	public void Unregister(Item item)
	{
		if (this.m_Items.Contains(item))
		{
			this.m_Items.Remove(item);
		}
		Map.Invalidate();
		GRadar.Invalidate();
	}

	public void Update(MapPackage map)
	{
		int count = this.m_Items.Count;
		if (count == 0)
		{
			return;
		}
		int length = map.cells.GetLength(0);
		int length2 = map.cells.GetLength(1);
		int cellX = map.CellX;
		int cellY = map.CellY;
		int num = cellX + length;
		int num2 = cellY + length2;
		int houseLevel = Options.Current.HouseLevel;
		for (int i = 0; i < count; i++)
		{
			Item item = this.m_Items[i];
			if (!item.InWorld)
			{
				continue;
			}
			CustomMultiEntry customMulti = CustomMultiLoader.GetCustomMulti(item.Serial, item.Revision);
			Multi multi = null;
			if (customMulti != null)
			{
				multi = customMulti.Multi;
			}
			if (multi == null)
			{
				multi = item.Multi;
			}
			if (multi == null)
			{
				continue;
			}
			multi.GetBounds(out var xMin, out var yMin, out var xMax, out var yMax);
			xMin += item.X;
			yMin += item.Y;
			xMax += item.X;
			yMax += item.Y;
			if (xMin >= num || xMax < cellX || yMin >= num2 || yMax < cellY)
			{
				continue;
			}
			List<MultiItem> list = multi.List;
			int count2 = list.Count;
			int num3 = int.MinValue | i;
			for (int j = 0; j < count2; j++)
			{
				MultiItem multiItem = list[j];
				if (multiItem.Flags == 0 && j != 0)
				{
					continue;
				}
				int num4 = item.X + multiItem.X;
				int num5 = item.Y + multiItem.Y;
				num4 -= cellX;
				num5 -= cellY;
				if (num4 >= 0 && num4 < length && num5 >= 0 && num5 < length2)
				{
					bool flag = true;
					int itemID = multiItem.ItemID;
					if (flag)
					{
						map.cells[num4, num5].Add(StaticItem.Instantiate((ushort)itemID, multiItem.ItemID, (sbyte)(item.Z + multiItem.Z), num3 | (j << 16)));
					}
				}
			}
		}
	}

	private unsafe MultiItem[] ReadFromDisk(int multiID)
	{
		BinaryReader binaryReader = new BinaryReader(Engine.FileManager.OpenMUL(Files.MultiIdx));
		binaryReader.BaseStream.Seek(multiID * 12, SeekOrigin.Begin);
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		binaryReader.Close();
		if (num == -1)
		{
			return new MultiItem[0];
		}
		Stream stream = Engine.FileManager.OpenMUL(Files.MultiMul);
		stream.Seek(num, SeekOrigin.Begin);
		byte[] array = new byte[num2];
		UnsafeMethods.ReadFile((FileStream)stream, array, 0, array.Length);
		stream.Close();
		int num3 = num2 / sizeof(MultiItem);
		MultiItem[] array2 = new MultiItem[num3];
		fixed (byte* ptr = array)
		{
			MultiItem* ptr2 = (MultiItem*)ptr;
			MultiItem* ptr3 = ptr2 + num3;
			fixed (MultiItem* ptr4 = array2)
			{
				MultiItem* ptr5 = ptr4;
				while (ptr2 < ptr3)
				{
					*(ptr5++) = *(ptr2++);
				}
			}
		}
		return array2;
	}
}
