using System.Collections.Generic;
using UOAIO.Profiles;

namespace UOAIO;

public class Spell
{
	private string m_Name;

	private int m_SpellID;

	private Power[] m_Power;

	private List<Reagent> m_Reagents;

	private string m_FullPower;

	private int m_Tithing;

	private int m_Skill;

	private int m_Mana;

	public SpellID SpellID => (SpellID)this.m_SpellID;

	public int Circle => 1 + (this.m_SpellID - 1) / 8;

	public List<Reagent> Reagents => this.m_Reagents;

	public Power[] Power => this.m_Power;

	public string FullPower => this.m_FullPower;

	public string Name => this.m_Name;

	public int Tithing
	{
		get
		{
			return this.m_Tithing;
		}
		set
		{
			this.m_Tithing = value;
		}
	}

	public int Skill
	{
		get
		{
			return this.m_Skill;
		}
		set
		{
			this.m_Skill = value;
		}
	}

	public int Mana
	{
		get
		{
			return this.m_Mana;
		}
		set
		{
			this.m_Mana = value;
		}
	}

	public Spell(string name, string power, int spellID)
	{
		this.m_Name = name;
		this.m_Power = UOAIO.Power.Parse(power);
		this.m_FullPower = power;
		this.m_SpellID = spellID;
		this.m_Reagents = new List<Reagent>();
	}

	public void Cast()
	{
		if (Preferences.Current.Options.ClearHandsBeforeCast)
		{
			Engine.Dequip(message: false);
		}
		Network.Send(new PCastSpell(this.m_SpellID));
	}
}
