using System.Windows.Forms;
using Ultima.Data;
using UOAIO.Targeting;

namespace UOAIO;

public class GContainerItem : Gump, IItemGump
{
	private bool m_Draw;

	private Texture m_Image;

	private int m_Width;

	private int m_Height;

	private int m_State;

	private int m_TileID;

	private IHue m_Hue;

	private Item m_Item;

	private Item m_Container;

	private bool m_Double;

	private VertexCache m_vCache;

	private VertexCache m_vCacheDouble;

	private int m_xOffset;

	private int m_yOffset;

	private static VertexCachePool m_vPool;

	protected VertexCachePool VCPool => GContainerItem.m_vPool;

	public int xOffset => this.m_xOffset;

	public int yOffset => this.m_yOffset;

	public int yBottom => this.m_Image.yMax;

	public bool Double => this.m_Double;

	public Texture Image => this.m_Image;

	public Item Item => this.m_Item;

	public Item Container => this.m_Container;

	public int TileID => this.m_TileID;

	public override int Width => this.m_Width + (this.m_Double ? 5 : 0);

	public override int Height => this.m_Height + (this.m_Double ? 5 : 0);

	public int State
	{
		get
		{
			return this.m_State;
		}
		set
		{
			this.m_State = value;
			int num = this.m_TileID;
			int num2 = (ushort)this.m_Item.Amount;
			if (this.m_Item != null)
			{
				this.m_Double = Map.m_ItemFlags[num & 0x3FFF][TileFlag.Generic] && num2 > 1;
				if (this.m_TileID >= 3818 && this.m_TileID <= 3826)
				{
					int num3 = (this.m_TileID - 3818) / 3;
					num3 *= 3;
					num3 += 3818;
					this.m_Double = false;
					num = ((num2 <= 1) ? num3 : ((num2 < 2 || num2 > 5) ? (num3 + 2) : (num3 + 1)));
				}
			}
			this.m_Image = ((this.m_State == 0) ? this.m_Hue : Hues.Load(32821)).GetItem(num);
			if (this.m_Image != null && !this.m_Image.IsEmpty())
			{
				this.m_xOffset = this.m_Image.xMin + (this.m_Image.xMax - this.m_Image.xMin + ((!this.m_Double) ? 1 : 6)) / 2;
				this.m_yOffset = this.m_Image.yMin;
				this.m_Width = this.m_Image.Width;
				this.m_Height = this.m_Image.Height;
				this.m_Draw = this.m_Item != null;
			}
			else
			{
				this.m_xOffset = (this.m_yOffset = (this.m_Width = (this.m_Height = 0)));
				this.m_Draw = false;
			}
			if (this.m_vCache != null)
			{
				this.m_vCache.Invalidate();
			}
			if (this.m_vCacheDouble != null)
			{
				this.m_vCacheDouble.Invalidate();
			}
		}
	}

	public override int Y
	{
		get
		{
			int num = this.m_Item.ID & 0x3FFF;
			int num2 = base.Y;
			if (num >= 13701 && num <= 13706)
			{
				num2 -= 20;
			}
			else if (num >= 13708 && num <= 13713)
			{
				num2 -= 20;
			}
			return num2;
		}
		set
		{
			base.Y = value;
		}
	}

	protected internal override void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
		this.State = 1;
	}

	protected internal override void OnMouseLeave()
	{
		this.State = 0;
		if (base.Tooltip != null)
		{
			((ItemTooltip)base.Tooltip).Gump = null;
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
	}

	protected internal override void OnSingleClick(int x, int y)
	{
		if (!TargetManager.IsActive)
		{
			this.m_Item.OnSingleClick();
		}
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Point p = base.PointToScreen(new Point(x, y));
			p = base.m_Parent.PointToClient(p);
			base.m_Parent.OnMouseUp(p.X, p.Y, mb);
		}
		else if (TargetManager.IsActive && (mb & MouseButtons.Left) != MouseButtons.None)
		{
			this.m_Item.OnTarget();
			Engine.CancelClick();
		}
		else if ((mb & MouseButtons.Left) != MouseButtons.None && (Control.ModifierKeys & Keys.Shift) != Keys.None)
		{
			Network.Send(new PPopupRequest(this.m_Item));
		}
	}

	protected internal override void OnDoubleClick(int X, int Y)
	{
		this.m_Item.OnDoubleClick();
	}

	protected internal override void OnDispose()
	{
		this.VCPool.ReleaseInstance(this.m_vCache);
		this.m_vCache = null;
		this.VCPool.ReleaseInstance(this.m_vCacheDouble);
		this.m_vCacheDouble = null;
	}

	public GContainerItem(Item Item, Item Container)
		: base(Item.X, Item.Y)
	{
		this.m_Item = Item;
		this.m_Container = Container;
		this.m_TileID = this.m_Item.ID;
		this.m_Hue = Hues.GetItemHue(this.m_TileID, this.m_Item.Hue);
		this.State = 0;
		base.m_CanDrag = true;
		base.m_CanDrop = true;
		base.m_QuickDrag = false;
		base.m_DragCursor = false;
		if (Engine.ServerFeatures.AOS)
		{
			base.Tooltip = new ItemTooltip(this.m_Item);
		}
	}

	public void Refresh()
	{
		this.m_TileID = this.m_Item.ID;
		this.m_Hue = Hues.GetItemHue(this.m_TileID, this.m_Item.Hue);
		this.State = 0;
		base.m_CanDrag = true;
		base.m_CanDrop = true;
		base.m_QuickDrag = false;
		base.m_DragCursor = false;
	}

	protected internal override void OnDragStart()
	{
		if (this.m_Item == null)
		{
			return;
		}
		base.m_IsDragging = false;
		Gumps.LastOver = null;
		Gumps.Drag = null;
		this.State = 0;
		Gump gump = this.m_Item.OnBeginDrag();
		if (gump.GetType() == typeof(GDragAmount))
		{
			((GDragAmount)gump).ToDestroy = this;
			return;
		}
		this.m_Item.RestoreInfo = new RestoreInfo(this.m_Item);
		World.Remove(this.m_Item);
		gump.m_OffsetX = base.m_OffsetX;
		gump.m_OffsetY = base.m_OffsetY;
		gump.X = Engine.m_xMouse - base.m_OffsetX;
		gump.Y = Engine.m_yMouse - base.m_OffsetY;
		if (base.m_Parent is GContainer)
		{
			((GContainer)base.m_Parent).m_Hash[this.m_Item] = null;
		}
		Gumps.Destroy(this);
	}

	protected internal override void OnDragDrop(Gump g)
	{
		if (g == null || !(g.GetType() == typeof(GDraggedItem)))
		{
			return;
		}
		GDraggedItem gDraggedItem = (GDraggedItem)g;
		Item item = gDraggedItem.Item;
		if (((GContainer)base.m_Parent).m_HitTest)
		{
			TileFlags tileFlags = Map.m_ItemFlags[this.m_Item.ID];
			if (tileFlags[TileFlag.Container])
			{
				Network.Send(new PDropItem(item.Serial, -1, -1, 0, this.m_Item.Serial));
				Gumps.Destroy(gDraggedItem);
			}
			else if (tileFlags[TileFlag.Generic] && item.ID == this.m_Item.ID && item.Hue == this.m_Item.Hue)
			{
				Point point = ((GContainer)base.m_Parent).Clip(gDraggedItem.Image, gDraggedItem.Double, base.m_Parent.PointToClient(new Point(Engine.m_xMouse - gDraggedItem.m_OffsetX, Engine.m_yMouse - gDraggedItem.m_OffsetY)), gDraggedItem.m_OffsetX, gDraggedItem.m_OffsetY);
				Network.Send(new PDropItem(item.Serial, (short)point.X, (short)point.Y, 0, this.m_Item.Serial));
				Gumps.Destroy(gDraggedItem);
			}
			else
			{
				base.m_Parent.OnDragDrop(gDraggedItem);
			}
		}
		else
		{
			base.m_Parent.OnDragDrop(gDraggedItem);
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		if (!this.m_Draw)
		{
			return;
		}
		if (this.m_vCache == null)
		{
			this.m_vCache = this.VCPool.GetInstance();
		}
		this.m_vCache.Draw(this.m_Image, X, Y);
		if (this.m_Double)
		{
			if (this.m_vCacheDouble == null)
			{
				this.m_vCacheDouble = this.VCPool.GetInstance();
			}
			this.m_vCacheDouble.Draw(this.m_Image, X + 5, Y + 5);
		}
		this.m_Item.MessageX = X + this.m_xOffset;
		this.m_Item.MessageY = Y + this.m_yOffset;
		this.m_Item.BottomY = Y + this.m_Image.yMax;
		this.m_Item.MessageFrame = Renderer.m_ActFrames;
	}

	protected internal override bool HitTest(int X, int Y)
	{
		if (this.m_Double)
		{
			return this.m_Draw && ((X < this.m_Image.Width && Y < this.m_Image.Height && this.m_Image.HitTest(X, Y)) || (X >= 5 && Y >= 5 && this.m_Image.HitTest(X - 5, Y - 5)));
		}
		return this.m_Draw && this.m_Image.HitTest(X, Y);
	}

	static GContainerItem()
	{
		GContainerItem.m_vPool = new VertexCachePool();
	}
}
