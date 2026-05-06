using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.IO;
using SharpDX.MediaFoundation;
using SharpDX.XAudio2;
using UOAIO.Profiles;

namespace UOAIO;

public class Music
{
	private static XAudio2 m_XAudio2;

	private static MasteringVoice m_MasteringVoice;

	private static NativeFileStream m_FileStream;

	private static AudioDecoder m_AudioDecoder;

	private static SourceVoice m_SourceVoice;

	private static AudioBuffer[] m_AudioBuffers;

	private static DataPointer[] m_MemoryBuffers;

	private static PMediaPlayerState m_State;

	private static int m_PlayCounter;

	private static AutoResetEvent m_BufferEndEvent;

	private static ManualResetEvent m_PlayEvent;

	private static ManualResetEvent m_WaitForPlayToOutput;

	private static Stopwatch m_Clock;

	private static TimeSpan m_PlayPosition;

	private static TimeSpan m_NextPlayPosition;

	private static TimeSpan m_PlayPositionStart;

	private static Task m_PlayingTask;

	private const int WaitPrecision = 1;

	private static string m_FileName;

	public static TimeSpan Length => Music.m_AudioDecoder.Duration;

	public static bool Playing => Music.m_State.HasFlag(PMediaPlayerState.Playing);

	public static bool Stopped => Music.m_State.HasFlag(PMediaPlayerState.Stopped);

	public static bool Paused => Music.m_State.HasFlag(PMediaPlayerState.Paused);

	public static PMediaPlayerState State { get; private set; }

	public static bool IsRepeating { get; set; }

	public static bool IsDisposed { get; private set; }

	public double Volume { get; set; }

	public static void Reset()
	{
		Music.m_PlayPosition = (Music.m_PlayPositionStart = (Music.m_NextPlayPosition = TimeSpan.Zero));
		Music.m_Clock.Restart();
		Music.m_PlayCounter++;
	}

	private static void PlayAsync()
	{
		int num = 0;
		int num2 = 0;
		try
		{
			while (true)
			{
				bool flag = true;
				while (!Music.IsDisposed && !Music.m_PlayEvent.WaitOne(1))
				{
				}
				if (Music.IsDisposed)
				{
					break;
				}
				Music.m_Clock.Restart();
				Music.m_PlayPositionStart = Music.m_NextPlayPosition;
				Music.m_PlayPosition = Music.m_PlayPositionStart;
				num = Music.m_PlayCounter;
				IEnumerator<DataPointer> enumerator = Music.m_AudioDecoder.GetSamples(Music.m_PlayPositionStart).GetEnumerator();
				bool flag2 = true;
				bool flag3 = false;
				while (true)
				{
					bool flag4 = true;
					while (!Music.IsDisposed && !Music.Stopped && !Music.m_PlayEvent.WaitOne(1))
					{
					}
					if (Music.IsDisposed || Music.Stopped)
					{
						Music.m_NextPlayPosition = TimeSpan.Zero;
						break;
					}
					if (num != Music.m_PlayCounter)
					{
						break;
					}
					while (Music.m_SourceVoice.State.BuffersQueued == Music.m_AudioBuffers.Length && !Music.IsDisposed && !Music.Stopped)
					{
						Music.m_BufferEndEvent.WaitOne(1);
					}
					if (Music.IsDisposed || Music.Stopped)
					{
						Music.m_NextPlayPosition = TimeSpan.Zero;
						break;
					}
					if (!enumerator.MoveNext())
					{
						flag3 = true;
						break;
					}
					DataPointer current = enumerator.Current;
					if (num != Music.m_PlayCounter)
					{
						break;
					}
					if (current.Size > Music.m_MemoryBuffers[num2].Size)
					{
						if (Music.m_MemoryBuffers[num2].Pointer != IntPtr.Zero)
						{
							Utilities.FreeMemory(Music.m_MemoryBuffers[num2].Pointer);
						}
						Music.m_MemoryBuffers[num2].Pointer = Utilities.AllocateMemory(current.Size);
						Music.m_MemoryBuffers[num2].Size = current.Size;
					}
					Utilities.CopyMemory(Music.m_MemoryBuffers[num2].Pointer, current.Pointer, current.Size);
					Music.m_AudioBuffers[num2].AudioDataPointer = Music.m_MemoryBuffers[num2].Pointer;
					Music.m_AudioBuffers[num2].AudioBytes = current.Size;
					if (flag2)
					{
						Music.m_Clock.Restart();
						flag2 = false;
						Music.m_WaitForPlayToOutput.Set();
					}
					Music.m_PlayPosition = new TimeSpan(Music.m_PlayPositionStart.Ticks + Music.m_Clock.Elapsed.Ticks);
					Music.m_SourceVoice.SubmitSourceBuffer(Music.m_AudioBuffers[num2], null);
					num2 = ++num2 % Music.m_AudioBuffers.Length;
				}
				if (!Music.IsDisposed && flag3 && !Music.IsRepeating && Music.Playing)
				{
					Music.Stop();
				}
			}
		}
		finally
		{
			Music.Stop();
		}
	}

	public static void Stop()
	{
		if (Music.m_XAudio2 == null || Music.Stopped)
		{
			return;
		}
		Music.m_PlayPosition = TimeSpan.Zero;
		Music.m_NextPlayPosition = TimeSpan.Zero;
		Music.m_PlayPositionStart = TimeSpan.Zero;
		Music.m_Clock.Stop();
		Music.m_PlayCounter++;
		Music.m_State = PMediaPlayerState.Stopped;
		Music.m_PlayEvent.Reset();
		if (Music.m_FileStream != null)
		{
			Music.m_FileStream.Close();
			Music.m_FileStream.Dispose();
			Music.m_FileStream = null;
		}
		if (Music.m_SourceVoice != null)
		{
			Music.m_SourceVoice.Stop();
			Music.m_SourceVoice.FlushSourceBuffers();
		}
		if (Music.m_AudioBuffers != null)
		{
			for (int i = 0; i < Music.m_AudioBuffers.Length; i++)
			{
				Utilities.FreeMemory(Music.m_MemoryBuffers[i].Pointer);
				Music.m_MemoryBuffers[i].Pointer = IntPtr.Zero;
			}
		}
	}

	public static void Destroy()
	{
		if (Music.m_XAudio2 != null)
		{
			Music.Stop();
			if (Music.m_SourceVoice != null)
			{
				Music.m_SourceVoice.Stop();
				Music.m_SourceVoice.DestroyVoice();
				Music.m_SourceVoice = null;
			}
			if (Music.m_AudioDecoder != null)
			{
				Music.m_AudioDecoder.Dispose();
				Music.m_AudioDecoder = null;
			}
			Music.m_MasteringVoice.DestroyVoice();
			Music.m_MasteringVoice.Dispose();
			Music.m_MasteringVoice = null;
			Music.m_XAudio2.Dispose();
			Music.m_XAudio2 = null;
			MediaFactory.Shutdown();
			Music.IsDisposed = true;
			Music.m_PlayingTask.Wait();
		}
	}

	public static void Dispose()
	{
		Music.Stop();
	}

	public static void UpdateVolume()
	{
	}

	public static void Play(string fileName)
	{
		if (!Music.Stopped && Music.m_FileName == fileName)
		{
			return;
		}
		if (Music.m_XAudio2 == null)
		{
			MediaFactory.Startup(131184, 1);
			Music.m_XAudio2 = new XAudio2();
			Music.m_XAudio2.StartEngine();
			Music.m_MasteringVoice = new MasteringVoice(Music.m_XAudio2);
			Music.m_AudioDecoder = new AudioDecoder();
			Music.m_PlayingTask = Task.Factory.StartNew(PlayAsync, TaskCreationOptions.LongRunning);
		}
		string text = Engine.FileManager.ResolveMUL($"music/{fileName}");
		if (!File.Exists(text))
		{
			return;
		}
		if (!Music.Stopped)
		{
			Music.Stop();
		}
		Music.m_FileName = fileName;
		Music.m_FileStream = new NativeFileStream(text, NativeFileMode.Open, NativeFileAccess.Read);
		Music.m_AudioDecoder.SetSourceStream(Music.m_FileStream);
		if (Music.m_SourceVoice == null)
		{
			Music.m_SourceVoice = new SourceVoice(Music.m_XAudio2, Music.m_AudioDecoder.WaveFormat);
			Music.m_SourceVoice.BufferEnd += SourceVoice_BufferEnd;
		}
		Music.m_SourceVoice.Start();
		Music.m_AudioBuffers = new AudioBuffer[3];
		Music.m_MemoryBuffers = new DataPointer[Music.m_AudioBuffers.Length];
		for (int i = 0; i < Music.m_AudioBuffers.Length; i++)
		{
			Music.m_AudioBuffers[i] = new AudioBuffer();
			Music.m_MemoryBuffers[i].Size = 32768;
			Music.m_MemoryBuffers[i].Pointer = Utilities.AllocateMemory(Music.m_MemoryBuffers[i].Size);
		}
		Volume volume = Preferences.Current.Music.Volume;
		if (volume == null || !volume.Mute)
		{
			if (volume != null)
			{
				Music.m_SourceVoice.SetVolume(1f);
			}
			bool flag = false;
			if (Music.Stopped)
			{
				Music.m_PlayCounter++;
				Music.m_WaitForPlayToOutput.Reset();
				flag = true;
			}
			else
			{
				Music.m_Clock.Start();
			}
			Music.m_State = PMediaPlayerState.Playing;
			Music.m_PlayEvent.Set();
			if (flag)
			{
				Music.m_WaitForPlayToOutput.WaitOne();
			}
		}
	}

	private static void SourceVoice_BufferEnd(IntPtr obj)
	{
		Music.m_BufferEndEvent.Set();
	}

	static Music()
	{
		Music.m_State = PMediaPlayerState.Stopped;
		Music.m_BufferEndEvent = new AutoResetEvent(initialState: false);
		Music.m_PlayEvent = new ManualResetEvent(initialState: false);
		Music.m_WaitForPlayToOutput = new ManualResetEvent(initialState: false);
		Music.m_Clock = new Stopwatch();
	}
}
