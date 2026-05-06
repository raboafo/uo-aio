using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UOAIO.Profiles;

namespace UOAIO;

public class GContainer : GFader, IContainerView, IAgentView
{
	private Item m_Item;

	private int m_xBoundLeft;

	private int m_yBoundTop;

	private int m_xBoundRight;

	private int m_yBoundBottom;

	public bool m_TradeContainer;

	public bool m_NoDrop;

	public bool m_HitTest = true;

	protected internal Dictionary<Item, GContainerItem> m_Hash = new Dictionary<Item, GContainerItem>();

	public Item Item => this.m_Item;

	public Gump Gump => this;

	public void Close()
	{
		Engine.Sounds.PlayContainerClose(base.m_GumpID);
		Gumps.Destroy(this);
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (base.m_CanClose && mb == MouseButtons.Right)
		{
			if (this.m_TradeContainer)
			{
				((GSecureTrade)base.m_Parent).Close();
			}
			else
			{
				this.Close();
			}
		}
	}

	public void OnChildUpdated(Item item)
	{
		if (base.m_GumpID == 9 && (item.ID & 0x3FFF) >= 8198 && (item.ID & 0x3FFF) <= 8272)
		{
			this.OnChildRemoved(item);
			return;
		}
		GContainerItem value = null;
		this.m_Hash.TryGetValue(item, out value);
		if (value == null)
		{
			value = new GContainerItem(item, this.m_Item);
			this.m_Hash[item] = value;
			base.m_Children.Add(value);
		}
		else
		{
			value.Refresh();
		}
		value.m_CanDrag = !this.m_TradeContainer;
	}

	public void OnChildAdded(Item item)
	{
		if (base.m_GumpID != 9 || (item.ID & 0x3FFF) < 8198 || (item.ID & 0x3FFF) > 8272)
		{
			GContainerItem value = null;
			this.m_Hash.TryGetValue(item, out value);
			if (value != null)
			{
				Gumps.Destroy(value);
			}
			value = (this.m_Hash[item] = new GContainerItem(item, this.m_Item));
			value.m_CanDrag = !this.m_TradeContainer;
			base.m_Children.Add(value);
		}
	}

	public void OnChildRemoved(Item item)
	{
		GContainerItem value = null;
		this.m_Hash.TryGetValue(item, out value);
		if (value != null)
		{
			Gumps.Destroy(value);
			this.m_Hash[item] = null;
		}
	}

	public GContainer(Item container, int gumpID)
		: this(container, gumpID, Hues.Default)
	{
	}

	public GContainer(Item container, int gumpID, IHue hue)
		: base(0.25f, 0.25f, 0.6f, gumpID, 50, 50, hue)
	{
		this.m_Item = container;
		base.m_CanDrop = true;
		this.GetBounds(gumpID);
		base.m_NonRestrictivePicking = true;
		foreach (Item item in container.Items)
		{
			if (base.m_GumpID != 9 || (item.ID & 0x3FFF) < 8198 || (item.ID & 0x3FFF) > 8272)
			{
				GContainerItem gContainerItem = new GContainerItem(item, this.m_Item);
				this.m_Hash[item] = gContainerItem;
				gContainerItem.m_CanDrag = !this.m_TradeContainer;
				base.m_Children.Add(gContainerItem);
			}
		}
	}

	private void GetBounds(int gumpID)
	{
		Rectangle rectangle = Engine.ContainerBoundsTable.Translate(gumpID);
		this.m_xBoundLeft = rectangle.X;
		this.m_yBoundTop = rectangle.Y;
		this.m_xBoundRight = rectangle.Right - 1;
		this.m_yBoundBottom = rectangle.Bottom - 1;
	}

	protected internal override bool HitTest(int x, int y)
	{
		return this.m_HitTest && base.HitTest(x, y);
	}

	protected internal override void OnDispose()
	{
		if (this.m_Item.ContainerView == this)
		{
			this.m_Item.SetContainerView(null);
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
	}

	protected internal override void Draw(int x, int y)
	{
		if (!this.m_TradeContainer)
		{
			base.Draw(x, y);
		}
		if (base.m_GumpID != 2330 && base.m_GumpID != 2350 && base.m_GumpID != 82 && Options.Current.ContainerGrid)
		{
			Rectangle grid = this.GetGrid();
			Renderer.SetTexture(null);
			float alpha = (float)((double)base.m_fAlpha * Math.Sqrt(base.m_fAlpha));
			Renderer.PushAlpha(alpha);
			int num = 0;
			int num2 = x + grid.Left;
			while (num <= grid.Width)
			{
				Renderer.DrawLine(num2 - 1, y + grid.Top - 1, num2 - 1, y + grid.Top + grid.Height * 21);
				num++;
				num2 += 21;
			}
			int num3 = 0;
			int num4 = y + grid.Top;
			while (num3 <= grid.Height)
			{
				Renderer.DrawLine(x + grid.Left - 1, num4 - 1, x + grid.Left + grid.Width * 21, num4 - 1);
				num3++;
				num4 += 21;
			}
			Renderer.PopAlpha();
		}
	}

	protected internal override void OnDragStart()
	{
		if (this.m_TradeContainer)
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
			Point point = base.m_Parent.PointToClient(new Point(Engine.m_xMouse, Engine.m_yMouse));
			base.m_Parent.m_OffsetX = point.X;
			base.m_Parent.m_OffsetY = point.Y;
			base.m_Parent.m_IsDragging = true;
			Gumps.Drag = base.m_Parent;
		}
	}

	public Rectangle GetGrid()
	{
		int num = this.m_xBoundRight - this.m_xBoundLeft + 1;
		int num2 = this.m_yBoundBottom - this.m_yBoundTop + 1;
		int num3 = 0;
		int num4 = 0;
		while (num4 + 20 < num)
		{
			num4 += 21;
			num3++;
		}
		int num5 = 0;
		int num6 = 0;
		while (num6 + 20 < num2)
		{
			num6 += 21;
			num5++;
		}
		return new Rectangle(this.m_xBoundLeft + (num - num3 * 21) / 2, this.m_yBoundTop + (num2 - num5 * 21) / 2, num3, num5);
	}

	public Point Clip(Texture img, bool xDouble, Point p, int xOffset, int yOffset)
	{
		if (base.m_GumpID == 2330 || base.m_GumpID == 2350)
		{
			return p;
		}
		if (Options.Current.ContainerGrid)
		{
			Rectangle grid = this.GetGrid();
			Point point = new Point(p, xOffset, yOffset) - new Point(grid.Left, grid.Top);
			point /= 21;
			if (point.X < 0)
			{
				point.X = 0;
			}
			if (point.Y < 0)
			{
				point.Y = 0;
			}
			if (point.X >= grid.Width)
			{
				point.X = grid.Width - 1;
			}
			if (point.Y >= grid.Height)
			{
				point.Y = grid.Height - 1;
			}
			Point point2 = new Point(grid.X + point.X * 21 + (20 - (((!xDouble) ? 1 : 6) + (img.xMax - img.xMin))) / 2, grid.Y + point.Y * 21 + (20 - (((!xDouble) ? 1 : 6) + (img.yMax - img.yMin))) / 2);
			point2.X -= img.xMin;
			point2.Y -= img.yMin;
			return point2;
		}
		Point point3 = new Point(p.X, p.Y);
		int num = p.X + img.xMin;
		int num2 = p.Y + img.yMin;
		int num3 = p.X + img.xMax;
		int num4 = p.Y + img.yMax;
		if (num < this.m_xBoundLeft)
		{
			point3.X = this.m_xBoundLeft - img.xMin;
		}
		if (num2 < this.m_yBoundTop)
		{
			point3.Y = this.m_yBoundTop - img.yMin;
		}
		if (xDouble)
		{
			num3 += 5;
			num4 += 5;
			if (num3 > this.m_xBoundRight)
			{
				point3.X = this.m_xBoundRight - img.xMax - 5;
			}
			if (num4 > this.m_yBoundBottom)
			{
				point3.Y = this.m_yBoundBottom - img.yMax - 5;
			}
		}
		else
		{
			if (num3 > this.m_xBoundRight)
			{
				point3.X = this.m_xBoundRight - img.xMax;
			}
			if (num4 > this.m_yBoundBottom)
			{
				point3.Y = this.m_yBoundBottom - img.yMax;
			}
		}
		return point3;
	}

	protected internal override void OnDragDrop(Gump g)
	{
		if (!this.m_HitTest)
		{
			base.m_Parent.OnDragDrop(g);
		}
		else if (g != null && g.GetType() == typeof(GDraggedItem))
		{
			GDraggedItem gDraggedItem = (GDraggedItem)g;
			Point point = this.Clip(gDraggedItem.Image, gDraggedItem.Double, base.PointToClient(new Point(Engine.m_xMouse - g.m_OffsetX, Engine.m_yMouse - g.m_OffsetY)), g.m_OffsetX, g.m_OffsetY);
			int num = gDraggedItem.Item.ID & 0x3FFF;
			if (num >= 13701 && num <= 13706)
			{
				point.Y += 20;
			}
			else if (num >= 13708 && num <= 13713)
			{
				point.Y += 20;
			}
			Gumps.Destroy(gDraggedItem);
			Network.Send(new PDropItem(gDraggedItem.Item.Serial, (short)point.X, (short)point.Y, 0, this.m_Item.Serial));
		}
	}

	public void OnAgentUpdated()
	{
	}

	public void OnAgentDeleted()
	{
		this.Close();
	}
}
