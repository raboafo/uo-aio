using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SharpDX.DirectSound;
using UOAIO.Assets;
using UOAIO.Profiles;

namespace UOAIO;

public class Sounds
{
	private struct SoundRequest
	{
		public int soundId;

		public int x;

		public int y;

		public int z;

		public float volume;

		public float frequency;

		public SoundRequest(int soundId, int x, int y, int z, float volume, float frequency)
		{
			this.soundId = soundId;
			this.x = x;
			this.y = y;
			this.z = z;
			this.volume = volume;
			this.frequency = frequency;
		}
	}

	public const int TableSize = 4096;

	private static DirectSound m_Device;

	private IAudioProvider _audioProvider;

	private BlockingCollection<SoundRequest> queue;

	private Task worker;

	private bool m_Enabled = true;

	private int[] m_SOC_SoundTable = new int[4] { 72, 47, 79, 45 };

	private int[] m_SOC_TransTable = new int[22]
	{
		0, 0, 1, 2, 4, 2, 3, 3, 3, 4,
		4, 4, 1, 3, 3, 3, 3, 1, 3, 3,
		4, 1
	};

	private int[] m_SCC_SoundTable = new int[3] { 88, 46, 44 };

	private int[] m_SCC_TransTable = new int[22]
	{
		0, 0, 1, 0, 3, 0, 2, 2, 2, 3,
		3, 3, 1, 2, 2, 2, 2, 1, 2, 2,
		3, 1
	};

	public static DirectSound Device => Sounds.m_Device;

	public bool Enabled
	{
		get
		{
			return this.m_Enabled;
		}
		set
		{
			this.m_Enabled = value;
		}
	}

	public Sounds()
	{
		try
		{
			Sounds.m_Device = new DirectSound();
			Sounds.m_Device.SetCooperativeLevel(Engine.m_Display.Handle, CooperativeLevel.Priority);
		}
		catch (Exception ex)
		{
			Debug.Trace("Error constructing sound factory");
			Debug.Error(ex);
			Sounds.m_Device = null;
		}
		this._audioProvider = new ManagedAudioProvider(new PhysicalAudioProvider(Sounds.m_Device));
		this.queue = new BlockingCollection<SoundRequest>(new ConcurrentQueue<SoundRequest>());
		this.worker = this.SpawnWorker();
	}

	public void Dispose()
	{
		if (this.queue != null)
		{
			this.queue.CompleteAdding();
			this.worker.Wait();
			this.queue.Dispose();
			this.queue = null;
			this.worker = null;
		}
		if (this._audioProvider != null)
		{
			this._audioProvider.Dispose();
		}
		if (Sounds.m_Device != null)
		{
			Sounds.m_Device.Dispose();
			Sounds.m_Device = null;
		}
	}

	public void PlayContainerOpen(int GumpID)
	{
		if (GumpID == 10851)
		{
			Engine.Sounds.PlaySound(391);
			return;
		}
		GumpID -= 60;
		if (GumpID >= 0 && GumpID <= 21)
		{
			int num = this.m_SOC_TransTable[GumpID];
			if (num < this.m_SOC_SoundTable.Length)
			{
				this.PlaySound(this.m_SOC_SoundTable[num]);
			}
		}
	}

	public void PlayContainerClose(int GumpID)
	{
		if (GumpID == 10851)
		{
			Engine.Sounds.PlaySound(457);
			return;
		}
		GumpID -= 60;
		if (GumpID >= 0 && GumpID <= 21)
		{
			int num = this.m_SCC_TransTable[GumpID];
			if (num < this.m_SCC_SoundTable.Length)
			{
				this.PlaySound(this.m_SCC_SoundTable[num]);
			}
		}
	}

	public void PlaySound(int SoundID, int X = -1, int Y = -1, int Z = -1, float Volume = 0.75f, float Frequency = 0f)
	{
		if (!Preferences.Current.Sound.Volume.Mute)
		{
			this.queue.Add(new SoundRequest
			{
				soundId = SoundID,
				x = X,
				y = Y,
				z = Z,
				volume = Volume,
				frequency = Frequency
			});
		}
	}

	private Task SpawnWorker()
	{
		return Task.Factory.StartNew(delegate
		{
			foreach (SoundRequest item in this.queue.GetConsumingEnumerable())
			{
				this.PlaySoundCore(item);
			}
		});
	}

	private void PlaySoundCore(SoundRequest req)
	{
		if (Sounds.m_Device == null || !this.m_Enabled)
		{
			return;
		}
		try
		{
			SecondarySoundBuffer secondarySoundBuffer = this._audioProvider.Acquire(req.soundId);
			if (secondarySoundBuffer == null)
			{
				return;
			}
			Mobile player = World.Player;
			if ((req.x == -1 && req.y == -1 && req.z == -1) || player == null)
			{
				try
				{
					secondarySoundBuffer.Pan = 0;
				}
				catch
				{
				}
				int value = Preferences.Current.Sound.Volume.Value;
				value -= (int)(5000f * (1f - req.volume));
				value -= 10000;
				if (value > 0)
				{
					value = 0;
				}
				else if (value < -10000)
				{
					value = -10000;
				}
				try
				{
					secondarySoundBuffer.Volume = value;
				}
				catch
				{
				}
			}
			else
			{
				int num = Math.Abs((req.x - player.X) * 11);
				int num2 = Math.Abs((req.y - player.Y) * 11);
				int num3 = Math.Abs(req.z - player.Z);
				int num4 = (int)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
				int num5 = req.x - req.y - (player.X - player.Y);
				num5 *= 350;
				num4 *= 10;
				num4 = -num4;
				num4 -= (int)(5000f * (1f - req.volume));
				int value2 = Preferences.Current.Sound.Volume.Value;
				num4 += 10000;
				num4 = num4 * value2 / 10000;
				num4 -= 10000;
				if (num4 > 0)
				{
					num4 = 0;
				}
				else if (num4 < -10000)
				{
					num4 = -10000;
				}
				if (num5 > 10000)
				{
					num5 = 10000;
				}
				else if (num5 < -10000)
				{
					num5 = -10000;
				}
				try
				{
					secondarySoundBuffer.Pan = num5;
				}
				catch
				{
				}
				try
				{
					secondarySoundBuffer.Volume = num4;
				}
				catch
				{
				}
			}
			try
			{
				secondarySoundBuffer.Frequency = (int)(22048f * (1f + req.frequency));
			}
			catch
			{
			}
			secondarySoundBuffer.CurrentPosition = 0;
			secondarySoundBuffer.Play(0, PlayFlags.None);
		}
		catch (Exception ex)
		{
			Debug.Error(ex);
		}
	}
}
