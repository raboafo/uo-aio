using System.Reflection;
using Veritas;

namespace UOAIO.Profiles;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class Preferences : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private FootstepData m_Footsteps;

	private SoundData m_Sound;

	private MusicData m_Music;

	private SpeechHues m_SpeechHues;

	private NotorietyHues m_NotorietyHues;

	private Options m_Options;

	private ScavengerAgent m_Scavenger;

	private ScreenLayout m_Layout;

	private RenderSettings _renderSettings;

	public override PersistableType TypeID => Preferences.TypeCode;

	public static Preferences Current => Profile.Current.Preferences;

	[Optionable("Footsteps", "Audio", Default = true)]
	public bool FootstepsEnabled
	{
		get
		{
			return !this.m_Footsteps.Volume.Mute;
		}
		set
		{
			this.m_Footsteps.Volume.Mute = !value;
		}
	}

	[Optionable("Sound", "Audio", Default = true)]
	public bool SoundEnabled
	{
		get
		{
			return !this.m_Sound.Volume.Mute;
		}
		set
		{
			this.m_Sound.Volume.Mute = !value;
		}
	}

	[Optionable("Music", "Audio", Default = true)]
	public bool MusicEnabled
	{
		get
		{
			return !this.m_Music.Volume.Mute;
		}
		set
		{
			this.m_Music.Volume.Mute = !value;
		}
	}

	public FootstepData Footsteps => this.m_Footsteps;

	public SoundData Sound => this.m_Sound;

	public MusicData Music => this.m_Music;

	[Optionable("Speech Hues")]
	public SpeechHues SpeechHues => this.m_SpeechHues;

	[Optionable("Notoriety Hues")]
	public NotorietyHues NotorietyHues => this.m_NotorietyHues;

	[Optionable("Options")]
	public Options Options => this.m_Options;

	[Optionable("Scavenger")]
	public ScavengerAgent Scavenger => this.m_Scavenger;

	[Optionable("Macros", "Configuration")]
	[MacroEditor]
	public string Macros
	{
		get
		{
			return "...";
		}
		set
		{
		}
	}

	[Optionable("Display Settings", "Configuration")]
	[RenderSettingEditor]
	public string DisplaySettings
	{
		get
		{
			return "...";
		}
		set
		{
		}
	}

	public RenderSettings RenderSettings => this._renderSettings;

	public ScreenLayout Layout => this.m_Layout;

	[Optionable("Restock Agent")]
	public RestockAgent RestockAgent
	{
		get
		{
			if (Player.Current == null)
			{
				return null;
			}
			return Player.Current.RestockAgent;
		}
	}

	private static PersistableObject Construct()
	{
		return new Preferences(isLoading: true);
	}

	public Preferences()
		: this(isLoading: false)
	{
	}

	private Preferences(bool isLoading)
	{
		if (!isLoading)
		{
			this.m_Footsteps = new FootstepData();
			this.m_Sound = new SoundData();
			this.m_Music = new MusicData();
			this.m_SpeechHues = new SpeechHues();
			this.m_NotorietyHues = new NotorietyHues();
			this.m_Options = new Options();
			this.m_Scavenger = new ScavengerAgent();
			this.m_Layout = new ScreenLayout();
			this._renderSettings = new RenderSettings();
		}
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		this._renderSettings.Serialize(op);
		if (this.m_Footsteps == null)
		{
			this.m_Footsteps = new FootstepData();
		}
		this.m_Footsteps.Serialize(op);
		this.m_Sound.Serialize(op);
		this.m_Music.Serialize(op);
		this.m_SpeechHues.Serialize(op);
		this.m_NotorietyHues.Serialize(op);
		this.m_Options.Serialize(op);
		this.m_Scavenger.Serialize(op);
		this.m_Layout.Serialize(op);
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			object child = ip.GetChild();
			if (child is FootstepData)
			{
				this.m_Footsteps = child as FootstepData;
			}
			else if (child is SoundData)
			{
				this.m_Sound = child as SoundData;
			}
			else if (child is MusicData)
			{
				this.m_Music = child as MusicData;
			}
			else if (child is SpeechHues)
			{
				this.m_SpeechHues = child as SpeechHues;
			}
			else if (child is NotorietyHues)
			{
				this.m_NotorietyHues = child as NotorietyHues;
			}
			else if (child is Options)
			{
				this.m_Options = child as Options;
			}
			else if (child is ScreenLayout)
			{
				this.m_Layout = child as ScreenLayout;
			}
			else if (child is ScavengerAgent)
			{
				this.m_Scavenger = child as ScavengerAgent;
			}
			else if (child is RenderSettings)
			{
				this._renderSettings = child as RenderSettings;
			}
		}
		if (this.m_Scavenger == null)
		{
			this.m_Scavenger = new ScavengerAgent();
		}
		if (this._renderSettings == null)
		{
			this._renderSettings = new RenderSettings();
		}
	}

	static Preferences()
	{
		Preferences.TypeCode = new PersistableType("preferences", Construct, FootstepData.TypeCode, SoundData.TypeCode, MusicData.TypeCode, SpeechHues.TypeCode, NotorietyHues.TypeCode, ScreenLayout.TypeCode, Options.TypeCode, ScavengerAgent.TypeCode, RenderSettings.TypeCode, RestockAgent.TypeCode);
	}
}
