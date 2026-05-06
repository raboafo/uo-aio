using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using UOAIO.Profiles;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Gump
{
	protected bool m_Modal;

	protected int m_Handle;

	protected int m_X;

	protected int m_Y;

	protected GumpList m_Children;

	protected Gump m_Parent;

	private Dictionary<string, object> m_Tags;

	protected internal int m_OffsetX;

	protected internal int m_OffsetY;

	protected internal bool m_CanDrag;

	protected internal bool m_IsDragging;

	protected internal bool m_CanDrop;

	protected internal bool m_QuickDrag;

	protected internal bool m_Restore;

	protected internal int m_DragClipX = 50;

	protected internal int m_DragClipY = 50;

	protected string m_GUID = "";

	protected internal bool m_DragCursor = true;

	protected internal bool m_OverridesCursor = true;

	protected internal bool m_Visible = true;

	protected internal bool m_ITranslucent;

	protected internal ITooltip m_Tooltip;

	protected internal int m_OverCursor = 9;

	protected internal bool m_NonRestrictivePicking;

	protected internal bool m_Disposed;

	protected GumpLayout m_Layout;

	public bool Disposed => this.m_Disposed;

	public bool Visible
	{
		get
		{
			return this.m_Visible;
		}
		set
		{
			this.m_Visible = value;
			Gumps.Invalidate();
		}
	}

	public ITooltip Tooltip
	{
		get
		{
			return this.m_Tooltip;
		}
		set
		{
			this.m_Tooltip = value;
		}
	}

	public string GUID
	{
		get
		{
			return this.m_GUID;
		}
		set
		{
			this.m_GUID = value;
			Gumps.Restore(this);
		}
	}

	public bool Modal
	{
		get
		{
			return this.m_Modal;
		}
		set
		{
			if (value)
			{
				if (!this.HasTag("Dispose"))
				{
					this.SetTag("Dispose", "Modal");
				}
				Gumps.Modal = this;
				return;
			}
			if (this.HasTag("Dispose"))
			{
				this.RemoveTag("Dispose");
			}
			if (Gumps.Modal == this)
			{
				Gumps.Modal = null;
			}
		}
	}

	public Gump Parent
	{
		get
		{
			return this.m_Parent;
		}
		set
		{
			this.m_Parent = value;
			if (this.m_Parent != null)
			{
				this.DefineLayout();
			}
		}
	}

	public virtual int X
	{
		get
		{
			return this.m_X;
		}
		set
		{
			this.m_X = value;
			if (this.m_Layout != null)
			{
				this.m_Layout.Update(this);
			}
		}
	}

	public virtual int Y
	{
		get
		{
			return this.m_Y;
		}
		set
		{
			this.m_Y = value;
			if (this.m_Layout != null)
			{
				this.m_Layout.Update(this);
			}
		}
	}

	public GumpList Children => this.m_Children;

	public virtual int Width
	{
		get
		{
			return Engine.ScreenWidth;
		}
		set
		{
		}
	}

	public virtual int Height
	{
		get
		{
			return Engine.ScreenHeight;
		}
		set
		{
		}
	}

	public virtual Gump FindChild(Predicate<Gump> predicate)
	{
		if (predicate(this))
		{
			return this;
		}
		if (this.m_Children != null)
		{
			foreach (Gump child in this.m_Children)
			{
				Gump gump = child.FindChild(predicate);
				if (gump != null)
				{
					return gump;
				}
			}
		}
		return null;
	}

	public virtual void DefineLayout()
	{
		if (this.m_Layout == null)
		{
			this.SetLayout(this.CreateLayout());
			if (this.m_Layout != null)
			{
				this.m_Layout.Update(this);
				Preferences.Current.Layout.Gumps.Add(this.m_Layout);
			}
		}
	}

	public virtual GumpLayout CreateLayout()
	{
		return null;
	}

	public void SetLayout(GumpLayout layout)
	{
		this.m_Layout = layout;
	}

	public void ManualClose()
	{
		Gumps.Destroy(this);
		if (this.m_Layout != null)
		{
			this.m_Layout.Remove();
		}
	}

	public void BringToTop()
	{
		if (this.m_Parent != null)
		{
			int index = this.m_Parent.Children.IndexOf(this);
			this.m_Parent.Children.RemoveAt(index);
			this.m_Parent.Children.Add(this);
			this.m_Parent.BringToTop();
			if (this.m_Layout != null)
			{
				this.m_Layout.BringToTop();
			}
		}
	}

	public void OffsetChildren(int xOffset, int yOffset)
	{
		Gump[] array = this.m_Children.ToArray();
		foreach (Gump gump in array)
		{
			gump.X += xOffset;
			gump.Y += yOffset;
		}
	}

	public virtual void Center()
	{
		if (this.m_Parent == null)
		{
			this.X = (Engine.ScreenWidth - this.Width) / 2;
			this.Y = (Engine.ScreenHeight - this.Height) / 2;
		}
		else
		{
			this.X = (this.m_Parent.Width - this.Width) / 2;
			this.Y = (this.m_Parent.Height - this.Height) / 2;
		}
	}

	public void RemoveTag(string Name)
	{
		this.m_Tags.Remove(Name);
	}

	public object GetTag(string Name)
	{
		if (this.m_Tags == null)
		{
			return null;
		}
		object value = null;
		this.m_Tags.TryGetValue(Name, out value);
		return value;
	}

	public void SetTag(string Name, object Value)
	{
		if (this.m_Tags == null)
		{
			this.m_Tags = new Dictionary<string, object>();
		}
		this.m_Tags[Name] = Value;
	}

	public bool HasTag(string Name)
	{
		if (this.m_Tags == null)
		{
			return false;
		}
		return this.m_Tags.ContainsKey(Name);
	}

	public bool IsChildOf(Gump g)
	{
		for (Gump gump = this; gump != null; gump = gump.Parent)
		{
			if (gump == g)
			{
				return true;
			}
		}
		return false;
	}

	public Point PointToScreen(Point p)
	{
		int num = 0;
		int num2 = 0;
		for (Gump gump = this; gump != null; gump = gump.Parent)
		{
			num += gump.X;
			num2 += gump.Y;
		}
		return new Point(p, num, num2);
	}

	public Point PointToClient(Point p)
	{
		int num = 0;
		int num2 = 0;
		for (Gump gump = this; gump != null; gump = gump.Parent)
		{
			num += gump.X;
			num2 += gump.Y;
		}
		return new Point(p, -num, -num2);
	}

	public Gump(int X, int Y)
	{
		this.m_Children = new GumpList(this);
		this.m_X = X;
		this.m_Y = Y;
	}

	protected internal virtual void Draw(int X, int Y)
	{
	}

	protected internal virtual void Render(int X, int Y)
	{
		if (this.m_Visible)
		{
			int x = X + this.X;
			int y = Y + this.Y;
			this.Draw(x, y);
			Gump[] array = this.m_Children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Render(x, y);
			}
		}
	}

	protected internal virtual bool HitTest(int X, int Y)
	{
		return false;
	}

	protected internal virtual void OnDispose()
	{
	}

	protected internal virtual void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
	}

	protected internal virtual void OnMouseDown(int X, int Y, MouseButtons mb)
	{
	}

	protected internal virtual void OnMouseMove(int X, int Y, MouseButtons mb)
	{
	}

	protected internal virtual void OnMouseUp(int X, int Y, MouseButtons mb)
	{
	}

	protected internal virtual void OnMouseWheel(int Delta)
	{
	}

	protected internal virtual void OnSingleClick(int X, int Y)
	{
	}

	protected internal virtual void OnDoubleClick(int X, int Y)
	{
	}

	protected internal virtual bool OnKeyDown(char Key)
	{
		return false;
	}

	protected internal virtual void OnMouseLeave()
	{
	}

	protected internal virtual void OnFocusChanged(Gump Focused)
	{
	}

	protected internal virtual void OnDragDrop(Gump g)
	{
	}

	protected internal virtual void OnDragEnter(Gump g)
	{
	}

	protected internal virtual void OnDragLeave(Gump g)
	{
	}

	protected internal virtual void OnDragMove()
	{
	}

	protected internal virtual void OnDragStart()
	{
	}
}
