using System;
using System.Collections;
using System.IO;
using Veritas;

namespace UOAIO;

public class MountTable
{
	private int m_Default;

	private Hashtable m_Entries;

	private bool m_Disposed;

	public void Dispose()
	{
		if (!this.m_Disposed)
		{
			this.m_Disposed = true;
			this.m_Entries.Clear();
			this.m_Entries = null;
		}
	}

	public MountTable()
	{
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile("play/config/mounts.cfg");
		if (archivedFile != null)
		{
			using (StreamReader ip = new StreamReader(archivedFile.Download()))
			{
				this.Load(ip);
				return;
			}
		}
		this.Default();
	}

	public int Translate(int itemID)
	{
		object obj = this.m_Entries[itemID];
		if (obj != null)
		{
			return (int)obj;
		}
		return this.m_Default;
	}

	public bool IsMount(int body)
	{
		return this.m_Entries.ContainsValue(body);
	}

	private void Load(StreamReader ip)
	{
		this.m_Entries = new Hashtable();
		int num = 0;
		string text;
		while ((text = ip.ReadLine()) != null)
		{
			text = text.Trim();
			if (text.Length > 0 && text[0] != '#')
			{
				int num2 = text.IndexOf('\t');
				string text2 = text.Substring(0, num2);
				string text3 = text.Substring(num2 + 1);
				bool flag = false;
				int num3;
				if (text2.StartsWith("0x"))
				{
					num3 = Convert.ToInt32(text2.Substring(2), 16);
				}
				else if (text2 == "default")
				{
					flag = true;
					num3 = 0;
				}
				else
				{
					num3 = Convert.ToInt32(text2);
				}
				int num4 = ((!text3.StartsWith("0x")) ? Convert.ToInt32(text3) : Convert.ToInt32(text3.Substring(2), 16));
				if (!flag)
				{
					this.m_Entries[num3] = num4;
				}
				else
				{
					this.m_Default = num4;
				}
				num++;
			}
		}
	}

	private void Save(string filePath)
	{
		try
		{
			using StreamWriter streamWriter = new StreamWriter(filePath, append: false);
			streamWriter.WriteLine("# Defines the table used to translate internal mount items to their corresponding body numbers");
			streamWriter.WriteLine("# All lines are trimmed. Empty lines, and lines starting with '#' are ignored.");
			streamWriter.WriteLine("# Format: <item number><tab><body number>");
			streamWriter.WriteLine("# Parser supports hex or decimal numbers. Any numbers prefixed with \"0x\" are treated as hex.");
			streamWriter.WriteLine("# Any lines improperly formatted, the parser will ignore.");
			streamWriter.WriteLine("# The \"default\" item number is a special case, item numbers which are not in the table fall into this category.");
			streamWriter.WriteLine("# Generated on {0}", DateTime.Now);
			streamWriter.WriteLine();
			foreach (DictionaryEntry entry in this.m_Entries)
			{
				streamWriter.Write("0x");
				streamWriter.Write(((int)entry.Key).ToString("X"));
				streamWriter.Write("\t0x");
				streamWriter.WriteLine(((int)entry.Value).ToString("X"));
			}
			streamWriter.WriteLine("default\t0x{0:X}", this.m_Default);
		}
		catch (Exception ex)
		{
			Debug.Trace("Error saving '{0}':", filePath);
			Debug.Error(ex);
		}
	}

	private void Default()
	{
		int[,] array = new int[41, 2]
		{
			{ 16023, 195 },
			{ 16024, 194 },
			{ 16026, 193 },
			{ 16027, 192 },
			{ 16028, 191 },
			{ 16030, 190 },
			{ 16031, 200 },
			{ 16032, 226 },
			{ 16033, 228 },
			{ 16034, 204 },
			{ 16035, 210 },
			{ 16036, 218 },
			{ 16037, 219 },
			{ 16038, 220 },
			{ 16039, 116 },
			{ 16040, 117 },
			{ 16041, 114 },
			{ 16042, 115 },
			{ 16043, 170 },
			{ 16044, 171 },
			{ 16045, 132 },
			{ 16052, 122 },
			{ 16047, 120 },
			{ 16048, 121 },
			{ 16049, 119 },
			{ 16050, 118 },
			{ 16051, 144 },
			{ 16053, 177 },
			{ 16054, 178 },
			{ 16055, 179 },
			{ 16056, 188 },
			{ 16058, 187 },
			{ 16059, 793 },
			{ 16060, 791 },
			{ 16061, 794 },
			{ 16062, 799 },
			{ 16016, 276 },
			{ 16017, 277 },
			{ 16018, 284 },
			{ 16020, 243 },
			{ 16069, 213 }
		};
		int length = array.GetLength(0);
		this.m_Entries = new Hashtable(length);
		for (int i = 0; i < length; i++)
		{
			this.m_Entries[array[i, 0]] = array[i, 1];
		}
		this.m_Default = 200;
	}
}
