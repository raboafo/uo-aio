using System;

namespace UOAIO;

public class TextMessage : IComparable
{
	protected Texture m_Image;

	protected bool m_Disposing;

	protected TimeSync m_Sync;

	protected TimeDelay m_Delay;

	protected int m_X;

	protected int m_Y;

	protected int m_Timestamp;

	private VertexCache m_vCache;

	private static VertexCachePool m_vPool;

	protected VertexCachePool VCPool => TextMessage.m_vPool;

	public virtual int X
	{
		get
		{
			return this.m_X;
		}
		set
		{
			this.m_X = value;
		}
	}

	public virtual int Y
	{
		get
		{
			return this.m_Y;
		}
		set
		{
			this.m_Y = value;
		}
	}

	public bool Disposing => this.m_Disposing;

	public bool Elapsed => this.m_Disposing || this.m_Delay.Elapsed;

	public float Alpha
	{
		get
		{
			if (!this.m_Disposing)
			{
				return 1f;
			}
			return (float)(1.0 - this.m_Sync.Normalized);
		}
	}

	public Texture Image => this.m_Image;

	public int CompareTo(object a)
	{
		if (a == null)
		{
			return -1;
		}
		if (a == this)
		{
			return 0;
		}
		TextMessage textMessage = (TextMessage)a;
		if (this.m_Timestamp < textMessage.m_Timestamp)
		{
			return -1;
		}
		if (this.m_Timestamp > textMessage.m_Timestamp)
		{
			return 1;
		}
		return 0;
	}

	public TextMessage(string Message)
		: this(Message, Engine.SystemDuration, Engine.DefaultFont, Engine.DefaultHue)
	{
	}

	public TextMessage(string Message, float Delay)
		: this(Message, Delay, Engine.DefaultFont, Engine.DefaultHue)
	{
	}

	public TextMessage(string Message, float Delay, IFont Font)
		: this(Message, Delay, Font, Engine.DefaultHue)
	{
	}

	public TextMessage(string Message, float Delay, IFont Font, IHue Hue)
	{
		this.m_Timestamp = Engine.Ticks;
		this.m_Image = Font.GetString(Message, Hue);
		this.m_Delay = new TimeDelay(Delay);
	}

	public void Draw(int x, int y)
	{
		if (this.m_vCache == null)
		{
			this.m_vCache = this.VCPool.GetInstance();
		}
		this.m_vCache.Draw(this.m_Image, x, y);
	}

	public void Dispose()
	{
		this.m_Disposing = true;
		this.m_Sync = new TimeSync(1.0);
	}

	public void OnRemove()
	{
		this.VCPool.ReleaseInstance(this.m_vCache);
		this.m_vCache = null;
	}

	static TextMessage()
	{
		TextMessage.m_vPool = new VertexCachePool();
	}
}
