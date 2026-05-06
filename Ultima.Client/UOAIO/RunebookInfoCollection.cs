using System;
using System.Collections;

namespace UOAIO;

public class RunebookInfoCollection : CollectionBase
{
	public class RunebookInfoCollectionEnumerator : IEnumerator
	{
		private int _index;

		private RunebookInfo _currentElement;

		private RunebookInfoCollection _collection;

		public RunebookInfo Current
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

		internal RunebookInfoCollectionEnumerator(RunebookInfoCollection collection)
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

	public RunebookInfo this[int index]
	{
		get
		{
			return (RunebookInfo)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(RunebookInfo value)
	{
		return base.List.Add(value);
	}

	public bool Contains(RunebookInfo value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(RunebookInfo value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(RunebookInfo value)
	{
		base.List.Remove(value);
	}

	public new RunebookInfoCollectionEnumerator GetEnumerator()
	{
		return new RunebookInfoCollectionEnumerator(this);
	}

	public void Insert(int index, RunebookInfo value)
	{
		base.List.Insert(index, value);
	}
}
