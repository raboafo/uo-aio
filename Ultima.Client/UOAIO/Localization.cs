using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace UOAIO;

public class Localization
{
	private static string m_Language;

	private static string m_Extension;

	private static string m_Cliloc1;

	private static Hashtable m_Files;

	private static Dictionary<int, string> m_Strings;

	private static byte[] m_Buffer;

	public static string Language => Localization.m_Language;

	static Localization()
	{
		Localization.m_Buffer = new byte[1024];
		Localization.m_Language = CultureInfo.CurrentUICulture.ThreeLetterWindowsLanguageName.ToUpper();
		if (File.Exists(Engine.FileManager.ResolveMUL("cliloc-1." + Localization.m_Language)))
		{
			Localization.m_Extension = "." + Localization.m_Language;
		}
		else
		{
			Localization.m_Extension = ".ENU";
		}
		Localization.m_Cliloc1 = "cliloc-1" + Localization.m_Extension;
		Localization.m_Files = new Hashtable();
		Localization.m_Strings = new Dictionary<int, string>(50000);
		Localization.LoadCompiledDatabase();
	}

	private static void LoadCompiledDatabase()
	{
		string path = Engine.FileManager.ResolveMUL("cliloc" + Localization.m_Extension);
		if (!File.Exists(path))
		{
			path = Engine.FileManager.ResolveMUL("cliloc.enu");
		}
		if (!File.Exists(path))
		{
			return;
		}
		using BinaryReader binaryReader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
		binaryReader.ReadInt32();
		binaryReader.ReadInt16();
		while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
		{
			int key = binaryReader.ReadInt32();
			binaryReader.ReadByte();
			int num = binaryReader.ReadInt16();
			if (num > Localization.m_Buffer.Length)
			{
				Localization.m_Buffer = new byte[(num + 1023) & -1024];
			}
			if (num == 0)
			{
				Localization.m_Strings[key] = "";
				continue;
			}
			binaryReader.Read(Localization.m_Buffer, 0, num);
			Localization.m_Strings[key] = Encoding.UTF8.GetString(Localization.m_Buffer, 0, num);
		}
	}

	public static string GetString(int number)
	{
		Localization.m_Strings.TryGetValue(number, out var value);
		if (value == null)
		{
			Debug.Trace("Can't find localization string, it's possible you're using in an incomaptible client version");
			value = "<fallback text>";
		}
		Engine.AddTextMessage(value);
		return value;
	}

	public static LocalizationFile GetFile(string path)
	{
		LocalizationFile localizationFile = (LocalizationFile)Localization.m_Files[path];
		if (localizationFile == null)
		{
			localizationFile = (LocalizationFile)(Localization.m_Files[path] = new LocalizationFile(path));
		}
		return localizationFile;
	}
}
