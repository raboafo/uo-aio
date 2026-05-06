using System;
using System.Collections.Generic;

namespace UOAIO;

public sealed class CommandArgs
{
	private Mobile m_Mobile;

	private string[] m_Parameters;

	private string[] m_Arguments;

	private int m_Step;

	private bool m_GoDefault;

	public Mobile Mobile => this.m_Mobile;

	public string[] Parameters => this.m_Parameters;

	public string[] Arguments => this.m_Arguments;

	public string Command => (this.m_Step - 1 >= 0 && this.m_Step - 1 < this.m_Parameters.Length) ? this.m_Parameters[this.m_Step - 1] : "";

	public int Step
	{
		get
		{
			return this.m_Step;
		}
		set
		{
			this.m_Step = value;
		}
	}

	public bool GoDefault
	{
		get
		{
			return this.m_GoDefault;
		}
		set
		{
			this.m_GoDefault = value;
		}
	}

	public int Length => this.m_Parameters.Length - this.m_Step;

	public string GetArgument(int index)
	{
		if (index < 0 || index >= this.m_Arguments.Length - this.m_Step)
		{
			return "";
		}
		return this.m_Arguments[index + this.m_Step];
	}

	public string GetString(int index)
	{
		if (index < 0 || index >= this.m_Parameters.Length - this.m_Step)
		{
			return "";
		}
		return this.m_Parameters[index + this.m_Step];
	}

	public int GetInt32(int index)
	{
		int result = 0;
		if (index >= 0 && index < this.m_Parameters.Length - this.m_Step)
		{
			int.TryParse(this.m_Parameters[index + this.m_Step], out result);
		}
		return result;
	}

	public bool GetBoolean(int index)
	{
		if (index < 0 || index >= this.m_Parameters.Length - this.m_Step)
		{
			return false;
		}
		switch (this.GetString(index).ToLower())
		{
		case "1":
		case "on":
		case "yes":
		case "true":
			return true;
		case "0":
		case "no":
		case "off":
		case "false":
			return false;
		default:
			try
			{
				return bool.Parse(this.m_Parameters[index + this.m_Step]);
			}
			catch
			{
				return false;
			}
		}
	}

	public double GetDouble(int index)
	{
		if (index < 0 || index >= this.m_Parameters.Length - this.m_Step)
		{
			return 0.0;
		}
		try
		{
			return double.Parse(this.m_Parameters[index + this.m_Step]);
		}
		catch
		{
			return 0.0;
		}
	}

	public TimeSpan GetTimeSpan(int index)
	{
		if (index < 0 || index >= this.m_Parameters.Length - this.m_Step)
		{
			return TimeSpan.Zero;
		}
		try
		{
			return TimeSpan.Parse(this.m_Parameters[index + this.m_Step]);
		}
		catch
		{
			return TimeSpan.Zero;
		}
	}

	public CommandArgs(Mobile mob, string args)
	{
		this.m_Mobile = mob;
		CommandArgs.Split(args, out this.m_Parameters, out this.m_Arguments);
	}

	public CommandArgs(Mobile mob, string[] parms, string[] args)
	{
		this.m_Mobile = mob;
		this.m_Parameters = parms;
		this.m_Arguments = args;
	}

	public static void Split(string value, out string[] parms, out string[] args)
	{
		char[] array = value.ToCharArray();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		int num = 0;
		int num2 = 0;
		while (num < array.Length)
		{
			switch (array[num])
			{
			case '"':
				num++;
				for (num2 = num; num2 < array.Length && (array[num2] != '"' || array[num2 - 1] == '\\'); num2++)
				{
				}
				list.Add(value.Substring(num, num2 - num));
				list2.Add(value.Substring(num - 1));
				num = num2 + 2;
				break;
			default:
				for (num2 = num; num2 < array.Length && array[num2] != ' '; num2++)
				{
				}
				list.Add(value.Substring(num, num2 - num));
				list2.Add(value.Substring(num));
				num = num2 + 1;
				break;
			case ' ':
				num++;
				break;
			}
		}
		parms = list.ToArray();
		args = list2.ToArray();
	}
}
