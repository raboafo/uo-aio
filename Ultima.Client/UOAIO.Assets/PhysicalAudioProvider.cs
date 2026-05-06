using System;
using SharpDX.DirectSound;
using SharpDX.Multimedia;

namespace UOAIO.Assets;

public sealed class PhysicalAudioProvider : IAudioProvider, IDisposable
{
	private const int HeaderSize = 40;

	private readonly DirectSound _soundDevice;

	private readonly WaveFormat _waveFormat;

	public PhysicalAudioProvider(DirectSound soundDevice)
	{
		if (soundDevice == null)
		{
			throw new ArgumentNullException("soundDevice");
		}
		this._soundDevice = soundDevice;
		this._waveFormat = this.GetWaveFormat();
	}

	private WaveFormat GetWaveFormat()
	{
		return WaveFormat.CreateCustomFormat(WaveFormatEncoding.Pcm, 22050, 1, 44100, 2, 16);
	}

	public SecondarySoundBuffer Acquire(int soundId)
	{
		if (soundId < 0 || soundId >= 4096)
		{
			return null;
		}
		if (!AssetSourceManager.Sounds.TryRead(soundId, out var data) || data == null || data.Length <= HeaderSize)
		{
			return null;
		}
		SoundBufferDescription soundBufferDescription = new SoundBufferDescription
		{
			Format = this._waveFormat,
			BufferBytes = data.Length - HeaderSize
		};
		soundBufferDescription.Flags |= BufferFlags.ControlVolume;
		soundBufferDescription.Flags |= BufferFlags.ControlFrequency;
		soundBufferDescription.Flags |= BufferFlags.ControlPan;
		SecondarySoundBuffer secondarySoundBuffer = new SecondarySoundBuffer(this._soundDevice, soundBufferDescription);
		byte[] array = new byte[data.Length - HeaderSize];
		Buffer.BlockCopy(data, HeaderSize, array, 0, array.Length);
		secondarySoundBuffer.Write(array, 0, SharpDX.DirectSound.LockFlags.EntireBuffer);
		return secondarySoundBuffer;
	}

	public void Dispose()
	{
	}
}
