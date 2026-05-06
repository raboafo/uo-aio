using System.Text;

namespace UOAIO;

public class BookPageInfo
{
	private string[] m_Lines;

	public string[] Lines
	{
		get
		{
			return this.m_Lines;
		}
		set
		{
			this.m_Lines = value;
		}
	}

	public BookPageInfo()
	{
		this.m_Lines = new string[0];
	}

	public BookPageInfo(string[] lines)
	{
		this.m_Lines = lines;
	}

	public string GetAllLines()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < this.m_Lines.Length; i++)
		{
			stringBuilder.AppendLine(this.m_Lines[i]);
		}
		return stringBuilder.ToString();
	}
}
