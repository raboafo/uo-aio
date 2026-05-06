using System;
using System.Collections.Generic;
using SharpDX.DirectSound;

namespace UOAIO.Assets;

public sealed class ManagedAudioProvider : IAudioProvider, IDisposable
{
	private sealed class CacheEntry : IDisposable
	{
		private IAudioProvider _audioProvider;

		private int _soundId;

		private List<SecondarySoundBuffer> _buffers;

		private bool _isEmpty;

		public CacheEntry(IAudioProvider audioProvider, int soundId)
		{
			this._audioProvider = audioProvider;
			this._soundId = soundId;
			this._buffers = new List<SecondarySoundBuffer>();
		}

		public SecondarySoundBuffer Acquire()
		{
			if (this._isEmpty)
			{
				return null;
			}
			SecondarySoundBuffer secondarySoundBuffer = this.FindAvailableBuffer();
			if (secondarySoundBuffer == null)
			{
				secondarySoundBuffer = this.CloneExistingBuffer(Sounds.Device);
				if (secondarySoundBuffer == null)
				{
					secondarySoundBuffer = this.CreateNewBuffer();
				}
				if (secondarySoundBuffer != null)
				{
					this.Register(secondarySoundBuffer);
				}
				else
				{
					this._isEmpty = true;
				}
			}
			return secondarySoundBuffer;
		}

		private SecondarySoundBuffer CreateNewBuffer()
		{
			return this._audioProvider.Acquire(this._soundId);
		}

		private SecondarySoundBuffer FindAvailableBuffer()
		{
			for (int i = 0; i < this._buffers.Count; i++)
			{
				SecondarySoundBuffer secondarySoundBuffer = this._buffers[i];
				BufferStatus status = (BufferStatus)secondarySoundBuffer.Status;
				if (status.HasFlag(BufferStatus.BufferLost))
				{
					this._buffers.RemoveAt(i--);
					secondarySoundBuffer.Dispose();
				}
				else if (!status.HasFlag(BufferStatus.Playing))
				{
					return secondarySoundBuffer;
				}
			}
			return null;
		}

		private SecondarySoundBuffer CloneExistingBuffer(DirectSound soundDevice)
		{
			for (int i = 0; i < this._buffers.Count; i++)
			{
				SecondarySoundBuffer sourceBuffer = this._buffers[i];
				try
				{
					SecondarySoundBuffer secondarySoundBuffer = (SecondarySoundBuffer)soundDevice.DuplicateSoundBuffer(sourceBuffer);
					if (secondarySoundBuffer != null)
					{
						return secondarySoundBuffer;
					}
				}
				catch
				{
				}
			}
			return null;
		}

		private void Register(SecondarySoundBuffer soundBuffer)
		{
			if (soundBuffer != null)
			{
				this._buffers.Add(soundBuffer);
			}
		}

		public void Dispose()
		{
			if (this._buffers.Count > 0)
			{
				for (int i = 0; i < this._buffers.Count; i++)
				{
					this._buffers[i].Dispose();
				}
				this._buffers.Clear();
			}
		}
	}

	private IAudioProvider _audioProvider;

	private CacheEntry[] _cacheTable;

	public ManagedAudioProvider(IAudioProvider audioProvider)
	{
		if (audioProvider == null)
		{
			throw new ArgumentNullException("audioProvider");
		}
		this._audioProvider = audioProvider;
		this._cacheTable = new CacheEntry[4096];
	}

	private bool Translate(ref int index)
	{
		GraphicTranslation graphicTranslation = GraphicTranslators.Sound[index];
		if (graphicTranslation != null)
		{
			index = graphicTranslation.FallbackId;
			return true;
		}
		return false;
	}

	public SecondarySoundBuffer Acquire(int soundId)
	{
		if (soundId < 0 || soundId >= this._cacheTable.Length)
		{
			return null;
		}
		CacheEntry cacheEntry = this._cacheTable[soundId];
		if (cacheEntry == null)
		{
			cacheEntry = (this._cacheTable[soundId] = new CacheEntry(this._audioProvider, soundId));
		}
		SecondarySoundBuffer secondarySoundBuffer = cacheEntry.Acquire();
		if (secondarySoundBuffer == null && this.Translate(ref soundId))
		{
			cacheEntry = this._cacheTable[soundId];
			if (cacheEntry == null)
			{
				cacheEntry = (this._cacheTable[soundId] = new CacheEntry(this._audioProvider, soundId));
			}
			secondarySoundBuffer = cacheEntry.Acquire();
		}
		return cacheEntry.Acquire();
	}

	public void Dispose()
	{
		for (int i = 0; i < this._cacheTable.Length; i++)
		{
			if (this._cacheTable[i] != null)
			{
				this._cacheTable[i].Dispose();
				this._cacheTable[i] = null;
			}
		}
	}
}
