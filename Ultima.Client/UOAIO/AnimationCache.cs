using System.Collections.Generic;

namespace UOAIO;

public class AnimationCache
{
	private Dictionary<int, Frames> m_Cache;

	private IHue m_Hue;

	public Frames this[int realID]
	{
		get
		{
			if (!this.m_Cache.TryGetValue(realID, out var value))
			{
				this.m_Cache.Add(realID, value = Engine.m_Animations.Create(realID, this.m_Hue));
			}
			return value;
		}
	}

	public AnimationCache(IHue hue)
	{
		this.m_Cache = new Dictionary<int, Frames>();
		this.m_Hue = hue;
	}

	public void Dispose()
	{
		foreach (Frames value in this.m_Cache.Values)
		{
			Frame[] frameList = value.FrameList;
			foreach (Frame frame in frameList)
			{
				if (frame != null && frame.Image != null)
				{
					frame.Image.Dispose();
				}
			}
		}
		this.m_Cache.Clear();
		this.m_Cache = null;
		this.m_Hue = null;
	}
}
