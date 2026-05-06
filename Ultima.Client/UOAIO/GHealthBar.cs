using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class GHealthBar : GDragable, IMobileStatus
{
	private Mobile m_Mobile;

	private bool m_Player;

	private string m_xName = "";

	private GLabel m_Name;

	private int m_xHPCur;

	private int m_xHPMax;

	private GImageClip m_HP;

	private int m_xStamCur;

	private int m_xStamMax;

	private GImageClip m_Stam;

	private int m_xManaCur;

	private int m_xManaMax;

	private GImageClip m_Mana;

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

	public void OnWeightChange(int Weight)
	{
	}

	public void OnArmorChange(int Armor)
	{
	}

	public void OnDexChange(int Dex)
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

	public void OnFlagsChange(MobileFlags Flags)
	{
		base.GumpID = (Flags[MobileFlag.Warmode] ? 2055 : 2051);
		this.m_HP.GumpID = this.GetHealthGumpID(Flags);
	}

	public void OnNotorietyChange(Notoriety n)
	{
		if (!this.m_Player)
		{
			base.Hue = Hues.GetNotoriety(n);
		}
	}

	public void OnGenderChange(int Gender)
	{
	}

	public void OnGoldChange(int Gold)
	{
	}

	public void OnHPCurChange(int HPCur)
	{
		if (this.m_xHPCur != HPCur || this.m_xHPMax != this.m_Mobile.MaximumHitPoints)
		{
			this.m_HP.Resize(HPCur, this.m_Mobile.MaximumHitPoints);
			this.m_xHPCur = HPCur;
			this.m_xHPMax = this.m_Mobile.MaximumHitPoints;
		}
	}

	public void OnHPMaxChange(int HPMax)
	{
		if (this.m_xHPMax != HPMax || this.m_xHPCur != this.m_Mobile.CurrentHitPoints)
		{
			this.m_HP.Resize(this.m_Mobile.CurrentHitPoints, HPMax);
			this.m_xHPCur = this.m_Mobile.CurrentHitPoints;
			this.m_xHPMax = HPMax;
		}
	}

	public void OnIntChange(int Int)
	{
	}

	public void OnManaCurChange(int ManaCur)
	{
		if (this.m_Player && (this.m_xManaCur != ManaCur || this.m_xManaMax != this.m_Mobile.MaximumMana))
		{
			this.m_Mana.Resize(ManaCur, this.m_Mobile.MaximumMana);
			this.m_xManaCur = ManaCur;
			this.m_xManaMax = this.m_Mobile.MaximumMana;
		}
	}

	public void OnManaMaxChange(int ManaMax)
	{
		if (this.m_Player && (this.m_xManaMax != ManaMax || this.m_xManaCur != this.m_Mobile.CurrentMana))
		{
			this.m_Mana.Resize(this.m_Mobile.CurrentMana, ManaMax);
			this.m_xManaCur = this.m_Mobile.CurrentMana;
			this.m_xManaMax = ManaMax;
		}
	}

	public void OnNameChange(string Name)
	{
		if (!this.m_Player && this.m_xName != Name)
		{
			this.m_Name.Text = Name;
			this.m_Name.Y = 11 + (24 - this.m_Name.Height) / 2;
			this.m_Name.Scissor(0, 0, 122, this.m_Name.Height);
			this.m_xName = Name;
		}
	}

	public void OnStamCurChange(int StamCur)
	{
		if ((this.m_Player && this.m_xStamCur != StamCur) || this.m_xStamMax != this.m_Mobile.MaximumStamina)
		{
			this.m_Stam.Resize(StamCur, this.m_Mobile.MaximumStamina);
			this.m_xStamCur = StamCur;
			this.m_xStamMax = this.m_Mobile.MaximumStamina;
		}
	}

	public void OnStamMaxChange(int StamMax)
	{
		if ((this.m_Player && this.m_xStamMax != StamMax) || this.m_xStamCur != this.m_Mobile.CurrentStamina)
		{
			this.m_Stam.Resize(this.m_Mobile.CurrentStamina, StamMax);
			this.m_xStamCur = this.m_Mobile.CurrentStamina;
			this.m_xStamMax = StamMax;
		}
	}

	public void OnStrChange(int Str)
	{
	}

	public void Close()
	{
		if (Engine.m_Highlight == this.m_Mobile && !this.m_Mobile.Player)
		{
			Engine.m_Highlight = null;
		}
		Gumps.Destroy(this);
		if (this.m_Mobile != null)
		{
			this.m_Mobile.StatusBar = null;
		}
	}

	public void OnRefresh()
	{
		Mobile mobile = this.m_Mobile;
		if (mobile == null)
		{
			this.Close();
		}
		this.OnHPCurChange(mobile.CurrentHitPoints);
		this.OnFlagsChange(mobile.Flags);
		if (this.m_Player)
		{
			this.OnStamCurChange(mobile.CurrentStamina);
			this.OnManaCurChange(mobile.CurrentMana);
		}
		else
		{
			this.OnNotorietyChange(mobile.Notoriety);
			this.OnNameChange(mobile.Name);
		}
	}

	protected internal override void OnDragDrop(Gump g)
	{
		if (g is GDraggedItem)
		{
			Item item = ((GDraggedItem)g).Item;
			if (item != null && this.m_Mobile != null)
			{
				Network.Send(new PDropItem(item.Serial, -1, -1, 0, this.m_Mobile.Serial));
				Gumps.Destroy(g);
			}
		}
	}

	private int GetHealthGumpID(MobileFlags flags)
	{
		if (this.m_Mobile.IsPoisoned)
		{
			return 2056;
		}
		if (this.m_Mobile.IsInvulnerable)
		{
			return 2057;
		}
		return 2054;
	}

	public GHealthBar(Mobile m, int X, int Y)
		: base((!m.Player) ? 2052 : (m.Flags[MobileFlag.Warmode] ? 2055 : 2051), X, Y)
	{
		this.m_Mobile = m;
		base.m_CanDrop = true;
		base.m_QuickDrag = false;
		this.m_Player = m.Player;
		if (!this.m_Player)
		{
			base.Hue = Hues.GetNotoriety(m.Notoriety);
		}
		if (Engine.ServerFeatures.AOS)
		{
			base.Tooltip = new MobileTooltip(m);
		}
		if (this.m_Player)
		{
			this.m_HP = new GImageClip(this.GetHealthGumpID(m.Flags), 34, 12, m.CurrentHitPoints, m.MaximumHitPoints);
			this.m_Mana = new GImageClip(2054, 34, 25, m.CurrentMana, m.MaximumMana);
			this.m_Stam = new GImageClip(2054, 34, 38, m.CurrentStamina, m.MaximumStamina);
			base.m_Children.Add(new GImage(2053, 34, 12));
			base.m_Children.Add(new GImage(2053, 34, 25));
			base.m_Children.Add(new GImage(2053, 34, 38));
			base.m_Children.Add(this.m_HP);
			base.m_Children.Add(this.m_Mana);
			base.m_Children.Add(this.m_Stam);
			this.m_xHPCur = m.CurrentHitPoints;
			this.m_xHPMax = m.MaximumHitPoints;
			this.m_xStamCur = m.CurrentStamina;
			this.m_xStamMax = m.MaximumStamina;
			this.m_xManaCur = m.CurrentMana;
			this.m_xManaMax = m.MaximumMana;
		}
		else
		{
			this.m_Name = new GLabel(m.Name, Engine.GetFont(1), Hues.Load(1109), 16, 0);
			this.m_Name.Y = 11 + (24 - this.m_Name.Height) / 2;
			this.m_Name.Scissor(0, 0, 122, this.m_Name.Height);
			this.m_HP = new GImageClip(this.GetHealthGumpID(m.Flags), 34, 38, m.CurrentHitPoints, m.MaximumHitPoints);
			base.m_Children.Add(new GImage(2053, 34, 38));
			base.m_Children.Add(this.m_Name);
			base.m_Children.Add(this.m_HP);
			this.m_xName = m.Name;
			this.m_xHPCur = m.CurrentHitPoints;
			this.m_xHPMax = m.MaximumHitPoints;
		}
	}

	protected internal override void OnDoubleClick(int X, int Y)
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

	protected internal override void OnMouseDown(int x, int y, MouseButtons mb)
	{
		base.BringToTop();
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
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			this.Close();
			Engine.CancelClick();
		}
		else if (TargetManager.IsActive && (mb & MouseButtons.Left) != MouseButtons.None)
		{
			this.m_Mobile.OnTarget();
			Engine.CancelClick();
		}
	}
}
