using System;
using System.Collections;

namespace UOAIO.Profiles;

public class ItemRefCollection : CollectionBase
{
	public class ItemRefCollectionEnumerator : IEnumerator
	{
		private int _index;

		private ItemRef _currentElement;

		private ItemRefCollection _collection;

		public ItemRef Current
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

		internal ItemRefCollectionEnumerator(ItemRefCollection collection)
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

	public ItemRef this[int index]
	{
		get
		{
			return (ItemRef)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(ItemRef value)
	{
		return base.List.Add(value);
	}

	public bool Contains(ItemRef value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(ItemRef value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(ItemRef value)
	{
		base.List.Remove(value);
	}

	public new ItemRefCollectionEnumerator GetEnumerator()
	{
		return new ItemRefCollectionEnumerator(this);
	}

	public void Insert(int index, ItemRef value)
	{
		base.List.Insert(index, value);
	}
}
