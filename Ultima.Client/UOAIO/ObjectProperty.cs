using System;
using System.Text.RegularExpressions;

namespace UOAIO;

public class ObjectProperty
{
	private int m_Number;

	private string m_Arguments;

	private string m_Text;

	private static string[] m_Args;

	private static Regex m_ArgReplace;

	public int Number => this.m_Number;

	public string Arguments => this.m_Arguments;

	public string Text => this.m_Text;

	public ObjectProperty(int number, string arguments)
	{
		this.m_Number = number;
		this.m_Arguments = arguments;
		ObjectProperty.m_Args = arguments.Split('\t');
		for (int i = 0; i < ObjectProperty.m_Args.Length; i++)
		{
			if (ObjectProperty.m_Args[i].Length > 1 && ObjectProperty.m_Args[i].StartsWith("#"))
			{
				try
				{
					ObjectProperty.m_Args[i] = Localization.GetString(Convert.ToInt32(ObjectProperty.m_Args[i].Substring(1)));
				}
				catch
				{
				}
			}
		}
		this.m_Text = Localization.GetString(number);
		this.m_Text = ObjectProperty.m_ArgReplace.Replace(this.m_Text, ArgReplace_Eval);
	}

	private static string ArgReplace_Eval(Match m)
	{
		try
		{
			int num = Convert.ToInt32(m.Groups[1].Value) - 1;
			return ObjectProperty.m_Args[num];
		}
		catch
		{
			return m.Value;
		}
	}

	static ObjectProperty()
	{
		ObjectProperty.m_ArgReplace = new Regex("~(?<1>\\d+).*?~", RegexOptions.Singleline);
	}
}
