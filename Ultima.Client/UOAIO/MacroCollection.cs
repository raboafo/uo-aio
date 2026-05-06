using System;
using System.Collections;

namespace UOAIO;

public class MacroCollection : CollectionBase
{
	public class MacroCollectionEnumerator : IEnumerator
	{
		private int _index;

		private Macro _currentElement;

		private MacroCollection _collection;

		public Macro Current
		{
			get
			{
				if (this._index == -1 || this._index >= this._collection.Count)
				{
					throw new IndexOutOfRangeException("Enumerator not started.");
				}
				return this._currentElement;
			}
		}

		object IEnumerator.Current
		{
			get
			{
				if (this._index == -1 || this._index >= this._collection.Count)
				{
					throw new IndexOutOfRangeException("Enumerator not started.");
				}
				return this._currentElement;
			}
		}

		internal MacroCollectionEnumerator(MacroCollection collection)
		{
			this._index = -1;
			this._collection = collection;
		}

		public void Reset()
		{
			this._index = -1;
			this._currentElement = null;
		}

		public bool MoveNext()
		{
			if (this._index < this._collection.Count - 1)
			{
				this._index++;
				this._currentElement = this._collection[this._index];
				return true;
			}
			this._index = this._collection.Count;
			return false;
		}
	}

	public Macro this[int index]
	{
		get
		{
			return (Macro)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(Macro value)
	{
		return base.List.Add(value);
	}

	public bool Contains(Macro value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(Macro value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(Macro value)
	{
		base.List.Remove(value);
	}

	public new MacroCollectionEnumerator GetEnumerator()
	{
		return new MacroCollectionEnumerator(this);
	}

	public void Insert(int index, Macro value)
	{
		base.List.Insert(index, value);
	}
}
