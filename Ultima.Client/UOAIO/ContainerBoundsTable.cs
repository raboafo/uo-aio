using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Veritas;

namespace UOAIO;

public class ContainerBoundsTable
{
	private Rectangle m_Default;

	private Dictionary<int, Rectangle> m_Entries;

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

	public ContainerBoundsTable()
	{
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile("play/config/container-bounds.cfg");
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

	public Rectangle Translate(int gumpID)
	{
		gumpID &= 0x3FFF;
		if (!this.m_Entries.TryGetValue(gumpID, out var value))
		{
			return this.m_Default;
		}
		return value;
	}

	private void Load(StreamReader ip)
	{
		this.m_Entries = new Dictionary<int, Rectangle>();
		int num = 0;
		string text;
		while ((text = ip.ReadLine()) != null)
		{
			text = text.Trim();
			if (text.Length > 0 && text[0] != '#')
			{
				string[] array = text.Split('\t');
				Rectangle rectangle = new Rectangle(this.IntConvert(array[1]), this.IntConvert(array[2]), this.IntConvert(array[3]), this.IntConvert(array[4]));
				if (array[0] == "default")
				{
					this.m_Default = rectangle;
				}
				else
				{
					this.m_Entries[this.IntConvert(array[0]) & 0x3FFF] = rectangle;
				}
				num++;
			}
		}
	}

	private int IntConvert(string s)
	{
		if (s.StartsWith("0x"))
		{
			return Convert.ToInt32(s.Substring(2), 16);
		}
		return Convert.ToInt32(s);
	}

	private void Default()
	{
		int[,] array = new int[25, 5]
		{
			{ 7, 30, 30, 240, 140 },
			{ 9, 20, 85, 104, 111 },
			{ 60, 44, 65, 142, 94 },
			{ 61, 29, 34, 108, 94 },
			{ 62, 33, 36, 109, 112 },
			{ 63, 19, 47, 163, 76 },
			{ 64, 16, 38, 136, 87 },
			{ 65, 35, 38, 110, 78 },
			{ 66, 18, 105, 144, 73 },
			{ 67, 16, 51, 168, 73 },
			{ 68, 20, 10, 150, 90 },
			{ 71, 16, 10, 132, 128 },
			{ 72, 16, 10, 138, 84 },
			{ 73, 18, 105, 144, 73 },
			{ 74, 18, 105, 144, 73 },
			{ 75, 16, 51, 168, 73 },
			{ 76, 46, 74, 150, 110 },
			{ 77, 76, 12, 64, 56 },
			{ 78, 24, 96, 172, 56 },
			{ 79, 24, 96, 172, 56 },
			{ 81, 16, 10, 138, 84 },
			{ 82, 0, 0, 110, 62 },
			{ 2330, 0, 0, 282, 230 },
			{ 2350, 0, 0, 282, 210 },
			{ 10851, 60, 33, 400, 315 }
		};
		int length = array.GetLength(0);
		this.m_Entries = new Dictionary<int, Rectangle>(length);
		for (int i = 0; i < length; i++)
		{
			this.m_Entries[array[i, 0]] = new Rectangle(array[i, 1], array[i, 2], array[i, 3], array[i, 4]);
		}
		this.m_Default = new Rectangle(0, 0, 200, 200);
	}
}
