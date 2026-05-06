using System;
using System.Collections.Generic;

namespace UOAIO;

public class MobileCell : IAgentCell, IAgentView, ICell, IDisposable, IAnimatedCell, IEntity
{
	private sbyte m_Z;

	public Mobile m_Mobile;

	private static Type MyType;

	public Type CellType => MobileCell.MyType;

	public PhysicalAgent Agent => this.m_Mobile;

	public List<ICell> Owner { get; set; }

	public int Serial => this.m_Mobile.Serial;

	public sbyte Z => this.m_Z;

	public sbyte SortZ
	{
		get
		{
			return this.m_Z;
		}
		set
		{
		}
	}

	public byte Height => 15;

	public void Update()
	{
		this.m_Z = (sbyte)this.m_Mobile.Z;
	}

	void IDisposable.Dispose()
	{
	}

	public MobileCell(Mobile m)
	{
		this.m_Mobile = m;
		this.m_Z = (sbyte)m.Z;
	}

	public void OnAgentUpdated()
	{
		Map.Update(this.m_Mobile);
	}

	public void OnAgentDeleted()
	{
		Map.Update(this.m_Mobile);
	}

	public void GetPackage(ref int Body, ref int Action, ref int Direction, ref int Frame, ref int Hue)
	{
		Mobile mobile = this.m_Mobile;
		Body = mobile.Body;
		if (mobile.Ghost)
		{
			Body = 970;
		}
		Hue = mobile.Hue;
		Hue ^= 32768;
		if (mobile.Walking.Count > 0)
		{
			Direction = Engine.GetAnimDirection((byte)mobile.Walking.Peek().NewDir);
			int num = 0;
			GenericAction g;
			if (mobile.IsMounted)
			{
				g = (((Direction & 0x80) == 0) ? GenericAction.MountedWalk : GenericAction.MountedRun);
				num = (((Direction & 0x80) != 0) ? 1 : 2);
			}
			else
			{
				g = (((Direction & 0x80) == 0) ? GenericAction.Walk : GenericAction.Run);
				num = (((Direction & 0x80) == 0) ? 4 : 2);
			}
			Action = Engine.m_Animations.ConvertAction(Body, mobile.Serial, mobile.X, mobile.Y, Direction, g, mobile);
			int frameCount = Engine.m_Animations.GetFrameCount(Body, Action, Direction & 7);
			if (frameCount == 0)
			{
				Frame = 0;
			}
			else
			{
				Frame = mobile.MovedTiles * num % frameCount;
			}
			return;
		}
		Direction = Engine.GetAnimDirection(mobile.Direction);
		if (mobile.Animation == null || !mobile.Animation.Running)
		{
			if (mobile.IsMounted)
			{
				GenericAction g2 = GenericAction.MountedStand;
				Action = Engine.m_Animations.ConvertAction(Body, mobile.Serial, mobile.X, mobile.Y, Direction, g2, mobile);
			}
			else if (mobile.IsSitting)
			{
				GenericAction g2 = GenericAction.Sit;
				int bodyID = (Body & 0xFFF) << 21;
				int direction = (Direction & 0xF) << 8;
				Action = Engine.m_Animations.ConvertAction(bodyID, mobile.Serial, mobile.X, mobile.Y, direction, g2, mobile);
			}
			else
			{
				GenericAction g2 = GenericAction.Stand;
				Action = Engine.m_Animations.ConvertAction(Body, mobile.Serial, mobile.X, mobile.Y, Direction, g2, mobile);
			}
			Frame = 0;
			return;
		}
		int frames = Renderer.m_Frames;
		Action = mobile.Animation.Action;
		Direction = Engine.GetAnimDirection((byte)(mobile.Direction & 7));
		Action %= 35;
		Direction &= 7;
		int num2 = Engine.m_Animations.GetFrameCount(Body, Action, Direction);
		if (num2 == 0)
		{
			num2 = 1;
		}
		int num3 = mobile.Animation.Delay * 2 + 4;
		if (num3 < 1)
		{
			num3 = 1;
		}
		Frame = (frames - mobile.Animation.Start) / num3 % num2;
		if (!mobile.Animation.Forward)
		{
			Frame = num2 - 1 - Frame;
		}
		if (mobile.Animation.Repeat && mobile.Animation.RepeatCount != 0 && frames >= mobile.Animation.Start + mobile.Animation.RepeatCount * num2 * num3 - 1)
		{
			if (mobile.Animation.OnAnimationEnd != null)
			{
				mobile.Animation.OnAnimationEnd(mobile.Animation, mobile);
			}
		}
		else if (!mobile.Animation.Repeat && frames >= mobile.Animation.Start + num2 * num3 - 1 && mobile.Animation.OnAnimationEnd != null)
		{
			mobile.Animation.OnAnimationEnd(mobile.Animation, mobile);
		}
		if (mobile.Animation.Repeat && mobile.Animation.RepeatCount != 0 && frames >= mobile.Animation.Start + mobile.Animation.RepeatCount * num2 * num3)
		{
			mobile.Animation.Stop();
			GenericAction g3 = ((!mobile.IsMounted) ? GenericAction.Stand : GenericAction.MountedStand);
			Action = Engine.m_Animations.ConvertAction(Body, mobile.Serial, mobile.X, mobile.Y, Direction, g3, mobile);
			Frame = 0;
			Direction = Engine.GetAnimDirection(mobile.Direction);
			Direction %= 8;
			mobile.Animation = null;
		}
		else if (!mobile.Animation.Repeat && frames >= mobile.Animation.Start + num2 * num3)
		{
			mobile.Animation.Stop();
			GenericAction g4 = ((!mobile.IsMounted) ? GenericAction.Stand : GenericAction.MountedStand);
			Action = Engine.m_Animations.ConvertAction(Body, mobile.Serial, mobile.X, mobile.Y, Direction, g4, mobile);
			Frame = 0;
			Direction = Engine.GetAnimDirection(mobile.Direction);
			Direction %= 8;
			mobile.Animation = null;
		}
	}

	static MobileCell()
	{
		MobileCell.MyType = typeof(MobileCell);
	}
}
