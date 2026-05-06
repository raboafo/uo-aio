using System;
using System.Collections.Generic;

namespace UOAIO;

public sealed class LayoutEntry
{
	private string name;

	private int[] parameters;

	private Dictionary<string, string> attributes;

	public int[] Parameters => this.parameters;

	public string Name => this.name;

	public int this[int index]
	{
		get
		{
			if (index < 0 || index >= this.parameters.Length)
			{
				return 0;
			}
			return this.parameters[index];
		}
	}

	public string GetAttribute(string name)
	{
		string value = null;
		if (this.attributes != null)
		{
			this.attributes.TryGetValue(name, out value);
		}
		return value;
	}

	public LayoutEntry(string format)
	{
		string[] array = format.Split(' ');
		if (array.Length == 0)
		{
			return;
		}
		this.name = array[0];
		using (new ScratchList<int>())
		{
			List<int> list = new List<int>();
			for (int i = 1; i < array.Length; i++)
			{
				try
				{
					int item;
					if (array[i].Contains("@"))
					{
						item = Convert.ToInt32(array[i].Split('@', '@')[1]);
						Engine.AddTextMessage(item.ToString());
					}
					else
					{
						item = Convert.ToInt32(array[i]);
					}
					list.Add(item);
				}
				catch
				{
					int num = array[i].IndexOf('=');
					if (num <= 0)
					{
						continue;
					}
					try
					{
						string key = array[i].Substring(0, num);
						string value = array[i].Substring(num + 1);
						if (this.attributes == null)
						{
							this.attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
						}
						this.attributes[key] = value;
					}
					catch
					{
					}
				}
			}
			this.parameters = list.ToArray();
		}
	}
}
