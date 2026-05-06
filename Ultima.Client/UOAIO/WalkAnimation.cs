using System;
using System.Collections;
using System.Collections.Generic;
using UOAIO.Profiles;

namespace UOAIO;

public class WalkAnimation
{
	private int m_NewX;

	private int m_NewY;

	private int m_NewZ;

	private int m_NewDir;

	private int m_X;

	private int m_Y;

	private TimeSync m_Sync;

	private float m_Duration;

	private float m_Frames;

	private Mobile m_Mobile;

	private bool m_Advance;

	private static Queue m_Pool;

	private static Queue<TimeSync> m_SyncPool;

	public float Duration
	{
		get
		{
			return this.m_Duration;
		}
		set
		{
			this.m_Duration = Math.Max(0f, value);
		}
	}

	public int xOffset => this.m_X;

	public int yOffset => this.m_Y;

	public int Frames => (int)this.m_Frames;

	public bool Advance => this.m_Advance;

	public int NewX => this.m_NewX;

	public int NewY => this.m_NewY;

	public int NewZ => this.m_NewZ;

	public int NewDir => this.m_NewDir;

	public static WalkAnimation PoolInstance(Mobile m, int x, int y, int z, int dir)
	{
		if (WalkAnimation.m_Pool == null)
		{
			WalkAnimation.m_Pool = new Queue();
		}
		if (WalkAnimation.m_Pool.Count > 0)
		{
			WalkAnimation walkAnimation = (WalkAnimation)WalkAnimation.m_Pool.Dequeue();
			walkAnimation.Initialize(m, x, y, z, dir);
			return walkAnimation;
		}
		return new WalkAnimation(m, x, y, z, dir);
	}

	private static int GetFrames(bool mounted, int idx)
	{
		if (!mounted)
		{
			return (idx == 0) ? 4 : 2;
		}
		return (idx != 0) ? 1 : 2;
	}

	public void Dispose()
	{
		WalkAnimation.m_Pool.Enqueue(this);
		if (this.m_Sync != null)
		{
			WalkAnimation.m_SyncPool.Enqueue(this.m_Sync);
		}
	}

	private WalkAnimation(Mobile m, int x, int y, int z, int dir)
	{
		this.Initialize(m, x, y, z, dir);
	}

	private void Initialize(Mobile m, int NewX, int NewY, int NewZ, int NewDir)
	{
		this.m_Mobile = m;
		this.m_NewX = NewX;
		this.m_NewY = NewY;
		this.m_NewZ = NewZ;
		this.m_NewDir = NewDir;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (m.Walking.Count == 0)
		{
			num = m.X;
			num2 = m.Y;
			num3 = m.Z;
		}
		else
		{
			IEnumerator<WalkAnimation> enumerator = m.Walking.GetEnumerator();
			WalkAnimation walkAnimation = null;
			while (enumerator.MoveNext())
			{
				walkAnimation = enumerator.Current;
			}
			if (walkAnimation != null)
			{
				num = walkAnimation.m_NewX;
				num2 = walkAnimation.m_NewY;
				num3 = walkAnimation.m_NewZ;
			}
		}
		if (!m.Player)
		{
			m.Direction = (byte)NewDir;
		}
		this.m_Advance = false;
		this.m_Sync = null;
		if (num != NewX || num2 != NewY || num3 != NewZ)
		{
			int num4 = NewX - num;
			int num5 = NewY - num2;
			int num6 = NewZ - num3;
			int x = (num4 - num5) * 22;
			int y = (num4 + num5) * 22 - num6 * 4;
			this.m_X = x;
			this.m_Y = y;
			int num7 = (NewDir >> 7) & 1;
			int num8 = m.Speed;
			if (num7 == 1)
			{
				num8 *= 2;
			}
			this.m_Duration = Walking.Speed / (float)num8;
			this.m_Frames = WalkAnimation.GetFrames(m.IsMounted, num7);
		}
		else
		{
			this.m_X = 0;
			this.m_Y = 0;
			this.m_Duration = 0.1f;
			this.m_Frames = 0f;
		}
	}

	public void Start()
	{
		this.Start(update: true);
	}

	public void Start(bool update)
	{
		if (this.m_Sync != null)
		{
			return;
		}
		if (WalkAnimation.m_SyncPool.Count > 0)
		{
			this.m_Sync = WalkAnimation.m_SyncPool.Dequeue();
			this.m_Sync.Initialize(this.m_Duration);
		}
		else
		{
			this.m_Sync = new TimeSync(this.m_Duration);
		}
		this.m_Advance = (this.m_NewDir & 7) >= 1 && (this.m_NewDir & 7) <= 4;
		if (this.m_Advance)
		{
			this.m_Mobile.SetLocation(this.m_NewX, this.m_NewY, this.m_NewZ);
			if (update)
			{
				this.m_Mobile.Update();
			}
			if (this.m_Mobile.Player)
			{
				Renderer.eOffsetX += this.m_X;
				Renderer.eOffsetY += this.m_Y;
			}
		}
	}

	public bool Snapshot(ref int xOffset, ref int yOffset, ref int fOffset)
	{
		if (this.m_Sync == null)
		{
			this.Start();
		}
		double num = this.m_Sync.Normalized;
		if (!Options.Current.SmoothWalk && num < 1.0)
		{
			switch ((int)this.m_Frames)
			{
			case 1:
				num = 0.0;
				break;
			case 2:
				num = ((num < 0.5) ? 0.49999 : 0.99999);
				break;
			case 4:
				num = ((!(num < 0.25)) ? ((!(num < 0.5)) ? ((!(num < 0.75)) ? 0.99999 : 0.74999) : 0.49999) : 0.24999);
				break;
			}
		}
		if (!this.m_Advance)
		{
			xOffset = (int)((double)(float)this.m_X * num);
			yOffset = (int)((double)(float)this.m_Y * num);
		}
		else
		{
			xOffset = -this.m_X + (int)((double)this.m_X * num);
			yOffset = -this.m_Y + (int)((double)this.m_Y * num);
		}
		fOffset = (int)((double)this.m_Frames * num);
		return num < 1.0;
	}

	static WalkAnimation()
	{
		WalkAnimation.m_SyncPool = new Queue<TimeSync>();
	}
}
