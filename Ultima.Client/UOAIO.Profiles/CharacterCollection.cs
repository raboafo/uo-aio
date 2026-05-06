using System;
using System.Collections;

namespace UOAIO.Profiles;

public class CharacterCollection : CollectionBase
{
	public class CharacterCollectionEnumerator : IEnumerator
	{
		private int _index;

		private Character _currentElement;

		private CharacterCollection _collection;

		public Character Current
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

		internal CharacterCollectionEnumerator(CharacterCollection collection)
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

	public Character this[int index]
	{
		get
		{
			return (Character)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(Character value)
	{
		return base.List.Add(value);
	}

	public bool Contains(Character value)
	{
		return base.List.Contains(value);
	}

	public int IndexOf(Character value)
	{
		return base.List.IndexOf(value);
	}

	public void Remove(Character value)
	{
		base.List.Remove(value);
	}

	public new CharacterCollectionEnumerator GetEnumerator()
	{
		return new CharacterCollectionEnumerator(this);
	}

	public void Insert(int index, Character value)
	{
		base.List.Insert(index, value);
	}
}
