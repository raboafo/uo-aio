using System;
using System.Collections;

namespace UOAIO.Profiles;

public class ServerCollection : CollectionBase
{
	public class ServerCollectionEnumerator : IEnumerator
	{
		private int _index;

		private Server _currentElement;

		private ServerCollection _collection;

		public Server Current
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

		internal ServerCollectionEnumerator(ServerCollection collection)
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

	public Server this[int index]
	{
		get
		{
			return (Server)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(Server value)
	{
		return base.List.Add(value);
	}

	public bool Contains(Server value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(Server value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(Server value)
	{
		base.List.Remove(value);
	}

	public new ServerCollectionEnumerator GetEnumerator()
	{
		return new ServerCollectionEnumerator(this);
	}

	public void Insert(int index, Server value)
	{
		base.List.Insert(index, value);
	}
}
