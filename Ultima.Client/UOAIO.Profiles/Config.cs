using System.IO;
using UOAIO;
using Veritas;

namespace UOAIO.Profiles;

public class Config : PersistableObject
{
	public static readonly PersistableType TypeCode;

	public static readonly Config Current;

	private ProfileList m_Profiles;

	private ServerList m_Servers;

	private const string RelativeUserDataPath = "config/Configuration.xml";

	private const string RelativeLegacyPath = "config.xml";

	public override PersistableType TypeID => Config.TypeCode;

	public ProfileList Profiles => this.m_Profiles;

	public ServerList Servers => this.m_Servers;

	private static PersistableObject Construct()
	{
		return new Config();
	}

	public Config()
	{
		this.m_Profiles = new ProfileList();
		this.m_Servers = new ServerList();
		this.Load();
	}

	private static string GetConfigurationPath()
	{
		string text = ClientRuntimeEnvironment.RuntimeDataPath("config/Configuration.xml");
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(text));
		if (!directoryInfo.Exists)
		{
			directoryInfo.Create();
		}
		return text;
	}

	public void Load()
	{
		string configurationPath = Config.GetConfigurationPath();
		if (!File.Exists(configurationPath))
		{
			string text = Engine.FileManager.BasePath("config.xml");
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
		}
		if (File.Exists(configurationPath))
		{
			using (FileStream stream = new FileStream(configurationPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				XmlPersistanceReader xmlPersistanceReader = new XmlPersistanceReader(stream);
				xmlPersistanceReader.ReadDocument(this);
				xmlPersistanceReader.Close();
			}
		}
	}

	public void Save()
	{
		XmlPersistanceWriter xmlPersistanceWriter = new XmlPersistanceWriter(Config.GetConfigurationPath());
		xmlPersistanceWriter.WriteDocument(this);
		xmlPersistanceWriter.Close();
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		this.m_Profiles.Serialize(op);
		this.m_Servers.Serialize(op);
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			object child = ip.GetChild();
			if (child is ProfileList)
			{
				this.m_Profiles = child as ProfileList;
			}
			else if (child is ServerList)
			{
				this.m_Servers = child as ServerList;
			}
		}
	}

	static Config()
	{
		Config.TypeCode = new PersistableType("config", Construct, ProfileList.TypeCode, ServerList.TypeCode);
		Config.Current = new Config();
	}
}
