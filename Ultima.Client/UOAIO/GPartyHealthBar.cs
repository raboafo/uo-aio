using System;
using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class GPartyHealthBar : Gump, IMobileStatus
{
	public Mobile m_Mobile;

	private string m_xName;

	private GLabel m_Name;

	private int m_Width;

	private int m_Height;

	public override int Width => this.m_Width;

	public override int Height => this.m_Height;

	public Gump Gump => this;

	public void OnEnergyChange()
	{
	}

	public void OnFireChange()
	{
	}

	public void OnColdChange()
	{
	}

	public void OnLuckChange()
	{
	}

	public void OnPoisonChange()
	{
	}

	public void OnDamageChange()
	{
	}

	public void OnWeightChange(int weight)
	{
	}

	public void OnArmorChange(int armor)
	{
	}

	public void OnFollCurChange(int current)
	{
	}

	public void OnFollMaxChange(int maximum)
	{
	}

	public void OnStatCapChange(int statCap)
	{
	}

	public void OnNotorietyChange(Notoriety n)
	{
		if (this.m_Mobile.Visible)
		{
			this.m_Name.Hue = Hues.GetNotoriety(n);
		}
		else
		{
			this.m_Name.Hue = Hues.Grayscale;
		}
	}

	public void OnGenderChange(int gender)
	{
	}

	public void OnGoldChange(int gold)
	{
	}

	public void OnStrChange(int str)
	{
	}

	public void OnHPCurChange(int cur)
	{
	}

	public void OnHPMaxChange(int max)
	{
	}

	public void OnDexChange(int dex)
	{
	}

	public void OnStamCurChange(int cur)
	{
	}

	public void OnStamMaxChange(int max)
	{
	}

	public void OnIntChange(int intel)
	{
	}

	public void OnManaCurChange(int cur)
	{
	}

	public void OnManaMaxChange(int max)
	{
	}

	public void OnFlagsChange(MobileFlags flags)
	{
		if (this.m_Mobile.Visible)
		{
			this.m_Name.Hue = Hues.GetNotoriety(this.m_Mobile.Notoriety);
		}
		else
		{
			this.m_Name.Hue = Hues.Grayscale;
		}
	}

	public void OnNameChange(string name)
	{
		if (this.m_xName != name)
		{
			this.SetName(name);
		}
	}

	protected internal override void OnDispose()
	{
		base.OnDispose();
		if (Engine.m_Highlight == this.m_Mobile && !this.m_Mobile.Player)
		{
			Engine.m_Highlight = null;
		}
		this.m_Mobile.StatusBar = null;
	}

	public void Close()
	{
		Gumps.Destroy(this);
	}

	public void OnRefresh()
	{
		this.OnNameChange(this.m_Mobile.Name);
		this.OnNotorietyChange(this.m_Mobile.Notoriety);
		bool flag = this.m_Mobile.MaximumStamina > 0;
		bool flag2 = this.m_Mobile.MaximumMana > 0;
		this.m_Height = 26 + (flag ? 6 : 0) + (flag2 ? 6 : 0);
	}

	protected internal override void OnDragStart()
	{
		if ((Control.ModifierKeys & Keys.Shift) == 0)
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
		}
	}

	protected internal override void OnDragDrop(Gump g)
	{
		if (g is GDraggedItem)
		{
			Item item = ((GDraggedItem)g).Item;
			if (item != null)
			{
				Network.Send(new PDropItem(item.Serial, -1, -1, 0, this.m_Mobile.Serial));
				Gumps.Destroy(g);
			}
		}
	}

	protected internal override void OnDoubleClick(int x, int y)
	{
		if (!TargetManager.IsActive)
		{
			if (this.m_Mobile.Player)
			{
				this.Close();
				this.m_Mobile.BigStatus = true;
				this.m_Mobile.OpenStatus(Drag: false);
			}
			else
			{
				this.m_Mobile.OnDoubleClick();
			}
		}
	}

	protected internal override void OnMouseEnter(int x, int y, MouseButtons mb)
	{
		if (!this.m_Mobile.Player)
		{
			Engine.m_Highlight = this.m_Mobile;
		}
	}

	protected internal override void OnMouseLeave()
	{
		if (Engine.m_Highlight == this.m_Mobile && !this.m_Mobile.Player)
		{
			Engine.m_Highlight = null;
		}
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

	protected internal override void OnSingleClick(int x, int y)
	{
		if (!TargetManager.IsActive)
		{
			this.m_Mobile.OnSingleClick();
		}
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) > MouseButtons.None)
		{
			if ((Control.ModifierKeys & Keys.Shift) > Keys.None)
			{
				this.Close();
				Engine.CancelClick();
			}
			else
			{
				Engine.amMoving = false;
			}
		}
		else if ((this.m_Mobile == World.Player || this.m_Mobile.IsInParty || this.m_Mobile.IsFriend || WorldEx.IsGuildMember(this.m_Mobile)) && (mb & MouseButtons.Left) != MouseButtons.None && (Control.ModifierKeys & Keys.Control) > Keys.None)
		{
			GRadar.m_FocusMob = this.m_Mobile;
		}
		else if (TargetManager.IsActive && (mb & MouseButtons.Left) > MouseButtons.None)
		{
			this.m_Mobile.OnTarget();
			Engine.CancelClick();
		}
	}

	public GPartyHealthBar(Mobile m, int x, int y)
		: base(x, y)
	{
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		base.m_CanDrop = true;
		this.m_Mobile = m;
		this.m_Name = new GLabel("", Engine.GetUniFont(0), m.Visible ? Hues.GetNotoriety(m.Notoriety) : Hues.Grayscale, 4, 4);
		this.SetName(m.Name);
		base.m_Children.Add(this.m_Name);
	}

	protected internal override bool HitTest(int x, int y)
	{
		return true;
	}

	protected internal override void Render(int x, int y)
	{
		base.Render(x, y);
	}

	protected internal override void Draw(int x, int y)
	{
		Renderer.SetTexture(null);
		Renderer.PushAlpha(0.4f);
		Renderer.SolidRect(0, x + 1, y + 1, this.m_Width - 2, this.m_Height - 2);
		Renderer.PopAlpha();
		Renderer.TransparentRect(0, x, y, this.m_Width, this.m_Height);
		bool flag = this.m_Mobile.MaximumStamina > 0;
		bool flag2 = this.m_Mobile.MaximumMana > 0;
		int num = 6 + (flag ? 6 : 0) + (flag2 ? 6 : 0);
		y += this.m_Height;
		y -= num + 1;
		Renderer.SolidRect(0, x, y, this.m_Width, num + 1);
		x++;
		y++;
		int num2 = this.m_Width - 2;
		if (this.m_Mobile.Ghost || this.m_Mobile.IsDeadPet)
		{
			Renderer.GradientRect(12632256, 6316128, x, y, num2, 5);
			if (flag)
			{
				y += 6;
				Renderer.GradientRect(12632256, 6316128, x, y, num2, 5);
			}
			if (flag2)
			{
				y += 6;
				Renderer.GradientRect(12632256, 6316128, x, y, num2, 5);
			}
			return;
		}
		MobileFlags flags = this.m_Mobile.Flags;
		int color;
		int color2;
		if (this.m_Mobile.IsPoisoned)
		{
			color = 65280;
			color2 = 32768;
		}
		else if (flags[MobileFlag.YellowHits])
		{
			color = 16760832;
			color2 = 8413184;
		}
		else
		{
			color = 2146559;
			color2 = 1073280;
		}
		int num3 = this.m_Mobile.CurrentHitPoints * num2 / Math.Max(1, this.m_Mobile.MaximumHitPoints);
		if (num3 > num2)
		{
			num3 = num2;
		}
		else if (num3 < 0)
		{
			num3 = 0;
		}
		Renderer.GradientRect(color, color2, x, y, num3, 5);
		Renderer.GradientRect(16711680, 8388608, x + num3, y, num2 - num3, 5);
		if (flag)
		{
			y += 6;
			num3 = this.m_Mobile.CurrentMana * num2 / Math.Max(1, this.m_Mobile.MaximumMana);
			if (num3 > num2)
			{
				num3 = num2;
			}
			else if (num3 < 0)
			{
				num3 = 0;
			}
			Renderer.GradientRect(2146559, 1073280, x, y, num3, 5);
			Renderer.GradientRect(16711680, 8388608, x + num3, y, num2 - num3, 5);
		}
		if (flag2)
		{
			y += 6;
			num3 = this.m_Mobile.CurrentStamina * num2 / Math.Max(1, this.m_Mobile.MaximumStamina);
			if (num3 > num2)
			{
				num3 = num2;
			}
			else if (num3 < 0)
			{
				num3 = 0;
			}
			Renderer.GradientRect(2146559, 1073280, x, y, num3, 5);
			Renderer.GradientRect(16711680, 8388608, x + num3, y, num2 - num3, 5);
		}
	}

	private void SetName(string name)
	{
		this.m_xName = name;
		if (this.m_Name.Font.GetStringWidth(name) > 70)
		{
			while (name.Length > 0 && this.m_Name.Font.GetStringWidth(name + "...") > 70)
			{
				name = name.Substring(0, name.Length - 1);
			}
			name += "...";
		}
		this.m_Name.Text = name;
		int num = this.m_Name.Image.xMax - this.m_Name.Image.xMin + 1 + 6;
		if (num < 80)
		{
			num = 80;
		}
		this.m_Name.Y = 3 - this.m_Name.Image.yMin;
		this.m_Name.X = (num - (this.m_Name.Image.xMax - this.m_Name.Image.xMin + 1)) / 2 - this.m_Name.Image.xMin;
		this.m_Width = num;
		bool flag = this.m_Mobile.MaximumStamina > 0;
		bool flag2 = this.m_Mobile.MaximumMana > 0;
		this.m_Height = 26 + (flag ? 6 : 0) + (flag2 ? 6 : 0);
		base.m_DragClipX = this.m_Width - 1;
		base.m_DragClipY = this.m_Height - 1;
	}
}
