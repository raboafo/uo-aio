using System;
using SharpDX.DirectSound;

namespace UOAIO.Assets;

public interface IAudioProvider : IDisposable
{
	SecondarySoundBuffer Acquire(int soundId);
}
