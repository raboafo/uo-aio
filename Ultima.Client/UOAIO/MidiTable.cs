using System;
using System.Collections;
using System.IO;
using Veritas;

namespace UOAIO;

public class MidiTable
{
	private Hashtable m_Entries;

	private bool m_Disposed;

	private Hashtable m_Overwrite;

	public void Dispose()
	{
		if (!this.m_Disposed)
		{
			this.m_Disposed = true;
			this.m_Entries.Clear();
			this.m_Entries = null;
		}
	}

	public MidiTable()
	{
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile("play/config/music.cfg");
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

	public string Translate(int midiID)
	{
		if (this.m_Overwrite == null)
		{
			this.LoadMP3Table();
		}
		string text = (string)this.m_Overwrite[midiID];
		if (text == null)
		{
			text = (string)this.m_Entries[midiID];
		}
		return text;
	}

	public void LoadMP3Table()
	{
		this.m_Overwrite = new Hashtable();
		string path = Engine.FileManager.ResolveMUL("music/digital/config.txt");
		if (!File.Exists(path))
		{
			return;
		}
		using StreamReader streamReader = new StreamReader(path);
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			text = text.Trim();
			if (text.Length == 0)
			{
				break;
			}
			string[] array = text.Split(' ');
			if (array.Length < 2)
			{
				break;
			}
			try
			{
				int num = int.Parse(array[0]);
				string text2 = array[1];
				int num2 = text2.IndexOf(',');
				if (num2 >= 0)
				{
					text2 = text2.Substring(0, num2);
				}
				this.m_Overwrite[num] = "digital/" + text2 + ".mp3";
			}
			catch
			{
			}
		}
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
				string value = text.Substring(num2 + 1);
				int num3 = ((!text2.StartsWith("0x")) ? Convert.ToInt32(text2) : Convert.ToInt32(text2.Substring(2), 16));
				this.m_Entries[num3] = value;
				num++;
			}
		}
	}

	private void Default()
	{
		object[,] array = new object[49, 2]
		{
			{ 0, "oldult01.mid" },
			{ 1, "create1.mid" },
			{ 2, "draglift.mid" },
			{ 3, "oldult02.mid" },
			{ 4, "oldult03.mid" },
			{ 5, "oldult04.mid" },
			{ 6, "oldult05.mid" },
			{ 7, "oldult06.mid" },
			{ 8, "stones2.mid" },
			{ 9, "britain1.mid" },
			{ 10, "britain2.mid" },
			{ 11, "bucsden.mid" },
			{ 12, "jhelom.mid" },
			{ 13, "lbcastle.mid" },
			{ 14, "linelle.mid" },
			{ 15, "magincia.mid" },
			{ 16, "minoc.mid" },
			{ 17, "ocllo.mid" },
			{ 18, "samlethe.mid" },
			{ 19, "serpents.mid" },
			{ 20, "skarabra.mid" },
			{ 21, "trinsic.mid" },
			{ 22, "vesper.mid" },
			{ 23, "wind.mid" },
			{ 24, "yew.mid" },
			{ 25, "cave01.mid" },
			{ 26, "dungeon9.mid" },
			{ 27, "forest_a.mid" },
			{ 28, "intown01.mid" },
			{ 29, "jungle_a.mid" },
			{ 30, "mountn_a.mid" },
			{ 31, "plains_a.mid" },
			{ 32, "sailing.mid" },
			{ 33, "swamp_a.mid" },
			{ 34, "tavern01.mid" },
			{ 35, "tavern02.mid" },
			{ 36, "tavern03.mid" },
			{ 37, "tavern04.mid" },
			{ 38, "combat1.mid" },
			{ 39, "combat2.mid" },
			{ 40, "combat3.mid" },
			{ 41, "approach.mid" },
			{ 42, "death.mid" },
			{ 43, "victory.mid" },
			{ 44, "btcastle.mid" },
			{ 45, "nujelm.mid" },
			{ 46, "dungeon2.mid" },
			{ 47, "cove.mid" },
			{ 48, "moonglow.mid" }
		};
		int length = array.GetLength(0);
		this.m_Entries = new Hashtable(length);
		for (int i = 0; i < length; i++)
		{
			this.m_Entries[array[i, 0]] = array[i, 1];
		}
	}
}
