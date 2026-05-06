using System;
using System.Collections;

namespace UOAIO;

public class RuneInfoExCollection : CollectionBase, IDisposable
{
	public class RuneInfoExCollectionEnumerator : IEnumerator, IDisposable
	{
		private int _index;

		private RuneInfoEx _currentElement;

		private RuneInfoExCollection _collection;

		public RuneInfoEx Current
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

		object IEnumerator.Current => this.Current;

		internal RuneInfoExCollectionEnumerator(RuneInfoExCollection collection)
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

		public void Dispose()
		{
			this._collection = null;
			this._currentElement = null;
		}
	}

	private bool _disposed;

	public RuneInfoEx this[int index]
	{
		get
		{
			return (RuneInfoEx)base.List[index];
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(RuneInfoEx value)
	{
		this.ThrowIfDisposed();
		return base.List.Add(value);
	}

	public bool Contains(RuneInfoEx value)
	{
		this.ThrowIfDisposed();
		return base.List.Contains(value);
	}

	public int IndexOf(RuneInfoEx value)
	{
		this.ThrowIfDisposed();
		return base.List.IndexOf(value);
	}

	public void Remove(RuneInfoEx value)
	{
		this.ThrowIfDisposed();
		base.List.Remove(value);
	}

	public new RuneInfoExCollectionEnumerator GetEnumerator()
	{
		this.ThrowIfDisposed();
		return new RuneInfoExCollectionEnumerator(this);
	}

	public void Insert(int index, RuneInfoEx value)
	{
		this.ThrowIfDisposed();
		base.List.Insert(index, value);
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (this._disposed)
		{
			return;
		}
		if (disposing)
		{
			foreach (object item in base.List)
			{
				if (item is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			base.List.Clear();
		}
		this._disposed = true;
	}

	private void ThrowIfDisposed()
	{
		if (this._disposed)
		{
			throw new ObjectDisposedException("RuneInfoExCollection");
		}
	}
}
