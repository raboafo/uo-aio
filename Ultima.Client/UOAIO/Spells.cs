using System;
using System.Text;
using System.Windows.Forms;

namespace UOAIO;

public class Spells
{
	private class GMinimizer : GRegion
	{
		public GMinimizer()
			: base(4, 101, 17, 17)
		{
			base.m_Tooltip = new Tooltip("Minimize");
		}

		protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
		{
			if ((mb & MouseButtons.Left) != MouseButtons.None)
			{
				base.m_Parent.Visible = false;
				Gumps.Desktop.Children.Add(new GSpellbookIcon(base.m_Parent, (Item)base.m_Parent.GetTag("Container")));
			}
		}
	}

	private static Reagent[] m_Reagents;

	private static SpellList m_RegularList;

	private static SpellList m_PaladinList;

	private static SpellList m_NecromancerList;

	private static string[] m_Numbers;

	public static SpellList RegularList
	{
		get
		{
			if (Spells.m_RegularList == null)
			{
				Spells.m_RegularList = new SpellList("magery");
			}
			return Spells.m_RegularList;
		}
	}

	public static SpellList PaladinList
	{
		get
		{
			if (Spells.m_PaladinList == null)
			{
				Spells.m_PaladinList = new SpellList("chivalry");
			}
			return Spells.m_PaladinList;
		}
	}

	public static SpellList NecromancerList
	{
		get
		{
			if (Spells.m_NecromancerList == null)
			{
				Spells.m_NecromancerList = new SpellList("necromancy");
			}
			return Spells.m_NecromancerList;
		}
	}

	public static Reagent[] Reagents => Spells.m_Reagents;

	public static Spell GetSpellByPower(string power)
	{
		Spell spellByPower = Spells.GetSpellByPower(Spells.RegularList, power);
		if (spellByPower == null)
		{
			spellByPower = Spells.GetSpellByPower(Spells.PaladinList, power);
		}
		if (spellByPower == null)
		{
			spellByPower = Spells.GetSpellByPower(Spells.NecromancerList, power);
		}
		return spellByPower;
	}

	public static Spell GetSpellByPower(SpellList list, string power)
	{
		for (int i = 0; i < list.Spells.Length; i++)
		{
			if (list.Spells[i].FullPower == power)
			{
				return list.Spells[i];
			}
		}
		return null;
	}

	public static Spell GetSpellByID(int spellID)
	{
		Spell spellByID = Spells.GetSpellByID(Spells.RegularList, spellID);
		if (spellByID == null)
		{
			spellByID = Spells.GetSpellByID(Spells.PaladinList, spellID);
		}
		if (spellByID == null)
		{
			spellByID = Spells.GetSpellByID(Spells.NecromancerList, spellID);
		}
		return spellByID;
	}

	public static Spell GetSpellByID(SpellList list, int spellID)
	{
		if (spellID < list.Start)
		{
			return null;
		}
		spellID -= list.Start;
		if (spellID >= list.Spells.Length)
		{
			return null;
		}
		return list.Spells[spellID];
	}

	public static Spell GetSpellByName(string name)
	{
		Spell spellByName = Spells.GetSpellByName(Spells.RegularList, name);
		if (spellByName == null)
		{
			spellByName = Spells.GetSpellByName(Spells.PaladinList, name);
		}
		if (spellByName == null)
		{
			spellByName = Spells.GetSpellByName(Spells.NecromancerList, name);
		}
		return spellByName;
	}

	public static Spell GetSpellByName(SpellList list, string name)
	{
		for (int i = 0; i < list.Spells.Length; i++)
		{
			if (list.Spells[i].Name == name)
			{
				return list.Spells[i];
			}
		}
		return null;
	}

	public static Reagent GetReagent(string Name)
	{
		return Name switch
		{
			"Black pearl" => Spells.m_Reagents[0], 
			"Bloodmoss" => Spells.m_Reagents[1], 
			"Garlic" => Spells.m_Reagents[2], 
			"Ginseng" => Spells.m_Reagents[3], 
			"Mandrake root" => Spells.m_Reagents[4], 
			"Nightshade" => Spells.m_Reagents[5], 
			"Sulfurous ash" => Spells.m_Reagents[6], 
			"Spiders' silk" => Spells.m_Reagents[7], 
			"Bat wing" => Spells.m_Reagents[8], 
			"Grave dust" => Spells.m_Reagents[9], 
			"Daemon blood" => Spells.m_Reagents[10], 
			"Nox crystal" => Spells.m_Reagents[11], 
			"Pig iron" => Spells.m_Reagents[12], 
			_ => throw new ArgumentException("Invalid reagent name. Valid values are: [\"Black pearl\", \"Bloodmoss\", \"Garlic\", \"Ginseng\", \"Mandrake root\", \"Nightshade\", \"Sulfurous ash\", \"Spiders' silk\"].", Name), 
		};
	}

	static Spells()
	{
		Debug.TimeBlock("Initializing Spells");
		Spells.m_Reagents = new Reagent[13]
		{
			new Reagent("Black pearl", 3962),
			new Reagent("Bloodmoss", 3963),
			new Reagent("Garlic", 3972),
			new Reagent("Ginseng", 3973),
			new Reagent("Mandrake root", 3974),
			new Reagent("Nightshade", 3976),
			new Reagent("Sulfurous ash", 3980),
			new Reagent("Spiders' silk", 3981),
			new Reagent("Bat wing", 3960),
			new Reagent("Grave dust", 3983),
			new Reagent("Daemon blood", 3965),
			new Reagent("Nox crystal", 3982),
			new Reagent("Pig iron", 3978)
		};
		Spells.m_Numbers = new string[8] { "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth" };
		Debug.EndBlock();
	}

	public static Gump OpenSpellbook(Item container)
	{
		Gump gump = Spells.OpenSpellbook(container.Circle, container.LastSpell, container);
		gump.X = (Engine.ScreenWidth - gump.Width) / 2;
		gump.Y = (Engine.ScreenHeight - gump.Height) / 2;
		Gumps.Desktop.Children.Add(gump);
		return gump;
	}

	private static void ChangeCircle_OnClick(Gump sender)
	{
		Gump parent = sender.Parent;
		Item item = (Item)parent.GetTag("Container");
		object tag = sender.GetTag("Circle");
		if (item != null)
		{
			int circle = ((tag != null) ? ((int)tag) : 0);
			int x = parent.X;
			int y = parent.Y;
			Gumps.Destroy(parent);
			Gump gump = Spells.OpenSpellbook(circle, item.LastSpell, item);
			gump.X = x;
			gump.Y = y;
			Gumps.Desktop.Children.Add(gump);
		}
	}

	public static int GetBookType(int itemID)
	{
		switch (itemID)
		{
		case 3643:
		case 3834:
			return 1;
		case 8787:
			return 2;
		case 8786:
			return 3;
		default:
			return 0;
		}
	}

	public static int GetBookIndex(int itemID)
	{
		return Spells.GetBookType(itemID) switch
		{
			2 => 11008, 
			3 => 11009, 
			_ => 2220, 
		};
	}

	public static int GetBookOffset(int itemID)
	{
		return Spells.GetBookType(itemID) switch
		{
			2 => 101, 
			3 => 201, 
			_ => 1, 
		};
	}

	public static int GetBookIcon(int itemID)
	{
		return Spells.GetBookType(itemID) switch
		{
			2 => 11011, 
			3 => 11012, 
			_ => 2234, 
		};
	}

	public static Gump OpenSpellbook(int circle, int lastSpell, Item container)
	{
		container.OpenSB = true;
		container.Circle = circle;
		container.LastSpell = lastSpell;
		circle &= -2;
		Engine.Sounds.PlaySound(85);
		Engine.DoEvents();
		GDragable gDragable = new GDragable(Spells.GetBookIndex(container.ID), 0, 0);
		gDragable.SetTag("Container", container);
		gDragable.SetTag("Dispose", "Spellbook");
		gDragable.Children.Add(new GMinimizer());
		SpellList spellList = ((container.SpellbookOffset == 101) ? Spells.NecromancerList : ((container.SpellbookOffset != 201) ? Spells.RegularList : Spells.PaladinList));
		if (lastSpell >= spellList.Start && lastSpell < spellList.Start + spellList.Spells.Length)
		{
			int num = (lastSpell - spellList.Start) / spellList.SpellsPerCircle;
			int num2 = (lastSpell - spellList.Start) % spellList.SpellsPerCircle;
			if (num >= 0 && num < spellList.Circles)
			{
				if (num == circle)
				{
					gDragable.Children.Add(new GImage(2221, 184, 2));
					gDragable.Children.Add(new GImage(2223, 183, 52 + num2 * 15));
				}
				else if (num == circle + 1)
				{
					gDragable.Children.Add(new GImage(2222, 204, 3));
					gDragable.Children.Add(new GImage(2224, 204, 52 + num2 * 15));
				}
			}
		}
		gDragable.Children.Add(new GLabel("INDEX", Engine.GetFont(6), Hues.Default, 106, 10));
		gDragable.Children.Add(new GLabel("INDEX", Engine.GetFont(6), Hues.Default, 269, 10));
		OnClick clickHandler = ChangeCircle_OnClick;
		int[] array = new int[8] { 58, 93, 130, 164, 227, 260, 297, 332 };
		int[] array2 = new int[2] { 52, 52 };
		if (spellList.DisplayIndex)
		{
			for (int i = 0; i < spellList.Circles; i++)
			{
				GButton gButton = new GButton(2225 + i, 2225 + i, 2225 + i, array[i], 175, clickHandler);
				gButton.SetTag("Circle", i);
				gDragable.Children.Add(gButton);
			}
		}
		if (spellList.DisplayCircles)
		{
			if (circle > 0)
			{
				GButton gButton2 = new GButton(2235, 2235, 2235, 50, 8, clickHandler);
				gButton2.SetTag("Circle", circle - 1);
				gDragable.Children.Add(gButton2);
			}
			if (circle < ((spellList.Circles - 1) & -2))
			{
				GButton gButton3 = new GButton(2236, 2236, 2236, 321, 8, clickHandler);
				gButton3.SetTag("Circle", circle + 2);
				gDragable.Children.Add(gButton3);
			}
			for (int j = circle; j < circle + 2; j++)
			{
				int x = (((j & 1) == 0) ? 62 : 225);
				string arg = ((j >= 0 && j < Spells.m_Numbers.Length) ? Spells.m_Numbers[j] : "Bad");
				gDragable.Children.Add(new GLabel($"{arg} Circle", Engine.GetFont(6), Hues.Default, x, 30));
			}
		}
		int num3 = circle * spellList.SpellsPerCircle;
		int num4 = (circle + 2) * spellList.SpellsPerCircle;
		for (int k = num3; k < num4; k++)
		{
			if (k < num3 || k >= num4 || !container.GetSpellContained(k))
			{
				continue;
			}
			int num5 = k / spellList.SpellsPerCircle;
			Spell spellByID = Spells.GetSpellByID(container.SpellbookOffset + k);
			if (spellByID != null)
			{
				GSpellName gSpellName = new GSpellName(container.SpellbookOffset + k, spellByID.Name, Engine.GetFont(9), Hues.Load(648), Hues.Load(651), 62 + (num5 & 1) * 163, array2[num5 & 1]);
				array2[num5 & 1] += 15;
				string text = $"{spellByID.Name}\n";
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(spellByID.Name);
				stringBuilder.Append('\n');
				for (int l = 0; l < spellByID.Power.Length; l++)
				{
					stringBuilder.Append(spellByID.Power[l].Name);
					stringBuilder.Append(' ');
				}
				for (int m = 0; m < spellByID.Reagents.Count; m++)
				{
					stringBuilder.Append('\n');
					stringBuilder.Append(spellByID.Reagents[m].Name);
				}
				if (spellByID.Tithing > 0)
				{
					stringBuilder.Append('\n');
					stringBuilder.AppendFormat("Tithing: {0}", spellByID.Tithing);
				}
				if (spellByID.Mana > 0)
				{
					stringBuilder.Append('\n');
					stringBuilder.AppendFormat("Mana: {0}", spellByID.Mana);
				}
				if (spellByID.Skill > 0)
				{
					stringBuilder.Append('\n');
					stringBuilder.AppendFormat("Skill: {0}", spellByID.Skill);
				}
				Tooltip tooltip = new Tooltip(stringBuilder.ToString(), Big: true);
				gSpellName.Tooltip = tooltip;
				gDragable.Children.Add(gSpellName);
			}
		}
		return gDragable;
	}
}
