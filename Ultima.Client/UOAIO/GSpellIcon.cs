using System.Windows.Forms;
using UOAIO.Profiles;

namespace UOAIO;

public class GSpellIcon : GClickable
{
	public int m_SpellID;

	public static int GetGumpIDFor(int spellID)
	{
		if (spellID >= 1 && spellID <= 64)
		{
			spellID--;
			return 2240 + spellID;
		}
		if (spellID >= 101 && spellID <= 116)
		{
			spellID -= 101;
			return 20480 + spellID;
		}
		if (spellID >= 201 && spellID <= 210)
		{
			spellID -= 201;
			return 20736 + spellID;
		}
		return 0;
	}

	public override GumpLayout CreateLayout()
	{
		return new SpellIconLayout();
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		if (Gumps.LastOver == this)
		{
			Renderer.SetTexture(null);
			Renderer.PushAlpha(0.1f);
			Renderer.SolidRect(16777215, X - 1, Y - 1, this.Width + 2, this.Height + 2);
			Renderer.SetAlpha(0.6f);
			Renderer.TransparentRect(16777215, X - 1, Y - 1, this.Width + 2, this.Height + 2);
			Renderer.PopAlpha();
		}
	}

	public GSpellIcon(int spellID)
		: base(0, 0, GSpellIcon.GetGumpIDFor(spellID))
	{
		this.m_SpellID = spellID;
		base.m_CanDrag = true;
		base.m_QuickDrag = false;
	}

	protected internal override void OnMouseMove(int X, int Y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None && Engine.amMoving)
		{
			Point point = base.PointToScreen(new Point(X, Y));
			int distance = 0;
			Engine.movingDir = Engine.GetDirection(point.X, point.Y, ref distance);
		}
	}

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None && (Control.ModifierKeys & Keys.Shift) == 0)
		{
			Point point = base.PointToScreen(new Point(x, y));
			int distance = 0;
			Engine.movingDir = Engine.GetDirection(point.X, point.Y, ref distance);
			Engine.amMoving = true;
		}
		else
		{
			base.BringToTop();
		}
	}

	protected internal override void OnDragStart()
	{
		if ((Control.ModifierKeys & Keys.Shift) == 0)
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
		}
	}

	protected override void OnDoubleClicked()
	{
		Spells.GetSpellByID(this.m_SpellID)?.Cast();
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
			{
				base.ManualClose();
				Engine.CancelClick();
			}
			else
			{
				Engine.amMoving = false;
			}
		}
	}
}
