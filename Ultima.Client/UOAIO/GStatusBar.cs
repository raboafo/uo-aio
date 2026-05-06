using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class GStatusBar : GFader, IMobileStatus
{
	private class GMinimizer : GRegion
	{
		private GStatusBar m_Owner;

		public GMinimizer(GStatusBar owner)
			: base(384, 146, 24, 25)
		{
			this.m_Owner = owner;
			base.m_Tooltip = new Tooltip("Minimize");
		}

		protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
		{
			if ((mb & MouseButtons.Left) != MouseButtons.None)
			{
				Mobile mobile = this.m_Owner.m_Mobile;
				this.m_Owner.Close();
				mobile.BigStatus = false;
				mobile.OpenStatus(Drag: false);
			}
		}
	}

	private static int strengthLock;

	private static int dexterityLock;

	private static int inteligenceLock;

	private Mobile m_Mobile;

	private string m_xName;

	private GLabel m_Name;

	private int m_xStr;

	private GLabel m_Str;

	private int m_xDex;

	private GLabel m_Dex;

	private int m_xInt;

	private GLabel m_Int;

	private int m_xArmor;

	private GLabel m_Armor;

	private int m_xGold;

	private GLabel m_Gold;

	private int m_xCold;

	private int m_xFire;

	private int m_xEnergy;

	private int m_xPoison;

	private int m_xLuck;

	private int m_xDamageMin;

	private int m_xDamageMax;

	private int m_xHitChance;

	private GLabel m_Cold;

	private GLabel m_Fire;

	private GLabel m_Energy;

	private GLabel m_Poison;

	private GLabel m_Luck;

	private GLabel m_Damages;

	private GLabel m_HitChance;

	private GLabel m_DefenceChance;

	private GLabel m_LowerManaCost;

	private GLabel m_WeaponDamageInc;

	private GLabel m_SwingSpeedInc;

	private GLabel m_LowerReagentCost;

	private GLabel m_SpellDamageInc;

	private GLabel m_FasterCasting;

	private GLabel m_FasterCastRecovery;

	private int m_xFollCur;

	private int m_xFollMax;

	private GLabel m_Followers;

	private int m_xStatCap;

	private GLabel m_StatCap;

	private GThreeToggle m_Lock;

	public static Stat[] m_Stat;

	private GAttributeCurMax m_Hits;

	private GAttributeCurMax m_Stam;

	private GAttributeCurMax m_Mana;

	private GAttributeCurMax m_Weight;

	public Gump Gump => this;

	public static int StrengthLock
	{
		get
		{
			return GStatusBar.strengthLock;
		}
		set
		{
			GStatusBar.strengthLock = value;
		}
	}

	public static int InteligenceLock
	{
		get
		{
			return GStatusBar.inteligenceLock;
		}
		set
		{
			GStatusBar.inteligenceLock = value;
		}
	}

	public static int DexterityLock
	{
		get
		{
			return GStatusBar.dexterityLock;
		}
		set
		{
			GStatusBar.dexterityLock = value;
		}
	}

	protected internal override void OnDispose()
	{
		if (this.m_Mobile != null)
		{
			this.m_Mobile.StatusBar = null;
			this.m_Mobile = null;
		}
	}

	private string FormatMinMax(int min, int max)
	{
		return min + "/" + max;
	}

	public void OnEnergyChange()
	{
		if (this.m_xEnergy != this.m_Mobile.EnergyResist)
		{
			this.m_xEnergy = this.m_Mobile.EnergyResist;
			this.m_Energy.Text = this.m_xEnergy.ToString();
		}
	}

	public void OnFireChange()
	{
		if (this.m_xFire != this.m_Mobile.FireResist)
		{
			this.m_xFire = this.m_Mobile.FireResist;
			this.m_Fire.Text = this.m_xFire.ToString();
		}
	}

	public void OnColdChange()
	{
		if (this.m_xCold != this.m_Mobile.ColdResist)
		{
			this.m_xCold = this.m_Mobile.ColdResist;
			this.m_Cold.Text = this.m_xCold.ToString();
		}
	}

	public void OnLuckChange()
	{
		if (this.m_xLuck != this.m_Mobile.Luck)
		{
			this.m_xLuck = this.m_Mobile.Luck;
			this.m_Luck.Text = this.m_xLuck.ToString();
		}
	}

	public void OnPoisonChange()
	{
		if (this.m_xPoison != this.m_Mobile.PoisonResist)
		{
			this.m_xPoison = this.m_Mobile.PoisonResist;
			this.m_Poison.Text = this.m_xPoison.ToString();
		}
	}

	public void OnDamageChange()
	{
		if (this.m_xDamageMin != this.m_Mobile.DamageMin || this.m_xDamageMax != this.m_Mobile.DamageMax)
		{
			this.m_xDamageMin = this.m_Mobile.DamageMin;
			this.m_xDamageMax = this.m_Mobile.DamageMax;
			this.m_Damages.Text = $"{this.m_xDamageMin}-{this.m_xDamageMax}";
		}
	}

	public void OnFollCurChange(int current)
	{
		if (this.m_Followers != null && (this.m_xFollCur != current || this.m_xFollMax != this.m_Mobile.FollowersMax))
		{
			this.m_Followers.Text = this.FormatMinMax(current, this.m_Mobile.FollowersMax);
			this.m_xFollCur = current;
			this.m_xFollMax = this.m_Mobile.FollowersMax;
		}
	}

	public void OnFollMaxChange(int maximum)
	{
		if (this.m_Followers != null && (this.m_xFollMax != maximum || this.m_xFollCur != this.m_Mobile.FollowersCur))
		{
			this.m_Followers.Text = this.FormatMinMax(this.m_Mobile.FollowersCur, maximum);
			this.m_xFollCur = this.m_Mobile.FollowersCur;
			this.m_xFollMax = this.m_Mobile.FollowersMax;
		}
	}

	public void OnStatCapChange(int statCap)
	{
		if (this.m_StatCap != null && this.m_xStatCap != statCap)
		{
			this.m_StatCap.Text = statCap.ToString();
			this.m_xStatCap = statCap;
		}
	}

	public void OnNotorietyChange(Notoriety n)
	{
	}

	public void OnWeightChange(int Weight)
	{
		this.m_Weight.SetValues(Weight, this.GetMaxWeight(this.m_Mobile.Strength));
	}

	private int GetMaxWeight(int str)
	{
		return 40 + (int)(3.5 * (double)str);
	}

	public void OnArmorChange(int Armor)
	{
		if (this.m_xArmor != Armor)
		{
			this.m_Armor.Text = Armor.ToString();
			this.m_xArmor = Armor;
		}
	}

	public void OnDexChange(int Dex)
	{
		if (this.m_xDex != Dex)
		{
			this.m_Dex.Text = Dex.ToString();
			this.m_xDex = Dex;
		}
	}

	public void OnFlagsChange(MobileFlags flags)
	{
	}

	public void OnGenderChange(int Gender)
	{
	}

	public void OnGoldChange(int Gold)
	{
		if (this.m_xGold != Gold)
		{
			this.m_Gold.Text = Gold.ToString();
			this.m_xGold = Gold;
		}
	}

	public void OnHitChanceChange(int HitChance)
	{
		if (this.m_xHitChance != HitChance)
		{
			this.m_HitChance.Text = HitChance.ToString();
			this.m_xHitChance = HitChance;
		}
	}

	public void OnHPCurChange(int HPCur)
	{
		this.m_Hits.SetValues(HPCur, this.m_Mobile.MaximumHitPoints);
	}

	public void OnHPMaxChange(int HPMax)
	{
		this.m_Hits.SetValues(this.m_Mobile.CurrentHitPoints, HPMax);
	}

	public void OnIntChange(int Int)
	{
		if (this.m_xInt != Int)
		{
			this.m_Int.Text = Int.ToString();
			this.m_xInt = Int;
		}
	}

	public void OnManaCurChange(int ManaCur)
	{
		this.m_Mana.SetValues(ManaCur, this.m_Mobile.MaximumMana);
	}

	public void OnManaMaxChange(int ManaMax)
	{
		this.m_Mana.SetValues(this.m_Mobile.CurrentMana, ManaMax);
	}

	public void OnNameChange(string Name)
	{
		if (this.m_xName != Name)
		{
			this.m_Name.Text = Name;
			this.m_xName = Name;
			this.m_Name.X = 39 + (352 - (this.m_Name.Image.xMax - this.m_Name.Image.xMin + 1)) / 2;
			this.m_Name.X -= this.m_Name.Image.xMin;
		}
	}

	public void OnStamCurChange(int StamCur)
	{
		this.m_Stam.SetValues(StamCur, this.m_Mobile.MaximumStamina);
	}

	public void OnStamMaxChange(int StamMax)
	{
		this.m_Stam.SetValues(this.m_Mobile.CurrentStamina, StamMax);
	}

	public void OnStrChange(int Str)
	{
		if (this.m_xStr != Str)
		{
			this.m_Str.Text = Str.ToString();
			this.m_xStr = Str;
			this.m_Weight.SetValues(this.m_Mobile.Weight, this.GetMaxWeight(Str));
		}
	}

	private void StrLock_OnStateChange(int state, Gump g)
	{
		if (GStatusBar.m_Stat[0].Lock != (StatLock)state)
		{
			GStatusBar.m_Stat[0].Lock = (StatLock)state;
			Network.Send(new PUpdateStatLock(GStatusBar.m_Stat[0]));
		}
	}

	private void DexLock_OnStateChange(int state, Gump g)
	{
		if (GStatusBar.m_Stat[1].Lock != (StatLock)state)
		{
			GStatusBar.m_Stat[1].Lock = (StatLock)state;
			Network.Send(new PUpdateStatLock(GStatusBar.m_Stat[1]));
		}
	}

	private void IntLock_OnStateChange(int state, Gump g)
	{
		if (GStatusBar.m_Stat[2].Lock != (StatLock)state)
		{
			GStatusBar.m_Stat[2].Lock = (StatLock)state;
			Network.Send(new PUpdateStatLock(GStatusBar.m_Stat[2]));
		}
	}

	public void Close()
	{
		if (Engine.m_Highlight == this.m_Mobile && !this.m_Mobile.Player)
		{
			Engine.m_Highlight = null;
		}
		Gumps.Destroy(this);
	}

	public void OnRefresh()
	{
		Mobile mobile = this.m_Mobile;
		if (mobile == null)
		{
			this.Close();
		}
		this.OnNameChange(mobile.Name);
		this.OnStrChange(mobile.Strength);
		this.OnHPCurChange(mobile.CurrentHitPoints);
		this.OnDexChange(mobile.Dexterity);
		this.OnStamCurChange(mobile.CurrentStamina);
		this.OnIntChange(mobile.Intelligence);
		this.OnManaCurChange(mobile.CurrentMana);
		this.OnArmorChange(mobile.Armor);
		this.OnFireChange();
		this.OnColdChange();
		this.OnPoisonChange();
		this.OnEnergyChange();
		this.OnLuckChange();
		this.OnDamageChange();
		this.OnGoldChange(mobile.Gold);
		this.OnWeightChange(mobile.Weight);
		this.OnNotorietyChange(mobile.Notoriety);
		this.OnStatCapChange(mobile.StatCap);
		this.OnFollCurChange(mobile.FollowersCur);
	}

	protected internal override bool HitTest(int X, int Y)
	{
		base.m_QuickDrag = !TargetManager.IsActive;
		return base.HitTest(X, Y);
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

	public GStatusBar(Mobile m, int X, int Y)
		: base(0.25f, 0.25f, 0.6f, 10860, X, Y)
	{
		this.m_xName = "";
		this.m_Mobile = m;
		base.m_CanDrop = true;
		base.m_QuickDrag = true;
		IFont font = Engine.GetFont(9);
		IHue hue = Hues.Load(1109);
		this.m_Lock = new GThreeToggle(2436, 2438, 2092, (int)GStatusBar.m_Stat[0].Lock, Hues.Default, 28, 77);
		this.m_Lock.OnStateChange = StrLock_OnStateChange;
		base.m_Children.Add(this.m_Lock);
		this.m_Lock = new GThreeToggle(2436, 2438, 2092, (int)GStatusBar.m_Stat[1].Lock, Hues.Default, 28, 105);
		this.m_Lock.OnStateChange = DexLock_OnStateChange;
		base.m_Children.Add(this.m_Lock);
		this.m_Lock = new GThreeToggle(2436, 2438, 2092, (int)GStatusBar.m_Stat[2].Lock, Hues.Default, 28, 133);
		this.m_Lock.OnStateChange = IntLock_OnStateChange;
		base.m_Children.Add(this.m_Lock);
		this.m_Name = new GLabel("", font, hue, 38, 50);
		this.m_Str = new GLabel("0", font, hue, 75, 77);
		this.m_Hits = new GAttributeCurMax(146, 72, 34, 21, this.m_Mobile.CurrentHitPoints, this.m_Mobile.MaximumHitPoints, font, hue);
		this.m_Dex = new GLabel("0", font, hue, 75, 105);
		this.m_Stam = new GAttributeCurMax(146, 100, 102, 63, this.m_Mobile.CurrentStamina, this.m_Mobile.MaximumStamina, font, hue);
		this.m_Int = new GLabel("0", font, hue, 75, 133);
		this.m_Mana = new GAttributeCurMax(146, 128, 34, 21, this.m_Mobile.CurrentMana, this.m_Mobile.MaximumMana, font, hue);
		this.m_Armor = new GLabel("0", font, hue, 475, 74);
		this.m_Fire = new GLabel("0", font, hue, 475, 91);
		this.m_Cold = new GLabel("0", font, hue, 475, 106);
		this.m_Poison = new GLabel("0", font, hue, 475, 119);
		this.m_Energy = new GLabel("0", font, hue, 475, 133);
		this.m_Luck = new GLabel("0", font, hue, 240, 105);
		this.m_Damages = new GLabel("0-0", font, hue, 310, 77);
		this.m_Gold = new GLabel("0", font, hue, 475, 161);
		this.m_HitChance = new GLabel("0", font, hue, 75, 161);
		this.m_DefenceChance = new GLabel("0/0", font, hue, 146, 161);
		this.m_Weight = new GAttributeCurMax(235, 128, 34, 21, this.m_Mobile.Weight, this.GetMaxWeight(this.m_Mobile.Strength), font, hue);
		this.m_LowerManaCost = new GLabel("0", font, hue, 240, 161);
		this.m_WeaponDamageInc = new GLabel("0", font, hue, 310, 105);
		this.m_SwingSpeedInc = new GLabel("0", font, hue, 310, 161);
		this.m_LowerReagentCost = new GLabel("0", font, hue, 400, 77);
		this.m_SpellDamageInc = new GLabel("0", font, hue, 400, 105);
		this.m_FasterCasting = new GLabel("0", font, hue, 400, 133);
		this.m_FasterCastRecovery = new GLabel("0", font, hue, 400, 161);
		this.m_StatCap = new GLabel("0", font, hue, 240, 77);
		this.m_Followers = new GLabel("0/0", font, hue, 310, 135);
		this.m_Name.X = 39 + (352 - (this.m_Name.Image.xMax - this.m_Name.Image.xMin + 1)) / 2;
		this.m_Name.X -= this.m_Name.Image.xMin;
		base.m_Children.Add(this.m_Name);
		base.m_Children.Add(this.m_Str);
		base.m_Children.Add(this.m_Hits);
		base.m_Children.Add(this.m_Dex);
		base.m_Children.Add(this.m_Stam);
		base.m_Children.Add(this.m_Int);
		base.m_Children.Add(this.m_Mana);
		base.m_Children.Add(this.m_Armor);
		base.m_Children.Add(this.m_Fire);
		base.m_Children.Add(this.m_Cold);
		base.m_Children.Add(this.m_Poison);
		base.m_Children.Add(this.m_Energy);
		base.m_Children.Add(this.m_Luck);
		base.m_Children.Add(this.m_Damages);
		base.m_Children.Add(this.m_Gold);
		base.m_Children.Add(this.m_HitChance);
		base.m_Children.Add(this.m_Weight);
		base.m_Children.Add(this.m_DefenceChance);
		base.m_Children.Add(this.m_LowerManaCost);
		base.m_Children.Add(this.m_WeaponDamageInc);
		base.m_Children.Add(this.m_SwingSpeedInc);
		base.m_Children.Add(this.m_LowerReagentCost);
		base.m_Children.Add(this.m_SpellDamageInc);
		base.m_Children.Add(this.m_FasterCasting);
		base.m_Children.Add(this.m_FasterCastRecovery);
		base.m_Children.Add(this.m_StatCap);
		base.m_Children.Add(this.m_Followers);
		base.m_Children.Add(new GMinimizer(this));
		this.AddTooltip(55, 70, 64, 26, 1061146);
		this.AddTooltip(55, 98, 64, 26, 1061147);
		this.AddTooltip(55, 126, 64, 26, 1061148);
		this.AddTooltip(55, 154, 64, 26, 1075616);
		this.AddTooltip(121, 70, 63, 26, 1061149);
		this.AddTooltip(121, 98, 63, 26, 1061150);
		this.AddTooltip(121, 126, 63, 26, 1061151);
		this.AddTooltip(121, 154, 63, 26, 1075620);
		this.AddTooltip(186, 70, 69, 26, 1061152);
		this.AddTooltip(186, 98, 69, 26, 1061153);
		this.AddTooltip(186, 126, 69, 26, 1061154);
		this.AddTooltip(186, 154, 69, 26, 1075621);
		this.AddTooltip(257, 70, 72, 26, 1061155);
		this.AddTooltip(257, 98, 72, 26, 1075619);
		this.AddTooltip(257, 126, 72, 26, 1061157);
		this.AddTooltip(257, 154, 72, 26, 1061156);
		this.AddTooltip(380, 70, 72, 26, 1075625);
		this.AddTooltip(380, 154, 72, 26, 1075628);
		this.AddTooltip(380, 154, 72, 26, 1075617);
		this.AddTooltip(380, 154, 72, 26, 1075618);
		this.AddTooltip(450, 74, 46, 14, 1061158);
		this.AddTooltip(450, 91, 46, 14, 1061159);
		this.AddTooltip(450, 106, 46, 13, 1061160);
		this.AddTooltip(450, 120, 46, 11, 1061161);
		this.AddTooltip(450, 133, 46, 16, 1061162);
		this.AddTooltip(450, 154, 46, 26, 1061156);
		this.OnRefresh();
		if (Engine.ServerFeatures.AOS)
		{
			base.Tooltip = new MobileTooltip(m);
		}
	}

	private void AddTooltip(int x, int y, int w, int h, int num)
	{
		GHotspot gHotspot = new GHotspot(x, y, w, h, this);
		gHotspot.m_CanDrag = true;
		gHotspot.m_QuickDrag = false;
		gHotspot.Tooltip = new Tooltip(Localization.GetString(num));
		base.m_Children.Add(gHotspot);
	}

	protected internal override void OnDoubleClick(int X, int Y)
	{
		if (!TargetManager.IsActive)
		{
			this.m_Mobile.OnDoubleClick();
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

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if ((mb & MouseButtons.Right) != MouseButtons.None)
		{
			Mobile mobile = this.m_Mobile;
			this.Close();
			mobile.BigStatus = false;
			Engine.CancelClick();
		}
		if ((mb & MouseButtons.Left) != MouseButtons.None)
		{
			if (TargetManager.IsActive)
			{
				this.m_Mobile.OnTarget();
				Engine.CancelClick();
			}
			else
			{
				this.m_Mobile.Look();
			}
		}
	}

	static GStatusBar()
	{
		GStatusBar.m_Stat = new Stat[3];
	}
}
