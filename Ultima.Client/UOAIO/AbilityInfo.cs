using System.Collections;

namespace UOAIO;

public class AbilityInfo
{
	private static int[] HatchetID;

	private static int[] LongSwordID;

	private static int[] BroadswordID;

	private static int[] KatanaID;

	private static int[] BladedStaffID;

	private static int[] HammerPickID;

	private static int[] WarAxeID;

	private static int[] KryssID;

	private static int[] SpearID;

	private static int[] CompositeBowID;

	private static int[] CleaverID;

	private static int[] LargeBattleAxeID;

	private static int[] BattleAxeID;

	private static int[] ExecAxeID;

	private static int[] CutlassID;

	private static int[] ScytheID;

	private static int[] WarMaceID;

	private static int[] PitchforkID;

	private static int[] WarForkID;

	private static int[] HalberdID;

	private static int[] MaulID;

	private static int[] MaceID;

	private static int[] GnarledStaffID;

	private static int[] QuarterStaffID;

	private static int[] LanceID;

	private static int[] CrossbowID;

	private static int[] VikingSwordID;

	private static int[] AxeID;

	private static int[] ShepherdsCrookID;

	private static int[] SmithsHammerID;

	private static int[] WarHammerID;

	private static int[] ScepterID;

	private static int[] SledgeHammerID;

	private static int[] ButcherKnifeID;

	private static int[] PickaxeID;

	private static int[] SkinningKnifeID;

	private static int[] WandID;

	private static int[] BardicheID;

	private static int[] ClubID;

	private static int[] ScimitarID;

	private static int[] HeavyCrossbowID;

	private static int[] TwoHandedAxeID;

	private static int[] DoubleAxeID;

	private static int[] CrescentBladeID;

	private static int[] DoubleBladedStaffID;

	private static int[] RepeatingCrossbowID;

	private static int[] DaggerID;

	private static int[] PikeID;

	private static int[] BoneHarvesterID;

	private static int[] ShortSpearID;

	private static int[] BowID;

	private static int[] BlackStaffID;

	private static AbilityInfo[] m_Abilities;

	private static Hashtable m_Table;

	private static AbilityInfo m_Active;

	private int m_Index;

	private int m_Tooltip;

	private int m_Icon;

	private int m_Name;

	private int[] m_Weapons;

	private GTextButton m_NameLabel;

	public static AbilityInfo Active
	{
		get
		{
			return AbilityInfo.m_Active;
		}
		set
		{
			AbilityInfo active = AbilityInfo.m_Active;
			if (value != active)
			{
				AbilityInfo.m_Active = value;
				if (value == null)
				{
					Network.Send(new PSetActiveAbility(0));
				}
				else
				{
					Network.Send(new PSetActiveAbility(value.Index + 1));
				}
				GCombatGump.Update();
			}
		}
	}

	public static AbilityInfo Primary => AbilityInfo.GetAbilityFor(World.Player, primary: true);

	public static AbilityInfo Secondary => AbilityInfo.GetAbilityFor(World.Player, primary: false);

	public static AbilityInfo[] Abilities => AbilityInfo.m_Abilities;

	public GTextButton NameLabel
	{
		get
		{
			return this.m_NameLabel;
		}
		set
		{
			this.m_NameLabel = value;
		}
	}

	public int Icon => this.m_Icon;

	public int Name => this.m_Name;

	public int Index => this.m_Index;

	public int Tooltip => this.m_Tooltip;

	public static void ClearActive()
	{
		AbilityInfo.m_Active = null;
		GCombatGump.Update();
	}

	public static AbilityInfo GetAbilityFor(Mobile m, bool primary)
	{
		if (m == null)
		{
			return AbilityInfo.m_Abilities[primary ? 4 : 10];
		}
		Item item = m.FindEquip(Layer.TwoHanded);
		if (item != null)
		{
			int iD = item.ID;
			ArrayList arrayList = (ArrayList)AbilityInfo.m_Table[iD];
			if (arrayList != null && arrayList.Count > 0)
			{
				return (AbilityInfo)arrayList[(!primary) ? (arrayList.Count - 1) : 0];
			}
		}
		item = m.FindEquip(Layer.OneHanded);
		if (item != null)
		{
			int iD2 = item.ID;
			ArrayList arrayList = (ArrayList)AbilityInfo.m_Table[iD2];
			if (arrayList != null && arrayList.Count > 0)
			{
				return (AbilityInfo)arrayList[(!primary) ? (arrayList.Count - 1) : 0];
			}
		}
		return AbilityInfo.m_Abilities[primary ? 4 : 10];
	}

	public AbilityInfo(int index, params int[][] weapons)
	{
		if (AbilityInfo.m_Table == null)
		{
			AbilityInfo.m_Table = new Hashtable();
		}
		this.m_Index = index;
		this.m_Name = 1028838 + index;
		this.m_Tooltip = 1061693 + index;
		this.m_Icon = 20992 + index;
		int num = 0;
		for (int i = 0; i < weapons.Length; i++)
		{
			num += weapons[i].Length;
		}
		this.m_Weapons = new int[num];
		int j = 0;
		int num2 = 0;
		for (; j < weapons.Length; j++)
		{
			int num3 = 0;
			while (num3 < weapons[j].Length)
			{
				this.m_Weapons[num2] = weapons[j][num3];
				if (this.m_Weapons[num2] == 3921)
				{
					int num4 = 0;
					num4++;
				}
				ArrayList arrayList = (ArrayList)AbilityInfo.m_Table[this.m_Weapons[num2]];
				if (arrayList == null)
				{
					arrayList = (ArrayList)(AbilityInfo.m_Table[this.m_Weapons[num2]] = new ArrayList());
				}
				arrayList.Add(this);
				num3++;
				num2++;
			}
		}
	}

	static AbilityInfo()
	{
		AbilityInfo.HatchetID = new int[2] { 3907, 3908 };
		AbilityInfo.LongSwordID = new int[2] { 3936, 3937 };
		AbilityInfo.BroadswordID = new int[2] { 3934, 3935 };
		AbilityInfo.KatanaID = new int[2] { 5118, 5119 };
		AbilityInfo.BladedStaffID = new int[2] { 9917, 9927 };
		AbilityInfo.HammerPickID = new int[2] { 5180, 5181 };
		AbilityInfo.WarAxeID = new int[2] { 5039, 5040 };
		AbilityInfo.KryssID = new int[2] { 5120, 5121 };
		AbilityInfo.SpearID = new int[2] { 3938, 3939 };
		AbilityInfo.CompositeBowID = new int[2] { 9922, 9932 };
		AbilityInfo.CleaverID = new int[2] { 3778, 3779 };
		AbilityInfo.LargeBattleAxeID = new int[2] { 5114, 5115 };
		AbilityInfo.BattleAxeID = new int[2] { 3911, 3912 };
		AbilityInfo.ExecAxeID = new int[2] { 3909, 3910 };
		AbilityInfo.CutlassID = new int[2] { 5184, 5185 };
		AbilityInfo.ScytheID = new int[2] { 9914, 9924 };
		AbilityInfo.WarMaceID = new int[2] { 5126, 5127 };
		AbilityInfo.PitchforkID = new int[2] { 3719, 3720 };
		AbilityInfo.WarForkID = new int[2] { 5124, 5125 };
		AbilityInfo.HalberdID = new int[2] { 5182, 5183 };
		AbilityInfo.MaulID = new int[2] { 5178, 5179 };
		AbilityInfo.MaceID = new int[2] { 3932, 1117 };
		AbilityInfo.GnarledStaffID = new int[2] { 5112, 5113 };
		AbilityInfo.QuarterStaffID = new int[2] { 3721, 3722 };
		AbilityInfo.LanceID = new int[2] { 9920, 9930 };
		AbilityInfo.CrossbowID = new int[2] { 3919, 3920 };
		AbilityInfo.VikingSwordID = new int[2] { 5049, 5050 };
		AbilityInfo.AxeID = new int[2] { 3913, 3914 };
		AbilityInfo.ShepherdsCrookID = new int[2] { 3713, 3714 };
		AbilityInfo.SmithsHammerID = new int[2] { 5100, 5092 };
		AbilityInfo.WarHammerID = new int[2] { 5176, 5177 };
		AbilityInfo.ScepterID = new int[2] { 9916, 9926 };
		AbilityInfo.SledgeHammerID = new int[2] { 4020, 4021 };
		AbilityInfo.ButcherKnifeID = new int[2] { 5110, 5111 };
		AbilityInfo.PickaxeID = new int[2] { 3717, 3718 };
		AbilityInfo.SkinningKnifeID = new int[2] { 3780, 3781 };
		AbilityInfo.WandID = new int[4] { 3570, 3571, 3572, 3573 };
		AbilityInfo.BardicheID = new int[2] { 3917, 3918 };
		AbilityInfo.ClubID = new int[2] { 5043, 5044 };
		AbilityInfo.ScimitarID = new int[2] { 5045, 5046 };
		AbilityInfo.HeavyCrossbowID = new int[2] { 5116, 5117 };
		AbilityInfo.TwoHandedAxeID = new int[2] { 5186, 5187 };
		AbilityInfo.DoubleAxeID = new int[2] { 3915, 3916 };
		AbilityInfo.CrescentBladeID = new int[2] { 9921, 9922 };
		AbilityInfo.DoubleBladedStaffID = new int[2] { 9919, 9929 };
		AbilityInfo.RepeatingCrossbowID = new int[2] { 9923, 9933 };
		AbilityInfo.DaggerID = new int[2] { 3921, 3922 };
		AbilityInfo.PikeID = new int[2] { 9918, 9928 };
		AbilityInfo.BoneHarvesterID = new int[2] { 9915, 9925 };
		AbilityInfo.ShortSpearID = new int[2] { 5122, 5123 };
		AbilityInfo.BowID = new int[2] { 5041, 5042 };
		AbilityInfo.BlackStaffID = new int[2] { 3568, 3569 };
		AbilityInfo.m_Abilities = new AbilityInfo[13]
		{
			new AbilityInfo(0, AbilityInfo.HatchetID, AbilityInfo.LongSwordID, AbilityInfo.BroadswordID, AbilityInfo.KatanaID, AbilityInfo.BladedStaffID, AbilityInfo.HammerPickID, AbilityInfo.WarAxeID, AbilityInfo.KryssID, AbilityInfo.SpearID, AbilityInfo.CompositeBowID),
			new AbilityInfo(1, AbilityInfo.CleaverID, AbilityInfo.LargeBattleAxeID, AbilityInfo.BattleAxeID, AbilityInfo.ExecAxeID, AbilityInfo.CutlassID, AbilityInfo.ScytheID, AbilityInfo.WarMaceID, AbilityInfo.WarAxeID, AbilityInfo.PitchforkID, AbilityInfo.WarForkID),
			new AbilityInfo(2, AbilityInfo.LongSwordID, AbilityInfo.BattleAxeID, AbilityInfo.HalberdID, AbilityInfo.MaulID, AbilityInfo.MaceID, AbilityInfo.GnarledStaffID, AbilityInfo.QuarterStaffID, AbilityInfo.LanceID, AbilityInfo.CrossbowID),
			new AbilityInfo(3, AbilityInfo.VikingSwordID, AbilityInfo.AxeID, AbilityInfo.BroadswordID, AbilityInfo.ShepherdsCrookID, AbilityInfo.SmithsHammerID, AbilityInfo.MaulID, AbilityInfo.WarMaceID, AbilityInfo.WarHammerID, AbilityInfo.ScepterID, AbilityInfo.SledgeHammerID),
			new AbilityInfo(4, AbilityInfo.ButcherKnifeID, AbilityInfo.PickaxeID, AbilityInfo.SkinningKnifeID, AbilityInfo.HatchetID, AbilityInfo.WandID, AbilityInfo.ShepherdsCrookID, AbilityInfo.MaceID, AbilityInfo.WarForkID),
			new AbilityInfo(5, AbilityInfo.BardicheID, AbilityInfo.AxeID, AbilityInfo.BladedStaffID, AbilityInfo.WandID, AbilityInfo.ClubID, AbilityInfo.PitchforkID, AbilityInfo.LanceID, AbilityInfo.HeavyCrossbowID),
			new AbilityInfo(6, AbilityInfo.PickaxeID, AbilityInfo.TwoHandedAxeID, AbilityInfo.DoubleAxeID, AbilityInfo.ScimitarID, AbilityInfo.KatanaID, AbilityInfo.CrescentBladeID, AbilityInfo.QuarterStaffID, AbilityInfo.DoubleBladedStaffID, AbilityInfo.RepeatingCrossbowID),
			new AbilityInfo(7, AbilityInfo.ButcherKnifeID, AbilityInfo.CleaverID, AbilityInfo.DaggerID, AbilityInfo.PikeID, AbilityInfo.KryssID, AbilityInfo.DoubleBladedStaffID),
			new AbilityInfo(8, AbilityInfo.ExecAxeID, AbilityInfo.BoneHarvesterID, AbilityInfo.CrescentBladeID, AbilityInfo.HammerPickID, AbilityInfo.ScepterID, AbilityInfo.ShortSpearID, AbilityInfo.CrossbowID, AbilityInfo.BowID),
			new AbilityInfo(9, AbilityInfo.HeavyCrossbowID, AbilityInfo.CompositeBowID, AbilityInfo.RepeatingCrossbowID),
			new AbilityInfo(10, AbilityInfo.VikingSwordID, AbilityInfo.BardicheID, AbilityInfo.ScimitarID, AbilityInfo.ScytheID, AbilityInfo.BoneHarvesterID, AbilityInfo.GnarledStaffID, AbilityInfo.BlackStaffID, AbilityInfo.PikeID, AbilityInfo.SpearID, AbilityInfo.BowID),
			new AbilityInfo(11, AbilityInfo.SkinningKnifeID, AbilityInfo.TwoHandedAxeID, AbilityInfo.CutlassID, AbilityInfo.SmithsHammerID, AbilityInfo.ClubID, AbilityInfo.DaggerID, AbilityInfo.ShortSpearID, AbilityInfo.SledgeHammerID),
			new AbilityInfo(12, AbilityInfo.LargeBattleAxeID, AbilityInfo.HalberdID, AbilityInfo.DoubleAxeID, AbilityInfo.WarHammerID, AbilityInfo.BlackStaffID)
		};
	}
}
