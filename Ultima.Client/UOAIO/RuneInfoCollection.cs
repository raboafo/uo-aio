using System;
using System.Collections;

namespace UOAIO;

public class RuneInfoCollection : CollectionBase
{
	public class RuneInfoCollectionEnumerator : IEnumerator
	{
		private int _index;

		private RuneInfo _currentElement;

		private RuneInfoCollection _collection;

		public RuneInfo Current
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

		internal RuneInfoCollectionEnumerator(RuneInfoCollection collection)
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

	public RuneInfo this[int index]
	{
		get
		{
			return (RuneInfo)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(RuneInfo value)
	{
		return base.List.Add(value);
	}

	public bool Contains(RuneInfo value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(RuneInfo value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(RuneInfo value)
	{
		base.List.Remove(value);
	}

	public new RuneInfoCollectionEnumerator GetEnumerator()
	{
		return new RuneInfoCollectionEnumerator(this);
	}

	public void Insert(int index, RuneInfo value)
	{
		base.List.Insert(index, value);
	}
}
