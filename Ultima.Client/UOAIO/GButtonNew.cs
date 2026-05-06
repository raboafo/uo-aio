using System;
using System.Windows.Forms;

namespace UOAIO;

public class GButtonNew : Gump, IClickable
{
	protected bool m_Enabled;

	protected int[] m_GumpID;

	protected int m_State;

	protected bool m_Invalidated;

	protected bool m_Draw;

	protected Texture m_Image;

	protected int m_Width;

	protected int m_Height;

	protected bool m_CanEnter;

	protected VertexCache m_vCache;

	private static VertexCachePool m_vPool;

	protected VertexCachePool VCPool => GButtonNew.m_vPool;

	public bool CanEnter
	{
		get
		{
			return this.m_CanEnter;
		}
		set
		{
			this.m_CanEnter = value;
		}
	}

	public int State
	{
		get
		{
			return this.m_State;
		}
		set
		{
			if (this.m_State != value)
			{
				this.m_State = value;
				this.Invalidate();
			}
		}
	}

	public override int Width
	{
		get
		{
			if (this.m_Invalidated)
			{
				this.Refresh();
			}
			return this.m_Width;
		}
	}

	public override int Height
	{
		get
		{
			if (this.m_Invalidated)
			{
				this.Refresh();
			}
			return this.m_Height;
		}
	}

	public bool Enabled
	{
		get
		{
			return this.m_Enabled;
		}
		set
		{
			if (this.m_Enabled != value)
			{
				this.m_Enabled = value;
				if (!this.m_Enabled)
				{
					this.State = 0;
				}
				this.Invalidate();
			}
		}
	}

	public event EventHandler Clicked;

	protected internal override void OnDispose()
	{
		this.VCPool.ReleaseInstance(this.m_vCache);
		this.m_vCache = null;
	}

	protected internal override bool OnKeyDown(char c)
	{
		if (this.m_CanEnter && c == '\r')
		{
			this.Click();
			return true;
		}
		return false;
	}

	protected void Invalidate()
	{
		this.m_Invalidated = true;
	}

	protected internal override void Draw(int x, int y)
	{
		if (this.m_Invalidated)
		{
			this.Refresh();
		}
		if (this.m_Draw)
		{
			if (this.m_vCache == null)
			{
				this.m_vCache = this.VCPool.GetInstance();
			}
			this.m_vCache.Draw(this.m_Image, x, y);
		}
	}

	protected virtual void Refresh()
	{
		this.m_Image = (this.m_Enabled ? Hues.Default : Hues.Grayscale).GetGump(this.m_GumpID[this.m_State]);
		if (this.m_Image != null && !this.m_Image.IsEmpty())
		{
			this.m_Width = this.m_Image.Width;
			this.m_Height = this.m_Image.Height;
			this.m_Draw = true;
		}
		else
		{
			this.m_Width = 0;
			this.m_Height = 0;
			this.m_Draw = false;
		}
		this.m_Invalidated = false;
		if (this.m_vCache != null)
		{
			this.m_vCache.Invalidate();
		}
	}

	protected internal override bool HitTest(int x, int y)
	{
		if (this.m_Invalidated)
		{
			this.Refresh();
		}
		return this.m_Draw && this.m_Image.HitTest(x, y);
	}

	protected internal override void OnMouseEnter(int x, int y, MouseButtons mb)
	{
		if (this.m_Enabled)
		{
			this.State = (((mb & MouseButtons.Left) == 0) ? 1 : 2);
		}
	}

	protected internal override void OnMouseLeave()
	{
		if (this.m_Enabled)
		{
			this.State = 0;
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		this.OnMouseEnter(x, y, mb);
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if (this.m_Enabled && (mb & MouseButtons.Left) != MouseButtons.None)
		{
			this.State = 1;
			this.InternalOnClicked();
		}
	}

	public GButtonNew(int gumpID, int x, int y)
		: this(gumpID, gumpID + 1, gumpID + 2, x, y)
	{
	}

	public GButtonNew(int inactiveID, int focusID, int pressedID, int x, int y)
		: base(x, y)
	{
		this.m_GumpID = new int[3] { inactiveID, focusID, pressedID };
		this.m_Enabled = true;
		this.m_Invalidated = true;
	}

	public void Click()
	{
		for (int i = 1; i <= 3; i++)
		{
			this.State = i % 3;
			Engine.DrawNow();
		}
		this.InternalOnClicked();
	}

	private void InternalOnClicked()
	{
		this.OnClicked();
		if (this.Clicked != null)
		{
			this.Clicked(this, EventArgs.Empty);
		}
	}

	protected virtual void OnClicked()
	{
	}

	static GButtonNew()
	{
		GButtonNew.m_vPool = new VertexCachePool();
	}
}
