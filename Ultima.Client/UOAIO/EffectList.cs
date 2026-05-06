using System.Collections;
using System.Collections.Generic;

namespace UOAIO;

public class EffectList : IEnumerable<Effect>, IEnumerable
{
	private List<Effect> m_List;

	public int Count => this.m_List.Count;

	public Effect this[int Index] => this.m_List[Index];

	public void Clear()
	{
		this.m_List.Clear();
	}

	public int IndexOf(Effect Child)
	{
		return this.m_List.IndexOf(Child);
	}

	public EffectList()
	{
		this.m_List = new List<Effect>(0);
	}

	public void Remove(Effect ToRemove)
	{
		this.m_List.Remove(ToRemove);
	}

	public void RemoveAt(int Index)
	{
		this.m_List.RemoveAt(Index);
	}

	public void Add(Effect ToAdd)
	{
		this.m_List.Add(ToAdd);
	}

	public void Insert(int Index, Effect ToAdd)
	{
		if (Index >= 0 && Index < this.m_List.Count)
		{
			this.m_List.Insert(Index, ToAdd);
		}
		else
		{
			this.m_List.Add(ToAdd);
		}
	}

	public IEnumerator<Effect> GetEnumerator()
	{
		return this.m_List.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}
}
