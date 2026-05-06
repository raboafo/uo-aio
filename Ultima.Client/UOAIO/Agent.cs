using System;
using System.Collections.Generic;

namespace UOAIO;

public abstract class Agent : IEntity, IPoint3D, IPoint2D
{
	private static List<Item> _emptyItems;

	private int _serial;

	private Agent _parent;

	private int _x;

	private int _y;

	private int _z;

	private List<Mobile> _mobiles;

	private List<Item> _items;

	public int Serial => this._serial;

	public Agent Parent => this._parent;

	public int X => this._x;

	public int Y => this._y;

	public int Z => this._z;

	public bool InWorld => this._parent is WorldAgent;

	public Agent WorldRoot
	{
		get
		{
			Agent agent = this;
			while (true)
			{
				if (agent._parent == null)
				{
					return null;
				}
				if (agent.InWorld)
				{
					break;
				}
				agent = agent._parent;
			}
			return agent;
		}
	}

	public bool HasItems => this._items != null && this._items.Count > 0;

	public List<Item> Items
	{
		get
		{
			if (this._items != null)
			{
				return this._items;
			}
			return Agent._emptyItems;
		}
	}

	public Agent(int serial)
	{
		this._serial = serial;
	}

	public void SetLocation(int x, int y, int z)
	{
		this.SetLocation(this._parent, x, y, z);
	}

	public void SetLocation(Agent parent, int x, int y, int z)
	{
		if (this._parent != parent)
		{
			if (this._parent != null)
			{
				this._parent.RemoveChild(this);
			}
			this._x = x;
			this._y = y;
			this._z = z;
			this.OnLocationChanged();
			parent?.AddChild(this);
		}
		else
		{
			this._x = x;
			this._y = y;
			this._z = z;
			this.OnLocationChanged();
		}
	}

	public bool IsChildOf(Agent agent)
	{
		if (agent == null)
		{
			return false;
		}
		for (Agent parent = this._parent; parent != null; parent = parent._parent)
		{
			if (parent == agent)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetWorldLocation(out int x, out int y, out int z)
	{
		Agent worldRoot = this.WorldRoot;
		if (worldRoot != null)
		{
			x = worldRoot.X;
			y = worldRoot.Y;
			z = worldRoot.Z;
			return true;
		}
		x = -1;
		y = -1;
		z = 0;
		return false;
	}

	public double DistanceSqrt(IPoint2D p)
	{
		int num = this._x - p.X;
		int num2 = this._y - p.Y;
		return Math.Sqrt(num * num + num2 * num2);
	}

	public int DistanceTo(int xTile, int yTile)
	{
		int num = this._x - xTile;
		int num2 = this._y - yTile;
		return (int)Math.Sqrt(num * num + num2 * num2);
	}

	public bool InRange(IPoint2D p, int range)
	{
		return p != null && this.InRange(p.X, p.Y, range);
	}

	public bool InRange(int x, int y, int range)
	{
		if (this.GetWorldLocation(out var x2, out var y2, out var _))
		{
			return x2 >= x - range && x2 <= x + range && y2 >= y - range && y2 <= y + range;
		}
		return false;
	}

	private void ClearChildren()
	{
		if (this._items != null)
		{
			this._items.Clear();
		}
		if (this._mobiles != null)
		{
			this._mobiles.Clear();
		}
	}

	private void AddChild(Agent child)
	{
		if (child.Parent == this)
		{
			return;
		}
		if (child.Parent != null)
		{
			child.Parent.RemoveChild(child);
		}
		child._parent = this;
		if (child is Item)
		{
			if (this._items == null)
			{
				this._items = new List<Item>();
			}
			this._items.Add((Item)child);
		}
		else if (child is Mobile)
		{
			if (this._mobiles == null)
			{
				this._mobiles = new List<Mobile>();
			}
			this._mobiles.Add((Mobile)child);
		}
		this.OnChildAdded(child);
		child.OnParentChanged(this);
	}

	private void RemoveChild(Agent child)
	{
		if (child.Parent == this)
		{
			child._parent = null;
			if (this._items != null && child is Item)
			{
				this._items.Remove((Item)child);
			}
			if (this._mobiles != null && child is Mobile)
			{
				this._mobiles.Remove((Mobile)child);
			}
			this.OnChildRemoved(child);
			child.OnParentChanged(null);
		}
	}

	public void Delete()
	{
		this.OnDeleted();
		while (this._items != null && this._items.Count > 0)
		{
			this._items[this._items.Count - 1].Delete();
		}
		while (this._mobiles != null && this._mobiles.Count > 0)
		{
			this._mobiles[this._mobiles.Count - 1].Delete();
		}
	}

	protected virtual void OnDeleted()
	{
		if (this._parent != null)
		{
			this._parent.RemoveChild(this);
		}
	}

	protected virtual void OnLocationChanged()
	{
	}

	protected virtual void OnParentChanged(Agent parent)
	{
	}

	protected virtual void OnChildAdded(Agent child)
	{
	}

	protected virtual void OnChildRemoved(Agent child)
	{
	}

	static Agent()
	{
		Agent._emptyItems = new List<Item>();
	}
}
