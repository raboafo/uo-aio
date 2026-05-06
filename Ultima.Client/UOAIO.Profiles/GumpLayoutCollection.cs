using System;
using System.Collections;

namespace UOAIO.Profiles;

public class GumpLayoutCollection : CollectionBase
{
	public class GumpLayoutCollectionEnumerator : IEnumerator
	{
		private int _index;

		private GumpLayout _currentElement;

		private GumpLayoutCollection _collection;

		public GumpLayout Current
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

		internal GumpLayoutCollectionEnumerator(GumpLayoutCollection collection)
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

	public GumpLayout this[int index]
	{
		get
		{
			return (GumpLayout)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(GumpLayout value)
	{
		return base.List.Add(value);
	}

	public bool Contains(GumpLayout value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(GumpLayout value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(GumpLayout value)
	{
		base.List.Remove(value);
	}

	public new GumpLayoutCollectionEnumerator GetEnumerator()
	{
		return new GumpLayoutCollectionEnumerator(this);
	}

	public void Insert(int index, GumpLayout value)
	{
		base.List.Insert(index, value);
	}
}
