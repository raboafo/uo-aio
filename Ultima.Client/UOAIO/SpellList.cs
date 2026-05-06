using System;
using System.IO;
using System.Xml;
using Veritas;

namespace UOAIO;

public class SpellList
{
	private int m_Circles;

	private bool m_DisplayCircles;

	private bool m_DisplayIndex;

	private Spell[] m_Spells;

	private int m_SpellID;

	private int m_Start;

	private int m_SpellsPerCircle;

	public int Circles => this.m_Circles;

	public int Start => this.m_Start;

	public int SpellsPerCircle => this.m_SpellsPerCircle;

	public bool DisplayCircles => this.m_DisplayCircles;

	public bool DisplayIndex => this.m_DisplayIndex;

	public Spell[] Spells => this.m_Spells;

	public SpellList(string name)
	{
		bool flag = true;
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile($"play/config/spells/{name}.xml");
		if (archivedFile != null)
		{
			using Stream input = archivedFile.Download();
			XmlTextReader xmlTextReader = new XmlTextReader(input)
			{
				WhitespaceHandling = WhitespaceHandling.None
			};
			flag = !this.Parse(xmlTextReader);
			xmlTextReader.Close();
		}
		if (flag)
		{
			this.m_Circles = 0;
			this.m_DisplayCircles = false;
			this.m_DisplayIndex = false;
			this.m_Spells = new Spell[0];
			this.m_SpellID = 0;
		}
	}

	private bool Parse_Reagent(XmlTextReader xml, Spell spell)
	{
		string text = null;
		while (xml.MoveToNextAttribute())
		{
			string name = xml.Name;
			string text2 = name;
			if (text2 == "name")
			{
				text = xml.Value;
				continue;
			}
			return false;
		}
		if (text == null)
		{
			return false;
		}
		spell.Reagents.Add(new Reagent(text));
		return true;
	}

	private bool Parse_Tithing(XmlTextReader xml, Spell spell)
	{
		string text = null;
		while (xml.MoveToNextAttribute())
		{
			string name = xml.Name;
			string text2 = name;
			if (text2 == "value")
			{
				text = xml.Value;
				continue;
			}
			return false;
		}
		if (text == null)
		{
			return false;
		}
		spell.Tithing = Convert.ToInt32(text);
		return true;
	}

	private bool Parse_Skill(XmlTextReader xml, Spell spell)
	{
		string text = null;
		while (xml.MoveToNextAttribute())
		{
			string name = xml.Name;
			string text2 = name;
			if (text2 == "value")
			{
				text = xml.Value;
				continue;
			}
			return false;
		}
		if (text == null)
		{
			return false;
		}
		spell.Skill = Convert.ToInt32(text);
		return true;
	}

	private bool Parse_Mana(XmlTextReader xml, Spell spell)
	{
		string text = null;
		while (xml.MoveToNextAttribute())
		{
			string name = xml.Name;
			string text2 = name;
			if (text2 == "value")
			{
				text = xml.Value;
				continue;
			}
			return false;
		}
		if (text == null)
		{
			return false;
		}
		spell.Mana = Convert.ToInt32(text);
		return true;
	}

	private bool Parse_Spell(XmlTextReader xml)
	{
		string text = null;
		string text2 = null;
		while (xml.MoveToNextAttribute())
		{
			string name = xml.Name;
			string text3 = name;
			if (!(text3 == "name"))
			{
				if (!(text3 == "power"))
				{
					return false;
				}
				text2 = xml.Value;
			}
			else
			{
				text = xml.Value;
			}
		}
		if (text == null)
		{
			return false;
		}
		if (text2 == null)
		{
			text2 = "";
		}
		Spell spell = new Spell(text, text2, this.m_Start + this.m_SpellID);
		this.m_Spells[this.m_SpellID++] = spell;
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
			case XmlNodeType.Element:
				switch (xml.Name)
				{
				case "reagent":
					if (!this.Parse_Reagent(xml, spell))
					{
						return false;
					}
					break;
				case "tithing":
					if (!this.Parse_Tithing(xml, spell))
					{
						return false;
					}
					break;
				case "skill":
					if (!this.Parse_Skill(xml, spell))
					{
						return false;
					}
					break;
				case "mana":
					if (!this.Parse_Mana(xml, spell))
					{
						return false;
					}
					break;
				default:
					return false;
				}
				break;
			case XmlNodeType.EndElement:
				return true;
			default:
				return false;
			case XmlNodeType.Comment:
				break;
			}
		}
		return false;
	}

	private bool Parse_Spells(XmlTextReader xml)
	{
		int circles = 0;
		int num = 0;
		int start = 0;
		int spellsPerCircle = 0;
		bool displayCircles = false;
		bool displayIndex = false;
		while (xml.MoveToNextAttribute())
		{
			switch (xml.Name)
			{
			case "circles":
				circles = Convert.ToInt32(xml.Value);
				break;
			case "count":
				num = Convert.ToInt32(xml.Value);
				break;
			case "start":
				start = Convert.ToInt32(xml.Value);
				break;
			case "spellsPerCircle":
				spellsPerCircle = Convert.ToInt32(xml.Value);
				break;
			case "displayCircles":
				displayCircles = Convert.ToBoolean(xml.Value);
				break;
			case "displayIndex":
				displayIndex = Convert.ToBoolean(xml.Value);
				break;
			default:
				return false;
			}
		}
		this.m_Circles = circles;
		this.m_DisplayCircles = displayCircles;
		this.m_DisplayIndex = displayIndex;
		this.m_Start = start;
		this.m_SpellsPerCircle = spellsPerCircle;
		this.m_Spells = new Spell[num];
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
			case XmlNodeType.EndElement:
				return true;
			case XmlNodeType.Element:
			{
				string name = xml.Name;
				string text = name;
				if (text == "spell")
				{
					if (!this.Parse_Spell(xml))
					{
						return false;
					}
					break;
				}
				return false;
			}
			default:
				return false;
			case XmlNodeType.Comment:
				break;
			}
		}
		return false;
	}

	private bool Parse(XmlTextReader xml)
	{
		while (xml.Read())
		{
			switch (xml.NodeType)
			{
			case XmlNodeType.Element:
			{
				string name = xml.Name;
				string text = name;
				if (text == "spells")
				{
					if (!this.Parse_Spells(xml))
					{
						return false;
					}
					break;
				}
				return false;
			}
			default:
				return false;
			case XmlNodeType.Comment:
			case XmlNodeType.XmlDeclaration:
				break;
			}
		}
		return true;
	}
}
