using System;

namespace UOAIO;

public class OptionableAttribute : Attribute
{
	private string m_Category;

	private string m_Name;

	private object m_Default;

	private bool m_OnlyAOS;

	public string Name
	{
		get
		{
			return this.m_Name;
		}
		set
		{
			this.m_Name = value;
		}
	}

	public string Category
	{
		get
		{
			return this.m_Category;
		}
		set
		{
			this.m_Category = value;
		}
	}

	public object Default
	{
		get
		{
			return this.m_Default;
		}
		set
		{
			this.m_Default = value;
		}
	}

	public bool OnlyAOS
	{
		get
		{
			return this.m_OnlyAOS;
		}
		set
		{
			this.m_OnlyAOS = value;
		}
	}

	public OptionableAttribute(string name)
	{
		this.m_Name = name;
		this.m_Category = "Misc";
	}

	public OptionableAttribute(string name, string category)
	{
		this.m_Name = name;
		this.m_Category = category;
	}
}
