using System;
using System.Collections;
using System.IO;
using Veritas;

namespace UOAIO;

public class Config
{
	private static ArrayList m_PaperdollCFG;

	private static string[] m_FileNames;

	public static string GetFile(int FileID)
	{
		return Config.m_FileNames[FileID];
	}

	public static int GetPaperdollGump(int BodyID)
	{
		int count = Config.m_PaperdollCFG.Count;
		for (int i = 0; i < count; i++)
		{
			PaperdollEntry paperdollEntry = (PaperdollEntry)Config.m_PaperdollCFG[i];
			if (paperdollEntry.BodyID == BodyID)
			{
				return paperdollEntry.GumpID;
			}
		}
		return 0;
	}

	static Config()
	{
		Config.m_FileNames = new string[27]
		{
			"Skills.idx", "Skills.mul", "SoundIdx.mul", "Sound.mul", "LightIdx.mul", "Light.mul", "Fonts.mul", "TileData.mul", "Anim.idx", "Anim.mul",
			"ArtIdx.mul", "Art.mul", "TexIdx.mul", "TexMaps.mul", "Hues.mul", "Multi.idx", "Multi.mul", "Map0.mul", "Map2.mul", "Statics0.mul",
			"Statics2.mul", "StaIdx0.mul", "StaIdx2.mul", "AnimData.mul", "VerData.mul", "GumpIdx.mul", "GumpArt.mul"
		};
		if (!File.Exists(Engine.FileManager.ResolveMUL(Files.Verdata)))
		{
			string text = Engine.FileManager.BasePath("data/ultima/empty-verdata.mul");
			Config.m_FileNames[24] = text;
			if (!File.Exists(text))
			{
				using Stream stream = File.Create(text, 4);
				stream.Write(new byte[4], 0, 4);
				stream.Flush();
			}
		}
		Config.m_PaperdollCFG = new ArrayList();
		ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile("play/config/paperdoll.cfg");
		if (archivedFile == null)
		{
			return;
		}
		using StreamReader streamReader = new StreamReader(archivedFile.Download());
		string text2 = null;
		while ((text2 = streamReader.ReadLine()) != null)
		{
			string[] array = text2.Split('\t');
			if (array.Length >= 2)
			{
				try
				{
					Config.m_PaperdollCFG.Add(new PaperdollEntry(Convert.ToInt32(array[0], 16), Convert.ToInt32(array[1], 16)));
				}
				catch
				{
				}
			}
		}
	}
}
