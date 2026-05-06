using System;

namespace UOAIO;

public class JournalEntry
{
	private Texture m_Image;

	private IHue m_Hue;

	private string m_Text;

	private int m_Width;

	private int m_Serial;

	private DateTime m_Time;

	public DateTime Time => this.m_Time;

	public IHue Hue => this.m_Hue;

	public int Serial => this.m_Serial;

	public Texture Image
	{
		get
		{
			return this.m_Image;
		}
		set
		{
			this.m_Image = value;
		}
	}

	public int Width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
		}
	}

	public string Text
	{
		get
		{
			return this.m_Text;
		}
		set
		{
			this.m_Text = value;
		}
	}

	public JournalEntry(string text, IHue hue, int serial)
	{
		this.m_Text = text;
		this.m_Hue = hue;
		this.m_Serial = serial;
		this.m_Time = DateTime.Now;
	}
}
