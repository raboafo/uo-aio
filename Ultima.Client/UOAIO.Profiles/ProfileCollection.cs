using System;
using System.Collections;

namespace UOAIO.Profiles;

public class ProfileCollection : CollectionBase
{
	public class ProfileCollectionEnumerator : IEnumerator
	{
		private int _index;

		private Profile _currentElement;

		private ProfileCollection _collection;

		public Profile Current
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

		internal ProfileCollectionEnumerator(ProfileCollection collection)
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

	public Profile this[int index]
	{
		get
		{
			return (Profile)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(Profile value)
	{
		return base.List.Add(value);
	}

	public bool Contains(Profile value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(Profile value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(Profile value)
	{
		base.List.Remove(value);
	}

	public new ProfileCollectionEnumerator GetEnumerator()
	{
		return new ProfileCollectionEnumerator(this);
	}

	public void Insert(int index, Profile value)
	{
		base.List.Insert(index, value);
	}
}
