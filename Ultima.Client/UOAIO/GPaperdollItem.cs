using System;
using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class GPaperdollItem : GImage, IItemGump, IAgentView
{
	private Mobile _mobile;

	private Item _item;

	private bool _canLift;

	private int m_xOffset;

	private int m_yOffset;

	public Mobile Mobile
	{
		get
		{
			return this._mobile;
		}
		set
		{
			this._mobile = value;
		}
	}

	public bool CanLift
	{
		get
		{
			return this._canLift;
		}
		set
		{
			this._canLift = value;
		}
	}

	public int xOffset => this.m_xOffset;

	public int yOffset => this.m_yOffset;

	public int yBottom => this.m_yOffset;

	public Item Item => this._item;

	protected internal override void OnSingleClick(int x, int y)
	{
		if (!TargetManager.IsActive)
		{
			this._item.OnSingleClick();
			this.m_xOffset = x;
			this.m_yOffset = y;
		}
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Point p = base.PointToScreen(new Point(X, Y));
			p = base.m_Parent.PointToClient(p);
			base.m_Parent.OnMouseUp(p.X, p.Y, mb);
		}
		else if (TargetManager.IsActive && (mb & MouseButtons.Left) != MouseButtons.None)
		{
			this._item.OnTarget();
			Engine.CancelClick();
		}
		else if ((mb & MouseButtons.Left) != MouseButtons.None && (Control.ModifierKeys & Keys.Shift) != Keys.None)
		{
			Network.Send(new PPopupRequest(this._item));
		}
	}

	protected internal override void OnDispose()
	{
		if (this._item.PaperdollItem == this)
		{
			this._item.PaperdollItem = null;
		}
	}

	protected internal override void OnDoubleClick(int X, int Y)
	{
		this._item.OnDoubleClick();
		this.m_xOffset = X;
		this.m_yOffset = Y;
	}

	protected internal override void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
	}

	protected internal override void OnMouseLeave()
	{
		if (base.Tooltip != null)
		{
			((ItemTooltip)base.Tooltip).Gump = null;
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.m_Parent.BringToTop();
	}

	public GPaperdollItem(Mobile mob, Item item, bool canLift)
		: base(8, 19)
	{
		this._mobile = mob;
		this._item = item;
		this._canLift = canLift;
		base.Alpha = 1f;
		if (Engine.ServerFeatures.AOS)
		{
			base.Tooltip = new ItemTooltip(item);
		}
		base.m_CanDrag = true;
		this.OnAgentUpdated();
	}

	protected internal override void OnDragStart()
	{
		int layer = (int)this._item.Layer;
		if (this._canLift && layer >= 1 && layer <= 24 && layer != 11 && layer != 16 && layer != 21)
		{
			base.m_IsDragging = false;
			Gumps.LastOver = null;
			Gumps.Drag = null;
			Gump gump = this._item.OnBeginDrag();
			if (gump.GetType() == typeof(GDragAmount))
			{
				((GDragAmount)gump).ToDestroy = this;
				return;
			}
			this._item.RestoreInfo = new RestoreInfo(this._item);
			World.Remove(this._item);
			Gumps.Destroy(this);
		}
		else
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
			Point point = base.PointToScreen(new Point(0, 0)) - base.m_Parent.PointToScreen(new Point(0, 0));
			base.m_Parent.m_OffsetX = point.X + base.m_OffsetX;
			base.m_Parent.m_OffsetY = point.Y + base.m_OffsetY;
			base.m_Parent.m_IsDragging = true;
			Gumps.Drag = base.m_Parent;
		}
	}

	protected internal override void Draw(int x, int y)
	{
		base.Draw(x, y);
		if (base.m_Draw)
		{
			this._item.MessageX = x + this.m_xOffset;
			this._item.MessageY = y + this.m_yOffset;
			this._item.BottomY = y + this.m_yOffset;
			this._item.MessageFrame = Renderer.m_ActFrames;
		}
	}

	protected internal override bool HitTest(int X, int Y)
	{
		if (base.m_Invalidated)
		{
			base.Refresh();
		}
		if (base.m_Image == null || !base.m_Draw)
		{
			return false;
		}
		switch (this._item.Layer)
		{
		case Layer.Ring:
		case Layer.Neck:
		case Layer.Bracelet:
		case Layer.Earrings:
		{
			int i = -3;
			int num = 0;
			for (; i <= 3; i++)
			{
				int num2 = -3;
				while (num2 <= 3)
				{
					if ((int)(Math.Sqrt(i * i + num2 * num2) + 0.5) <= 3 && X + i >= 0 && X + i < base.m_Width && Y + num2 >= 0 && Y + num2 < base.m_Height && base.m_Image.HitTest(X + i, Y + num2))
					{
						return true;
					}
					num2++;
					num++;
				}
			}
			return false;
		}
		default:
			return base.m_Image.HitTest(X, Y);
		}
	}

	public void OnAgentUpdated()
	{
		Item item = this._item;
		int iD = item.ID;
		int hue = item.Hue;
		int equipGumpID = Gumps.GetEquipGumpID(iD, this._mobile.BodyGender, ref hue);
		base.GumpID = equipGumpID;
		base.Hue = Hues.GetItemHue(iD, hue);
		int layer = (int)item.Layer;
		base.m_QuickDrag = layer < 1 || layer > 24 || layer == 11 || layer == 16;
	}

	public void OnAgentDeleted()
	{
		Gumps.Destroy(this);
	}
}
