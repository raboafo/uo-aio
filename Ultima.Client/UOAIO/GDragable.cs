using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GDragable : Gump, ITranslucent
{
	protected bool m_Draw;

	protected Texture m_Gump;

	protected VertexCache m_vCache;

	protected int m_Width;

	protected int m_Height;

	protected bool m_Drag;

	protected bool m_CanClose = true;

	protected bool m_LinksMoved;

	protected List<Gump> m_Dockers;

	protected List<Gump> m_Linked;

	protected bool m_bAlpha;

	protected float m_fAlpha = 1f;

	protected int m_GumpID;

	protected IHue m_Hue;

	public int GumpID
	{
		get
		{
			return this.m_GumpID;
		}
		set
		{
			if (this.m_GumpID != value)
			{
				this.m_GumpID = value;
				this.m_vCache.Invalidate();
				this.m_Gump = this.m_Hue.GetGump(this.m_GumpID);
				if (this.m_Gump != null && !this.m_Gump.IsEmpty())
				{
					this.m_Width = this.m_Gump.Width;
					this.m_Height = this.m_Gump.Height;
					this.m_Draw = true;
				}
				else
				{
					this.m_Draw = false;
				}
			}
		}
	}

	public IHue Hue
	{
		get
		{
			return this.m_Hue;
		}
		set
		{
			if (this.m_Hue != value)
			{
				this.m_Hue = value;
				this.m_Gump = this.m_Hue.GetGump(this.m_GumpID);
				this.m_vCache.Invalidate();
				if (this.m_Gump != null && !this.m_Gump.IsEmpty())
				{
					this.m_Width = this.m_Gump.Width;
					this.m_Height = this.m_Gump.Height;
					this.m_Draw = true;
				}
				else
				{
					this.m_Draw = false;
				}
			}
		}
	}

	public float Alpha
	{
		get
		{
			return this.m_fAlpha;
		}
		set
		{
			this.m_fAlpha = value;
			this.m_bAlpha = value != 1f;
		}
	}

	public bool CanClose
	{
		get
		{
			return this.m_CanClose;
		}
		set
		{
			this.m_CanClose = value;
		}
	}

	public bool Drag
	{
		get
		{
			return this.m_Drag;
		}
		set
		{
			this.m_Drag = value;
		}
	}

	public int OffsetX
	{
		get
		{
			return base.m_OffsetX;
		}
		set
		{
			base.m_OffsetX = value;
		}
	}

	public int OffsetY
	{
		get
		{
			return base.m_OffsetY;
		}
		set
		{
			base.m_OffsetY = value;
		}
	}

	public override int Width => this.m_Width;

	public override int Height => this.m_Height;

	public GDragable(int GumpID, int X, int Y)
		: this(GumpID, Hues.Default, X, Y)
	{
	}

	public GDragable(int GumpID, IHue Hue, int X, int Y)
		: base(X, Y)
	{
		this.m_vCache = new VertexCache();
		this.m_GumpID = GumpID;
		this.m_Hue = Hue;
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		this.m_Dockers = new List<Gump>();
		this.m_Linked = new List<Gump>();
		this.m_Gump = Hue.GetGump(GumpID);
		if (this.m_Gump != null && !this.m_Gump.IsEmpty())
		{
			this.m_Width = this.m_Gump.Width;
			this.m_Height = this.m_Gump.Height;
			this.m_Draw = true;
		}
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (this.m_CanClose && mb == MouseButtons.Right)
		{
			Gumps.Destroy(this);
			Engine.CancelClick();
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		if (this.m_Draw)
		{
			Renderer.PushAlpha(this.m_fAlpha);
			this.m_vCache.Draw(this.m_Gump, X, Y);
			Renderer.PopAlpha();
		}
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return this.m_Draw && this.m_Gump.HitTest(X, Y);
	}
}
