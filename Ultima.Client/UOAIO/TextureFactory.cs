using System.Collections;
using System.Collections.Generic;
using SharpDX.Direct3D9;

namespace UOAIO;

public abstract class TextureFactory
{
	private static List<TextureFactory> m_Factories;

	public static Queue m_Disposing;

	private List<Texture> m_Textures = new List<Texture>();

	public abstract TextureTransparency Transparency { get; }

	public static void StippleDispose(int timeNow)
	{
		int num = timeNow - 15000;
		for (int i = 0; i < 5000; i++)
		{
			if (TextureFactory.m_Disposing.Count <= 0)
			{
				break;
			}
			object obj = TextureFactory.m_Disposing.Dequeue();
			if (obj is Frames)
			{
				Frames frames = (Frames)obj;
				if (frames.Disposed || frames.LastAccessTime <= num)
				{
					frames.Dispose();
					Engine.m_Animations.m_Frames.Remove(frames);
					i += frames.FrameCount;
				}
				continue;
			}
			Texture texture = (Texture)obj;
			if (!texture.m_Surface.IsDisposed)
			{
				texture.m_Surface.Dispose();
			}
			if (texture.m_Factory != null)
			{
				texture.m_Factory.m_Textures.Remove(texture);
			}
		}
	}

	public TextureFactory()
	{
		TextureFactory.m_Factories.Add(this);
	}

	public void Remove(Texture tex)
	{
		this.m_Textures.Remove(tex);
	}

	public static void FullCleanup(int timeNow)
	{
		for (int i = 0; i < TextureFactory.m_Factories.Count; i++)
		{
			TextureFactory.m_Factories[i].Cleanup(timeNow);
		}
	}

	public void Cleanup(int timeNow)
	{
		int num = timeNow - 15000;
		for (int i = 0; i < this.m_Textures.Count; i++)
		{
			Texture texture = this.m_Textures[i];
			if (texture.m_Surface == null || texture.m_Surface.IsDisposed)
			{
				this.m_Textures.RemoveAt(i--);
			}
			else if (texture.m_LastAccess <= num)
			{
				TextureFactory.m_Disposing.Enqueue(texture);
			}
		}
	}

	protected unsafe Texture Construct(bool isReconstruct)
	{
		if (!this.CoreLookup())
		{
			return Texture.Empty;
		}
		this.CoreGetDimensions(out var width, out var height);
		Texture texture = new Texture(width, height, Format.A1R5G5B5, Pool.Managed, isReconstruct, this.Transparency);
		if (texture.IsEmpty())
		{
			return Texture.Empty;
		}
		LockData lockData = texture.Lock(LockFlags.WriteOnly);
		this.CoreProcessImage(lockData.Width, lockData.Height, lockData.Pitch, (ushort*)lockData.pvSrc, (ushort*)lockData.pvSrc + lockData.Width, (ushort*)((byte*)lockData.pvSrc + lockData.Height * lockData.Pitch), (lockData.Pitch >> 1) - lockData.Width, lockData.Pitch >> 1);
		texture.Unlock();
		this.CoreAssignArgs(texture);
		this.m_Textures.Add(texture);
		return texture;
	}

	public abstract Texture Reconstruct(object[] args);

	protected abstract void CoreAssignArgs(Texture tex);

	protected abstract bool CoreLookup();

	protected abstract void CoreGetDimensions(out int width, out int height);

	protected unsafe abstract void CoreProcessImage(int width, int height, int stride, ushort* pLine, ushort* pLineEnd, ushort* pImageEnd, int lineDelta, int lineEndDelta);

	static TextureFactory()
	{
		TextureFactory.m_Factories = new List<TextureFactory>();
		TextureFactory.m_Disposing = new Queue();
	}
}
