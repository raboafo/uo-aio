using System;
using System.Collections;

namespace UOAIO.Profiles;

public class PlayerCollection : CollectionBase
{
	public class PlayerCollectionEnumerator : IEnumerator
	{
		private int _index;

		private Player _currentElement;

		private PlayerCollection _collection;

		public Player Current
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

		internal PlayerCollectionEnumerator(PlayerCollection collection)
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

	public Player this[int index]
	{
		get
		{
			return (Player)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(Player value)
	{
		return base.List.Add(value);
	}

	public bool Contains(Player value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(Player value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(Player value)
	{
		base.List.Remove(value);
	}

	public new PlayerCollectionEnumerator GetEnumerator()
	{
		return new PlayerCollectionEnumerator(this);
	}

	public void Insert(int index, Player value)
	{
		base.List.Insert(index, value);
	}
}
