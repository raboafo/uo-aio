using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace UOAIO.Videos;

public sealed class Playback : IDisposable
{
	public class FadeEffect : Fade
	{
		private Playback _video;

		public FadeEffect(Playback video)
			: base(0, 0f, 1f, 2f)
		{
			this._video = video;
		}

		protected internal override void OnFadeComplete()
		{
			this._video.BeginStreaming();
			Engine.Effects.Add(new MiddleFadeEffect(this._video, new string[5]
			{
				"",
				this._video.ServerName,
				"",
				this._video.TimeStamp.ToLocalTime().ToString("MMMM dd, yyyy"),
				""
			}, 0));
		}
	}

	public class MiddleFadeEffect : Fade
	{
		private Playback _video;

		private GLabel m_Label;

		private string[] _text;

		private int _index;

		private int gx;

		private int gy;

		public MiddleFadeEffect(Playback video, string[] text, int index)
			: base(0, 1f, 1f, (text[index] == "") ? 1f : 4f)
		{
			this._video = video;
			this._text = text;
			this._index = index;
			this.m_Label = new GLabel(this._text[this._index], Engine.GetFont(4), Hues.Load(33921), 0, 0);
			this.m_Label.Alpha = 0f;
			this.m_Label.X = (Engine.GameWidth - this.m_Label.Width) / 2;
			this.m_Label.Y = (Engine.GameHeight - this.m_Label.Height) / 2;
			Gumps.Desktop.Children.Add(this.m_Label);
		}

		public override bool Evaluate(ref double Alpha)
		{
			int num = Engine.GameX - this.gx;
			int num2 = Engine.GameY - this.gy;
			this.m_Label.X += num;
			this.m_Label.Y += num2;
			this.gx += num;
			this.gy += num2;
			double normalized = base.m_Sync.Normalized;
			if (normalized < 0.35)
			{
				this.m_Label.Alpha = (float)(normalized / 0.35);
			}
			else if (normalized < 0.7)
			{
				this.m_Label.Alpha = 1f;
			}
			else if (normalized < 1.0)
			{
				this.m_Label.Alpha = (float)((1.0 - normalized) / 0.30000000000000004);
			}
			else
			{
				this.m_Label.Alpha = 0f;
			}
			return base.Evaluate(ref Alpha);
		}

		protected internal override void OnFadeComplete()
		{
			Gumps.Destroy(this.m_Label);
			this._index++;
			if (this._index < this._text.Length)
			{
				Engine.Effects.Add(new MiddleFadeEffect(this._video, this._text, this._index));
				return;
			}
			Cursor.Visible = true;
			this._video._stopwatch.Start();
			Engine.Effects.Add(new EndDeathEffect());
		}
	}

	public class EndDeathEffect : Fade
	{
		public EndDeathEffect()
			: base(0, 1f, 0f, 1f)
		{
		}

		protected internal override void OnFadeComplete()
		{
		}
	}

	private static Playback _video;

	private const int Version = 4;

	private GZBlockIn _stream;

	private int _version;

	private byte[] _hashCode;

	private DateTime _timeStamp;

	private TimeSpan _duration;

	private string _playerName;

	private string _serverName;

	private IPAddress _ipAddress;

	private Stopwatch _stopwatch;

	private PlaybackState _state;

	private long _origin;

	private int _elapsed;

	private bool _isStreaming;

	private int _next;

	private bool _hasNext;

	private int _last;

	private int _speedBase;

	private int _speedNumerator = 1;

	private int _speedDenominator = 1;

	public static Playback Video
	{
		get
		{
			return Playback._video;
		}
		set
		{
			if (Playback._video != null)
			{
				if (Playback._video != null)
				{
					Playback._video.Dispose();
				}
				Playback._video = value;
			}
		}
	}

	public static bool Active => Playback._video != null && Playback._video.State > PlaybackState.Stopped;

	public byte[] HashCode => this._hashCode;

	public DateTime TimeStamp => this._timeStamp;

	public TimeSpan Duration => this._duration;

	public string PlayerName => this._playerName;

	public string ServerName => this._serverName;

	public IPAddress IpAddress => this._ipAddress;

	public TimeSpan Elapsed => TimeSpan.FromMilliseconds(this._elapsed);

	public bool IsPlaying => this._state == PlaybackState.Playing;

	public bool IsPaused => this._state == PlaybackState.Paused;

	public PlaybackState State => this._state;

	public static void Download(Uri uri)
	{
		WebRequest req = WebRequest.Create(uri);
		req.BeginGetResponse(delegate(IAsyncResult asyncResult)
		{
			try
			{
				MemoryStream memoryStream = new MemoryStream();
				using (WebResponse webResponse = req.EndGetResponse(asyncResult))
				{
					Stream responseStream = webResponse.GetResponseStream();
					byte[] array = new byte[2048];
					int count;
					while ((count = responseStream.Read(array, 0, array.Length)) > 0)
					{
						memoryStream.Write(array, 0, count);
					}
					memoryStream.Seek(0L, SeekOrigin.Begin);
				}
				Playback video = new Playback(memoryStream);
				Engine.AddTextMessage("Download complete.");
				VideoPlaybackGump toAdd = new VideoPlaybackGump(video);
				Gumps.Desktop.Children.Add(toAdd);
				Playback._video = video;
			}
			catch (Exception ex)
			{
				Engine.AddTextMessage(ex.ToString());
			}
		}, null);
	}

	public Playback(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		this._stopwatch = new Stopwatch();
		this.Open(stream);
	}

	public void Play()
	{
		if (this._state == PlaybackState.Stopped)
		{
			this.Start();
			this._state = PlaybackState.Playing;
		}
	}

	public void Pause()
	{
		if (this._state == PlaybackState.Playing)
		{
			this._stopwatch.Stop();
			this._state = PlaybackState.Paused;
		}
		else if (this._state == PlaybackState.Paused)
		{
			this._stopwatch.Start();
			this._state = PlaybackState.Playing;
		}
	}

	public void Start()
	{
		Cursor.Visible = false;
		Engine.Effects.Add(new FadeEffect(this));
	}

	private void BeginStreaming()
	{
		Debug.Trace($"{this._stopwatch}");
		Debug.Trace($"{this._stream}");
		if (this._stream != null)
		{
			this._isStreaming = true;
			this._stream.Seek(this._origin, SeekOrigin.Begin);
			this.ReadInitialState();
		}
	}

	public void Stop()
	{
		this._stopwatch.Stop();
		this._state = PlaybackState.Stopped;
		this.Dispose();
		if (Playback._video == this)
		{
			Playback._video = null;
		}
	}

	private int GetTimeSample()
	{
		return this._speedBase + (int)(this._stopwatch.ElapsedMilliseconds * this._speedNumerator / this._speedDenominator);
	}

	public void UpdateSpeed(int numerator, int denominator)
	{
		if (this._speedNumerator != numerator || this._speedDenominator != denominator)
		{
			this._speedBase = this.GetTimeSample();
			this._speedNumerator = numerator;
			this._speedDenominator = denominator;
			this._stopwatch.Reset();
			this._stopwatch.Start();
		}
	}

	public void Cycle()
	{
		if (!this._isStreaming)
		{
			return;
		}
		while (true)
		{
			if (!this._hasNext)
			{
				if (this._stream.EndOfFile)
				{
					this.Stop();
					break;
				}
				this._next = this._last + this._stream.Compressed.ReadInt32();
				this._last = this._next;
				this._hasNext = true;
			}
			this._elapsed = Math.Max(this.GetTimeSample(), this._elapsed);
			if (this._elapsed < this._next)
			{
				break;
			}
			this._hasNext = false;
			int num = this._stream.Compressed.ReadByte();
			if (num >= 0 && num < 255)
			{
				PacketHandler packetHandler = PacketHandlers.Registry.Get(num);
				if (packetHandler != null)
				{
					int num2 = packetHandler.Length;
					if (num2 < 0)
					{
						num2 = this._stream.Compressed.ReadByte();
						num2 <<= 8;
						num2 |= this._stream.Compressed.ReadByte();
					}
					byte[] array = new byte[num2];
					int num3 = 0;
					array[num3++] = (byte)num;
					if (packetHandler.Length < 0)
					{
						array[num3++] = (byte)(num2 >> 8);
						array[num3++] = (byte)num2;
					}
					this._stream.Compressed.Read(array, num3, num2 - num3);
					packetHandler.Handle(new PacketReader(array, 0, array.Length, packetHandler.Length >= 0, (byte)num, packetHandler.Callback.Method.Name));
				}
				else
				{
					Debug.Trace("Bad packet {{ packetID: 0x{0:X2}; }}", num);
					try
					{
						while (!this._stream.EndOfFile)
						{
							int num4 = this._stream.Compressed.ReadInt32();
							if (num4 < 1000)
							{
								this._hasNext = true;
								this._next = this._elapsed + num4;
							}
							else
							{
								this._stream.Seek(-3L, SeekOrigin.Current);
							}
						}
					}
					catch
					{
						this.Stop();
						break;
					}
				}
				bool flag = true;
				continue;
			}
			this.Stop();
			break;
		}
	}

	private void ReadInitialState()
	{
		int num = this._stream.Compressed.ReadInt32();
		num += (int)this._stream.Position;
		World.Clear();
		Map.Invalidate();
		Mobile mobile = this.ReadMobile(this._stream.Compressed, isPlayer: true);
		while (this._stream.Position < num)
		{
			switch (this._stream.Compressed.ReadByte())
			{
			case 0:
			{
				Item item = this.ReadItem(this._stream.Compressed);
				break;
			}
			case 1:
			{
				Mobile mobile2 = this.ReadMobile(this._stream.Compressed, isPlayer: false);
				break;
			}
			}
		}
	}

	private Mobile ReadMobile(BinaryReader ip, bool isPlayer)
	{
		Mobile mobile = World.WantMobile(ip.ReadInt32());
		if (isPlayer)
		{
			World.Serial = mobile.Serial;
		}
		int x = ip.ReadInt32();
		int y = ip.ReadInt32();
		int z = ip.ReadInt32();
		if (isPlayer)
		{
			World.SetLocation(x, y, z);
		}
		mobile.SetLocation(World.Agent, x, y, z);
		mobile.Refresh = true;
		mobile.Hue = ip.ReadUInt16();
		mobile.Body = ip.ReadUInt16();
		mobile.Direction = ip.ReadByte();
		mobile.Name = ip.ReadString();
		mobile.Notoriety = (Notoriety)ip.ReadByte();
		mobile.Flags.Value = ip.ReadByte();
		mobile.MaximumHitPoints = ip.ReadUInt16();
		mobile.CurrentHitPoints = ip.ReadUInt16();
		mobile.UpdateReal();
		byte b = ip.ReadByte();
		int num = ip.ReadInt32();
		while (num-- > 0)
		{
			ip.ReadUInt32();
		}
		if (isPlayer)
		{
			mobile.Strength = ip.ReadUInt16();
			mobile.Dexterity = ip.ReadUInt16();
			mobile.Intelligence = ip.ReadUInt16();
			mobile.MaximumStamina = ip.ReadUInt16();
			mobile.CurrentStamina = ip.ReadUInt16();
			mobile.MaximumMana = ip.ReadUInt16();
			mobile.CurrentMana = ip.ReadUInt16();
			ip.ReadByte();
			ip.ReadByte();
			ip.ReadByte();
			mobile.Gold = ip.ReadInt32();
			mobile.Weight = ip.ReadUInt16();
			if (this._version < 4)
			{
				throw new InvalidOperationException();
			}
			num = ip.ReadByte();
			for (int i = 0; i < num; i++)
			{
				Skill skill = Engine.Skills[i];
				if (skill != null)
				{
					skill.Real = (float)(int)ip.ReadUInt16() / 10f;
					ip.ReadUInt16();
					skill.Value = (float)(int)ip.ReadUInt16() / 10f;
					skill.Lock = (SkillLock)ip.ReadByte();
				}
				else
				{
					ip.ReadInt16();
					ip.ReadInt16();
					ip.ReadInt16();
					ip.ReadByte();
				}
			}
			mobile.Armor = ip.ReadUInt16();
			mobile.StatCap = ip.ReadUInt16();
			mobile.FollowersCur = ip.ReadByte();
			mobile.FollowersMax = ip.ReadByte();
			mobile.TithingPoints = ip.ReadInt32();
			mobile.LightLevel = ip.ReadSByte();
			Engine.Effects.GlobalLight = ip.ReadByte();
			ip.ReadUInt16();
			ip.ReadByte();
			num = ip.ReadByte();
			while (num-- > 0)
			{
				ip.ReadInt32();
			}
		}
		mobile.Refresh = false;
		mobile.Update();
		return mobile;
	}

	private Item ReadItem(BinaryReader ip)
	{
		Item item = World.WantItem(ip.ReadInt32());
		int x = ip.ReadInt32();
		int y = ip.ReadInt32();
		int z = ip.ReadInt32();
		item.Hue = ip.ReadUInt16();
		item.ID = ip.ReadInt16();
		item.Amount = ip.ReadInt16();
		item.Direction = ip.ReadByte();
		item.Flags.Value = ip.ReadByte();
		item.Layer = (Layer)ip.ReadByte();
		ip.ReadString();
		int num = ip.ReadInt32();
		Agent parent = World.Agent;
		if (num >= 1073741824)
		{
			parent = World.WantItem(num);
		}
		else if (num >= 1)
		{
			parent = World.WantMobile(num);
		}
		item.SetLocation(parent, x, y, z);
		int num2 = ip.ReadInt32();
		while (num2-- > 0)
		{
			ip.ReadUInt32();
		}
		if (this._version > 2 && ip.ReadInt32() != 0)
		{
			ip.ReadBytes(ip.ReadUInt16());
		}
		return item;
	}

	private void Open(Stream stream)
	{
		this._stream = new GZBlockIn(stream);
		this._version = this._stream.Raw.ReadByte();
		if (this._version > 4)
		{
			this.Dispose();
			throw new VersionMismatchException();
		}
		this._stream.IsCompressed = this._version > 1;
		this._hashCode = this._stream.Raw.ReadBytes(16);
		this._timeStamp = DateTime.FromFileTime(this._stream.Raw.ReadInt64());
		this._duration = TimeSpan.FromMilliseconds(this._stream.Raw.ReadInt32());
		this._playerName = this._stream.Compressed.ReadString();
		this._serverName = this._stream.Compressed.ReadString();
		if (this._version > 1)
		{
			this._ipAddress = new IPAddress(this._stream.Compressed.ReadUInt32());
		}
		this._origin = this._stream.Position;
		long position = this._stream.RawStream.Position;
		this._stream.RawStream.Seek(17L, SeekOrigin.Begin);
		using (MD5 mD = MD5.Create())
		{
			byte[] array = mD.ComputeHash(this._stream.RawStream);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != this.HashCode[i])
				{
					this.Dispose();
					throw new HashCodeMismatchException();
				}
			}
		}
		this._stream.RawStream.Seek(position, SeekOrigin.Begin);
	}

	public void Dispose()
	{
		if (this._stream != null)
		{
			this._stream.Close();
			this._stream = null;
		}
	}
}
