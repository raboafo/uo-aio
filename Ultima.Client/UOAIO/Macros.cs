using System;
using System.IO;
using System.Windows.Forms;
using Veritas;

namespace UOAIO;

public class Macros
{
	public static MacroConfig m_Config;

	public static MacroSet m_Current;

	private static MacroCollection m_Running;

	private const string RelativeUserDataPath = "config/Macros.xml";

	private const string RelativeLegacyPath = "data/ultima/macros/macros.xml";

	private const string RelativeArchivePath = "play/macros/macros.xml";

	public static MacroConfig Config
	{
		get
		{
			if (Macros.m_Config == null)
			{
				Macros.m_Config = Macros.LoadConfig();
			}
			return Macros.m_Config;
		}
		set
		{
			Macros.m_Config = value;
		}
	}

	public static MacroSet Current
	{
		get
		{
			if (Macros.m_Current == null)
			{
				Macros.m_Current = Macros.FindCurrent();
			}
			return Macros.m_Current;
		}
		set
		{
			Macros.m_Current = value;
		}
	}

	public static MacroCollection List => Macros.Current.Macros;

	public static MacroCollection Running => Macros.m_Running;

	public static void Reset()
	{
		Macros.StopAll();
		Macros.m_Current = null;
		Macros.m_Current = Macros.Current;
	}

	private static MacroSet FindCurrent(MacroConfig config, Mobile mob)
	{
		int serial = mob?.Serial ?? 0;
		int server = ((mob != null) ? ((Engine.m_ServerName != null) ? Engine.m_ServerName.GetHashCode() : 0) : 0);
		MacroSet macroSet = config[serial, server];
		if (macroSet == null && (mob == null || Macros.Exists(Macros.GetMobilePath(mob))))
		{
			macroSet = Macros.LoadTextMacroSet(mob);
			macroSet.Serial = serial;
			macroSet.Server = server;
			config.MacroSets.Add(macroSet);
			Macros.Save();
		}
		return macroSet;
	}

	public static MacroSet FindCurrent()
	{
		MacroConfig config = Macros.Config;
		MacroSet macroSet = null;
		Mobile player = World.Player;
		macroSet = Macros.FindCurrent(config, player);
		if (macroSet == null && player != null)
		{
			macroSet = Macros.FindCurrent(config, null);
		}
		return macroSet;
	}

	private static string GetConfigurationPath()
	{
		string text = ClientRuntimeEnvironment.RuntimeDataPath("config/Macros.xml");
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(text));
		if (!directoryInfo.Exists)
		{
			directoryInfo.Create();
		}
		return text;
	}

	private static MacroConfig LoadConfig()
	{
		MacroConfig macroConfig = new MacroConfig();
		string configurationPath = Macros.GetConfigurationPath();
		if (!File.Exists(configurationPath))
		{
			string text = Engine.FileManager.BasePath("data/ultima/macros/macros.xml");
			if (File.Exists(text))
			{
				try
				{
					File.Move(text, configurationPath);
				}
				catch
				{
					File.Copy(text, configurationPath, overwrite: false);
				}
			}
			else
			{
				ArchivedFile archivedFile = Engine.FileManager.GetArchivedFile("play/macros/macros.xml");
				if (archivedFile != null)
				{
					using FileStream output = new FileStream(configurationPath, FileMode.Create, FileAccess.Write, FileShare.None);
					archivedFile.Download(output);
				}
			}
		}
		if (File.Exists(configurationPath))
		{
			using FileStream stream = new FileStream(configurationPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			XmlPersistanceReader xmlPersistanceReader = new XmlPersistanceReader(stream);
			xmlPersistanceReader.ReadDocument(macroConfig);
			xmlPersistanceReader.Close();
		}
		return macroConfig;
	}

	public static bool Start(Keys key)
	{
		MacroSet current = Macros.Current;
		foreach (Macro macro in current.Macros)
		{
			if (!macro.CheckKey(key))
			{
				continue;
			}
			if (macro.Running)
			{
				macro.Stop();
			}
			macro.Start();
			return true;
		}
		return false;
	}

	public static void StopAll()
	{
		while (Macros.m_Running.Count > 0)
		{
			Macros.m_Running[0].Stop();
		}
	}

	public static void Slice()
	{
		if (Macros.m_Running == null)
		{
			return;
		}
		for (int i = 0; i < Macros.m_Running.Count; i++)
		{
			Macro macro = Macros.m_Running[i];
			if (!macro.Slice())
			{
				Macros.m_Running.RemoveAt(i--);
			}
		}
	}

	public static string ReadLine(StreamReader ip)
	{
		string text;
		while ((text = ip.ReadLine()) != null)
		{
			text = text.Trim();
			if (text.Length != 0 && !text.StartsWith(";"))
			{
				break;
			}
		}
		return text;
	}

	public static void Skip(string line, StreamReader ip)
	{
		Debug.Trace("Skipping improperly formatted line in macros.txt: {0}", line);
		do
		{
			line = line.Trim();
		}
		while (!line.StartsWith("#") && (line = ip.ReadLine()) != null);
	}

	public static void Cleanup()
	{
		MacroSet current = Macros.Current;
		MacroCollection macros = current.Macros;
		for (int num = macros.Count - 1; num >= 0; num--)
		{
			if (macros[num].Actions.Count == 0)
			{
				macros.RemoveAt(num);
			}
		}
	}

	public static void Save()
	{
		XmlPersistanceWriter.SaveObject(Macros.Config, Macros.GetConfigurationPath());
	}

	public static string GetMobilePath(Mobile mob)
	{
		string text = Engine.m_ServerName;
		if (text == null)
		{
			text = "";
		}
		return (text.GetHashCode() ^ mob.Serial).ToString("X8");
	}

	public static bool Exists(string filename)
	{
		string path = Engine.FileManager.BasePath($"data/ultima/macros/{filename}.txt");
		return File.Exists(path);
	}

	public static MacroSet LoadTextMacroSet(Mobile mob)
	{
		if (mob != null)
		{
			string mobilePath = Macros.GetMobilePath(mob);
			if (Macros.Exists(mobilePath))
			{
				return Macros.LoadTextMacroSet(mobilePath);
			}
		}
		return Macros.LoadTextMacroSet("Macros");
	}

	public static MacroSet LoadTextMacroSet(string fileName)
	{
		MacroSet macroSet = new MacroSet();
		string path = Engine.FileManager.BasePath($"data/ultima/macros/{fileName}.txt");
		if (File.Exists(path))
		{
			using StreamReader ip = new StreamReader(path);
			while (true)
			{
				string text = Macros.ReadLine(ip);
				if (text == null)
				{
					break;
				}
				if (text.Length != 5)
				{
					Macros.Skip(text, ip);
					break;
				}
				string[] array = text.Split(' ');
				if (array.Length != 3)
				{
					Macros.Skip(text, ip);
					break;
				}
				bool flag = true;
				int num = 0;
				while (flag && num < array.Length)
				{
					flag = array[num] == "0" || array[num] == "1";
					num++;
				}
				if (!flag)
				{
					Macros.Skip(text, ip);
					break;
				}
				Keys keys = Keys.None;
				if (array[0] != "0")
				{
					keys |= Keys.Control;
				}
				if (array[1] != "0")
				{
					keys |= Keys.Alt;
				}
				if (array[2] != "0")
				{
					keys |= Keys.Shift;
				}
				string text2 = Macros.ReadLine(ip);
				if (text2 == null)
				{
					break;
				}
				Keys keys2 = Keys.Modifiers | Keys.KeyCode;
				switch (text2)
				{
				case "WheelUp":
				case "Wheel Up":
					keys2 = (Keys)69632;
					break;
				case "WheelDown":
				case "Wheel Down":
					keys2 = (Keys)69633;
					break;
				case "WheelPress":
				case "Wheel Press":
					keys2 = (Keys)69634;
					break;
				default:
					try
					{
						keys2 = (Keys)Enum.Parse(typeof(Keys), text2, ignoreCase: true);
					}
					catch
					{
					}
					break;
				}
				if (keys2 == (Keys.Modifiers | Keys.KeyCode))
				{
					Macros.Skip(text2, ip);
					break;
				}
				MacroData macroData = new MacroData();
				macroData.Key = keys2;
				macroData.Mods = keys;
				string text3;
				while ((text3 = Macros.ReadLine(ip)) != null && !text3.StartsWith("#"))
				{
					int num2 = text3.IndexOf(' ');
					Action action = ((num2 < 0) ? new Action(text3, "") : new Action(text3.Substring(0, num2), text3.Substring(num2 + 1)));
					if (action.Handler == null)
					{
						Debug.Trace("Bad macro action: {0}", text3);
					}
					macroData.Actions.Add(action);
				}
				macroSet.Macros.Add(new Macro(macroData));
			}
		}
		return macroSet;
	}

	static Macros()
	{
		Macros.m_Running = new MacroCollection();
	}
}
