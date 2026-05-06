using System.Reflection;
using Veritas;

namespace UOAIO.Profiles;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class SpeechHues : PersistableObject
{
	public const int Default = 96;

	public static readonly PersistableType TypeCode;

	private int[] m_Hues;

	public override PersistableType TypeID => SpeechHues.TypeCode;

	[Optionable("Regular", "Speech Hues", Default = 96)]
	[OptionHue]
	public int Regular
	{
		get
		{
			return this[SpeechType.Regular];
		}
		set
		{
			this[SpeechType.Regular] = value;
		}
	}

	[Optionable("Yell", "Speech Hues", Default = 96)]
	[OptionHue]
	public int Yell
	{
		get
		{
			return this[SpeechType.Yell];
		}
		set
		{
			this[SpeechType.Yell] = value;
		}
	}

	[Optionable("Emote", "Speech Hues", Default = 96)]
	[OptionHue]
	public int Emote
	{
		get
		{
			return this[SpeechType.Emote];
		}
		set
		{
			this[SpeechType.Emote] = value;
		}
	}

	[Optionable("Whisper", "Speech Hues", Default = 96)]
	[OptionHue]
	public int Whisper
	{
		get
		{
			return this[SpeechType.Whisper];
		}
		set
		{
			this[SpeechType.Whisper] = value;
		}
	}

	public int this[SpeechType speechType]
	{
		get
		{
			return this.m_Hues[(int)speechType];
		}
		set
		{
			this.m_Hues[(int)speechType] = value;
		}
	}

	private static PersistableObject Construct()
	{
		return new SpeechHues();
	}

	public SpeechHues()
	{
		this.m_Hues = new int[6];
		for (int i = 0; i < this.m_Hues.Length; i++)
		{
			this.m_Hues[i] = 96;
		}
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetInt32("regular", this.Regular);
		op.SetInt32("yell", this.Yell);
		op.SetInt32("emote", this.Emote);
		op.SetInt32("whisper", this.Whisper);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.Regular = ip.GetInt32("regular");
		this.Yell = ip.GetInt32("yell");
		this.Emote = ip.GetInt32("emote");
		this.Whisper = ip.GetInt32("whisper");
	}

	static SpeechHues()
	{
		SpeechHues.TypeCode = new PersistableType("speechHues", Construct);
	}
}
