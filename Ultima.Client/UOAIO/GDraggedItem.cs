using System.Reflection;
using System.Windows.Forms;
using Ultima.Data;
using UOAIO.Targeting;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GDraggedItem : Gump, IItemGump
{
	private bool m_Draw;

	private Texture m_Image;

	private int m_Width;

	private int m_Height;

	private Item m_Item;

	private IHue m_Hue;

	private bool m_Double;

	private int m_xOffset;

	private int m_yOffset;

	private VertexCache m_vCache;

	private VertexCache m_vCacheDouble;

	public int xOffset => this.m_xOffset;

	public int yOffset => this.m_yOffset;

	public int yBottom => this.m_Image.yMax;

	public bool Double => this.m_Double;

	public override int Width => this.m_Width;

	public override int Height => this.m_Height;

	public Item Item => this.m_Item;

	public IHue Hue => this.m_Hue;

	public Texture Image => this.m_Image;

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
		if (TargetManager.IsActive && (mb & MouseButtons.Left) != MouseButtons.None)
		{
			this.m_Item.OnTarget();
			Engine.CancelClick();
		}
	}

	protected internal override void OnDoubleClick(int X, int Y)
	{
		this.m_Item.OnDoubleClick();
	}

	public GDraggedItem(Item item)
		: base(0, 0)
	{
		this.m_vCache = new VertexCache();
		this.m_Item = item;
		int num = this.m_Item.ID;
		int num2 = (ushort)this.m_Item.Amount;
		this.m_Double = Map.m_ItemFlags[num][TileFlag.Generic] && num2 > 1;
		if (num >= 3818 && num <= 3826)
		{
			int num3 = (num - 3818) / 3;
			num3 *= 3;
			num3 += 3818;
			this.m_Double = false;
			num = ((num2 <= 1) ? num3 : ((num2 < 2 || num2 > 5) ? (num3 + 2) : (num3 + 1)));
		}
		this.m_Hue = Hues.GetItemHue(num, this.m_Item.Hue);
		this.m_Image = this.m_Hue.GetItem(num);
		if (this.m_Image != null && !this.m_Image.IsEmpty())
		{
			this.m_Draw = true;
			this.m_Width = this.m_Image.Width;
			this.m_Height = this.m_Image.Height;
			int num4 = ((!this.m_Double) ? 1 : 6);
			this.m_xOffset = (base.m_OffsetX = this.m_Image.xMin + (this.m_Image.xMax - this.m_Image.xMin + num4) / 2);
			this.m_yOffset = this.m_Image.yMin;
			base.m_OffsetY = this.m_yOffset + (this.m_Image.yMax - this.m_Image.yMin + num4) / 2;
			if (this.m_Double)
			{
				this.m_Width += 5;
				this.m_Height += 5;
			}
		}
		base.m_DragCursor = false;
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		base.m_IsDragging = true;
		Gumps.Drag = this;
		Gumps.LastOver = this;
		base.m_X = Engine.m_xMouse - base.m_OffsetX;
		base.m_Y = Engine.m_yMouse - base.m_OffsetY;
	}

	protected internal override bool HitTest(int x, int y)
	{
		if (this.m_Double)
		{
			return this.m_Draw && ((this.X < this.m_Image.Width && this.Y < this.m_Image.Height && this.m_Image.HitTest(x, y)) || (this.X >= 5 && this.Y >= 5 && this.m_Image.HitTest(x - 5, y - 5)));
		}
		return this.m_Draw && this.m_Image.HitTest(x, y);
	}

	protected internal override void Draw(int x, int y)
	{
		if (!this.m_Draw)
		{
			return;
		}
		this.m_vCache.Draw(this.m_Image, x, y);
		if (this.m_Double)
		{
			if (this.m_vCacheDouble == null)
			{
				this.m_vCacheDouble = new VertexCache();
			}
			this.m_vCacheDouble.Draw(this.m_Image, x + 5, y + 5);
		}
		this.m_Item.MessageX = this.X + this.m_xOffset;
		this.m_Item.MessageY = this.Y + this.m_yOffset;
		this.m_Item.BottomY = this.Y + this.m_Image.yMax;
		this.m_Item.MessageFrame = Renderer.m_ActFrames;
	}
}
