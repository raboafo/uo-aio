namespace UOAIO;

public class ResistInfo
{
	public int m_Physical;

	public int m_Fire;

	public int m_Cold;

	public int m_Poison;

	public int m_Energy;

	public string[] m_Names;

	public static ResistInfo[] m_Armor;

	public static ResistInfo[] m_Materials;

	public ResistInfo(int physical, int fire, int cold, int poison, int energy, params string[] names)
	{
		this.m_Physical = physical;
		this.m_Fire = fire;
		this.m_Cold = cold;
		this.m_Poison = poison;
		this.m_Energy = energy;
		this.m_Names = names;
	}

	public static ResistInfo Find(string text, ResistInfo[] list)
	{
		for (int i = 0; i < list.Length; i++)
		{
			for (int j = 0; j < list[i].m_Names.Length; j++)
			{
				if (text.IndexOf(list[i].m_Names[j]) >= 0)
				{
					return list[i];
				}
			}
		}
		return null;
	}

	static ResistInfo()
	{
		ResistInfo.m_Armor = new ResistInfo[13]
		{
			new ResistInfo(2, 4, 3, 3, 3, "leather ", "female leather "),
			new ResistInfo(2, 4, 3, 3, 4, "studded ", "female studded ", " ranger armor"),
			new ResistInfo(3, 3, 1, 5, 3, "ringmail "),
			new ResistInfo(5, 3, 2, 3, 2, "platemail "),
			new ResistInfo(3, 3, 3, 3, 3, "dragon "),
			new ResistInfo(6, 6, 7, 5, 7, "daemon bone "),
			new ResistInfo(4, 4, 4, 1, 2, "chainmail "),
			new ResistInfo(3, 3, 4, 2, 4, "bone "),
			new ResistInfo(7, 2, 2, 2, 2, "bascinet"),
			new ResistInfo(3, 3, 3, 3, 3, "close helm"),
			new ResistInfo(2, 4, 4, 3, 2, "helmet"),
			new ResistInfo(4, 1, 4, 4, 2, "norse helm"),
			new ResistInfo(3, 1, 3, 3, 4, "orc helm")
		};
		ResistInfo.m_Materials = new ResistInfo[17]
		{
			new ResistInfo(6, 0, 0, 0, 0, "dull copper "),
			new ResistInfo(2, 1, 0, 0, 5, "shadow iron "),
			new ResistInfo(1, 1, 0, 5, 2, "copper "),
			new ResistInfo(3, 0, 5, 1, 1, "bronze "),
			new ResistInfo(1, 1, 2, 0, 2, "golden "),
			new ResistInfo(2, 3, 2, 2, 2, "agapite "),
			new ResistInfo(3, 3, 2, 3, 1, "verite "),
			new ResistInfo(4, 0, 3, 3, 3, "valorite "),
			new ResistInfo(5, 0, 0, 0, 0, "spined "),
			new ResistInfo(2, 3, 2, 2, 2, "horned "),
			new ResistInfo(2, 1, 2, 3, 4, "barbed "),
			new ResistInfo(0, 10, -3, 0, 0, "red dragon "),
			new ResistInfo(-3, 0, 0, 0, 0, "yellow dragon "),
			new ResistInfo(10, 0, 0, 0, -3, "black dragon "),
			new ResistInfo(0, -3, 0, 10, 0, "green dragon "),
			new ResistInfo(-3, 0, 10, 0, 0, "white dragon "),
			new ResistInfo(0, 0, 0, -3, 10, "blue dragon ")
		};
	}
}
