using System.Collections.Generic;
using System.Reflection;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public sealed class GumpList
{
	private Gump m_Owner;

	private List<Gump> m_List;

	private Gump[] m_Array;

	private int m_Count;

	private static Gump[] m_Empty;

	public int Count => this.m_Count;

	public Gump this[int index] => this.m_List[index];

	public Gump[] ToArray()
	{
		if (this.m_Array == null)
		{
			if (this.m_Count == 0)
			{
				this.m_Array = GumpList.m_Empty;
			}
			else
			{
				this.m_Array = this.m_List.ToArray();
			}
		}
		return this.m_Array;
	}

	public void Set(GumpList g)
	{
		this.m_List = new List<Gump>(g.m_List);
		Gump[] array = this.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Parent = this.m_Owner;
		}
		this.m_Array = null;
		this.m_Count = this.m_List.Count;
		Gumps.Invalidate();
	}

	public void Clear()
	{
		while (this.m_List.Count > 0)
		{
			Gump gump = this.m_List[0];
			Gumps.Destroy(gump);
			this.m_List.Remove(gump);
		}
		this.m_Count = 0;
		this.m_Array = null;
	}

	public void Swap(int a, int b)
	{
		Gump value = this.m_List[a];
		this.m_List[a] = this.m_List[b];
		this.m_List[b] = value;
	}

	public int IndexOf(Gump Child)
	{
		return this.m_List.IndexOf(Child);
	}

	public GumpList(Gump Owner)
	{
		this.m_List = new List<Gump>(0);
		this.m_Owner = Owner;
		this.m_Array = GumpList.m_Empty;
	}

	public void Remove(Gump ToRemove)
	{
		this.m_Array = null;
		this.m_List.Remove(ToRemove);
		Gumps.Invalidate();
		this.m_Count = this.m_List.Count;
	}

	public void RemoveAt(int index)
	{
		this.m_Array = null;
		this.m_List.RemoveAt(index);
		Gumps.Invalidate();
		this.m_Count--;
	}

	public void Add(Gump ToAdd)
	{
		this.m_Array = null;
		Gumps.Invalidate();
		ToAdd.Parent = this.m_Owner;
		this.m_Count++;
		this.m_List.Add(ToAdd);
	}

	public void Add(GumpList list)
	{
		Gump[] array = list.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Parent = this.m_Owner;
			this.m_List.Add(array[i]);
		}
		this.m_Array = null;
		this.m_Count = this.m_List.Count;
		Gumps.Invalidate();
	}

	public void Insert(int Index, Gump ToAdd)
	{
		this.m_Array = null;
		Gumps.Invalidate();
		ToAdd.Parent = this.m_Owner;
		if (Index >= 0 && Index < this.m_List.Count)
		{
			this.m_List.Insert(Index, ToAdd);
		}
		else
		{
			this.m_List.Add(ToAdd);
		}
		this.m_Count++;
	}

	public IEnumerator<Gump> GetEnumerator()
	{
		return this.m_List.GetEnumerator();
	}

	static GumpList()
	{
		GumpList.m_Empty = new Gump[0];
	}
}
