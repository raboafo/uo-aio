namespace UOAIO;

public struct Power
{
	private string m_Name;

	private char m_Symbol;

	public string Name => this.m_Name;

	public char Symbol => this.m_Symbol;

	public Power(string Name)
	{
		this.m_Name = Name;
		if (Name.Length > 0)
		{
			this.m_Symbol = Name[0];
		}
		else
		{
			this.m_Symbol = '\0';
		}
	}

	public static Power[] Parse(string Words)
	{
		string[] array = Words.Split(' ');
		int num = array.Length;
		Power[] array2 = new Power[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = new Power(array[i]);
		}
		return array2;
	}
}
