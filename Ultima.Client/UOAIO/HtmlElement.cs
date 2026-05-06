using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace UOAIO;

public class HtmlElement
{
	private string m_Name;

	private ElementType m_Type;

	private Hashtable m_Attributes;

	private static Regex[] m_AttributesRegex;

	public string Name => this.m_Name;

	public ElementType Type => this.m_Type;

	public string GetAttribute(string name)
	{
		if (this.m_Attributes == null)
		{
			return null;
		}
		return (string)this.m_Attributes[name];
	}

	public HtmlElement(string name, ElementType type, Hashtable attributes)
	{
		this.m_Name = name;
		this.m_Type = type;
		this.m_Attributes = attributes;
	}

	public static HtmlElement[] GetElements(string text)
	{
		ArrayList arrayList = new ArrayList();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (true)
		{
			num3 = num2;
			num = text.IndexOf('<', num2);
			if (num == -1)
			{
				arrayList.Add(new HtmlElement(text.Substring(num3), ElementType.Text, null));
				break;
			}
			num2 = text.IndexOf('>', num + 1);
			if (num2 == -1)
			{
				arrayList.Add(new HtmlElement(text.Substring(num3), ElementType.Text, null));
				break;
			}
			if (num != num3)
			{
				arrayList.Add(new HtmlElement(text.Substring(num3, num - num3), ElementType.Text, null));
			}
			num2++;
			arrayList.Add(HtmlElement.Parse(text.Substring(num, num2 - num)));
		}
		return (HtmlElement[])arrayList.ToArray(typeof(HtmlElement));
	}

	public static HtmlElement Parse(string ele)
	{
		if (ele.StartsWith("<"))
		{
			ele = ele.Substring(1);
		}
		if (ele.EndsWith(">"))
		{
			ele = ele.Substring(0, ele.Length - 1);
		}
		ElementType elementType = ((!ele.StartsWith("/")) ? ElementType.Start : ElementType.End);
		if (elementType == ElementType.End)
		{
			ele = ele.Substring(1);
		}
		int num = ele.IndexOf(' ');
		if (num == -1)
		{
			return new HtmlElement(ele, elementType, null);
		}
		string name = ele.Substring(0, num);
		string text = ele.Substring(++num);
		Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < HtmlElement.m_AttributesRegex.Length; i++)
		{
			while (true)
			{
				Match match = HtmlElement.m_AttributesRegex[i].Match(text);
				if (!match.Success)
				{
					break;
				}
				string value = match.Groups["name"].Value;
				string value2 = match.Groups["value"].Value;
				hashtable[value] = value2;
				text = text.Remove(match.Index, match.Length);
			}
		}
		return new HtmlElement(name, elementType, hashtable);
	}

	static HtmlElement()
	{
		HtmlElement.m_AttributesRegex = new Regex[3]
		{
			new Regex("(?<name>\\w+)\\s*=\\s*'(?<value>.*?)'"),
			new Regex("(?<name>\\w+)\\s*=\\s*\"(?<value>.*?)\""),
			new Regex("(?<name>\\w+)\\s*=\\s*(?<value>[^\\s]*)")
		};
	}
}
