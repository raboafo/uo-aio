using System;
using System.Collections;

namespace UOAIO;

public class MacroSetCollection : CollectionBase
{
	public class MacroSetCollectionEnumerator : IEnumerator
	{
		private int _index;

		private MacroSet _currentElement;

		private MacroSetCollection _collection;

		public MacroSet Current
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

		internal MacroSetCollectionEnumerator(MacroSetCollection collection)
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

	public MacroSet this[int index]
	{
		get
		{
			return (MacroSet)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(MacroSet value)
	{
		return base.List.Add(value);
	}

	public bool Contains(MacroSet value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(MacroSet value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(MacroSet value)
	{
		base.List.Remove(value);
	}

	public new MacroSetCollectionEnumerator GetEnumerator()
	{
		return new MacroSetCollectionEnumerator(this);
	}

	public void Insert(int index, MacroSet value)
	{
		base.List.Insert(index, value);
	}
}
