using System;
using System.Collections;

namespace UOAIO;

public class ActionCollection : CollectionBase
{
	public class ActionCollectionEnumerator : IEnumerator
	{
		private int _index;

		private Action _currentElement;

		private ActionCollection _collection;

		public Action Current
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

		internal ActionCollectionEnumerator(ActionCollection collection)
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

	public Action this[int index]
	{
		get
		{
			return (Action)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(Action value)
	{
		return base.List.Add(value);
	}

	public bool Contains(Action value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(Action value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(Action value)
	{
		base.List.Remove(value);
	}

	public new ActionCollectionEnumerator GetEnumerator()
	{
		return new ActionCollectionEnumerator(this);
	}

	public void Insert(int index, Action value)
	{
		base.List.Insert(index, value);
	}
}
