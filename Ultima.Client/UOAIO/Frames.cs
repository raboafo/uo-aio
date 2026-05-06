namespace UOAIO;

public class Frames
{
	public int FrameCount;

	public Frame[] FrameList;

	private static Frames m_Empty;

	public bool Disposed
	{
		get
		{
			for (int i = 0; i < this.FrameList.Length; i++)
			{
				if (this.FrameList[i].Image.m_Surface != null && this.FrameList[i].Image.m_Surface.IsDisposed)
				{
					return true;
				}
			}
			return false;
		}
	}

	public int LastAccessTime
	{
		get
		{
			int num = 0;
			for (int i = 0; i < this.FrameList.Length; i++)
			{
				if (this.FrameList[i].Image.m_LastAccess > num)
				{
					num = this.FrameList[i].Image.m_LastAccess;
				}
			}
			return num;
		}
	}

	public static Frames Empty
	{
		get
		{
			if (Frames.m_Empty == null)
			{
				Frames.m_Empty = new Frames();
				Frames.m_Empty.FrameList = new Frame[0];
			}
			return Frames.m_Empty;
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < this.FrameList.Length; i++)
		{
			if (this.FrameList[i].Image.m_Surface != null && !this.FrameList[i].Image.m_Surface.IsDisposed)
			{
				this.FrameList[i].Image.m_Surface.Dispose();
				Texture.m_Textures.Remove(this.FrameList[i].Image);
			}
		}
	}

	public static Frames Clone(Frames original, ShaderData shaderData)
	{
		if (original == null || original == Frames.m_Empty)
		{
			return original;
		}
		Frames frames = new Frames();
		frames.FrameCount = original.FrameCount;
		frames.FrameList = new Frame[original.FrameList.Length];
		for (int i = 0; i < frames.FrameList.Length; i++)
		{
			frames.FrameList[i] = Frame.Clone(original.FrameList[i], shaderData);
		}
		return frames;
	}
}
