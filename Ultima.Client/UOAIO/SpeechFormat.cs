using UOAIO.Profiles;

namespace UOAIO;

public class SpeechFormat
{
	public static readonly SpeechFormat Client;

	public static readonly SpeechFormat Yell;

	public static readonly SpeechFormat Emote;

	public static readonly SpeechFormat Whisper;

	public static readonly SpeechFormat Guild;

	public static readonly SpeechFormat Alliance;

	public static readonly SpeechFormat Party;

	public static readonly SpeechFormat Regular;

	public static readonly SpeechFormat[] Formats;

	protected string m_Prepend;

	protected string m_Prefix;

	protected string m_Format;

	protected byte m_MessageType;

	protected SpeechType m_SpeechType;

	public virtual int Hue => Preferences.Current.SpeechHues[this.m_SpeechType];

	public byte MessageType => this.m_MessageType;

	public SpeechType SpeechType => this.m_SpeechType;

	public SpeechFormat(string prepend, string prefix, string format, byte messageType, SpeechType speechType)
	{
		this.m_Prepend = prepend;
		this.m_Prefix = prefix;
		this.m_Format = format;
		this.m_MessageType = messageType;
		this.m_SpeechType = speechType;
	}

	public static SpeechFormat Find(string text)
	{
		for (int i = 0; i < SpeechFormat.Formats.Length; i++)
		{
			if (SpeechFormat.Formats[i].IsMatch(text))
			{
				return SpeechFormat.Formats[i];
			}
		}
		return SpeechFormat.Formats[SpeechFormat.Formats.Length - 1];
	}

	public virtual void OnSpeech(string text)
	{
		this.Invoke(this.Mutate(text, display: false));
	}

	public virtual void Invoke(string text)
	{
		Network.Send(new PUnicodeSpeech(text, getKeywords: true, this));
	}

	public virtual bool IsMatch(string text)
	{
		return this.m_Prefix == null || text.StartsWith(this.m_Prefix);
	}

	public virtual string Mutate(string text, bool display)
	{
		if (this.m_Prefix != null)
		{
			text = text.Substring(this.m_Prefix.Length);
		}
		if (!display && this.m_Format != null)
		{
			text = string.Format(this.m_Format, text);
		}
		else if (display)
		{
			text = this.m_Prepend + text + "_";
		}
		return text;
	}

	static SpeechFormat()
	{
		SpeechFormat.Client = new ClientFormat("Client: ", ". ", null, 0, SpeechType.Regular);
		SpeechFormat.Yell = new SpeechFormat("Yell: ", "! ", null, 9, SpeechType.Yell);
		SpeechFormat.Emote = new SpeechFormat("Emote: ", ": ", "*{0}*", 2, SpeechType.Emote);
		SpeechFormat.Whisper = new SpeechFormat("Whisper: ", "; ", null, 8, SpeechType.Whisper);
		SpeechFormat.Guild = new SpeechFormat("Guild: ", "\\ ", null, 13, SpeechType.Guild);
		SpeechFormat.Alliance = new SpeechFormat("Alliance: ", "| ", null, 14, SpeechType.Alliance);
		SpeechFormat.Party = new PartyFormat("Party: ", "/", null, 0, SpeechType.Regular);
		SpeechFormat.Regular = new SpeechFormat(null, null, null, 0, SpeechType.Regular);
		SpeechFormat.Formats = new SpeechFormat[8]
		{
			SpeechFormat.Client,
			SpeechFormat.Yell,
			SpeechFormat.Emote,
			SpeechFormat.Whisper,
			SpeechFormat.Guild,
			SpeechFormat.Alliance,
			SpeechFormat.Party,
			SpeechFormat.Regular
		};
	}
}
