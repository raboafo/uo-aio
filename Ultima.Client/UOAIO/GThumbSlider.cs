using System;
using System.Windows.Forms;

namespace UOAIO;

public abstract class GThumbSlider : GSliderBase
{
	private SliderOrientation _orientation;

	private SliderState _state;

	private int _seekOffset;

	public SliderOrientation Orientation
	{
		get
		{
			return this._orientation;
		}
		set
		{
			this._orientation = value;
		}
	}

	public SliderState State
	{
		get
		{
			return this._state;
		}
		set
		{
			this._state = value;
		}
	}

	protected virtual int GetTrackSize()
	{
		return this._orientation switch
		{
			SliderOrientation.Horizontal => this.Width, 
			SliderOrientation.Vertical => this.Height, 
			_ => throw new InvalidOperationException(), 
		};
	}

	protected virtual int GetPosition(int x, int y)
	{
		return this._orientation switch
		{
			SliderOrientation.Horizontal => x, 
			SliderOrientation.Vertical => y, 
			_ => throw new InvalidOperationException(), 
		};
	}

	protected abstract int GetThumbSize();

	public GThumbSlider(int x, int y, SliderOrientation orientation)
		: base(x, y)
	{
		this._orientation = orientation;
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		int trackSize = this.GetTrackSize();
		int thumbSize = this.GetThumbSize();
		int position = base.GetPosition(trackSize - thumbSize);
		switch (this._state)
		{
		case SliderState.SmallUp:
			base.Value -= base.SmallOffset;
			break;
		case SliderState.SmallDown:
			base.Value += base.SmallOffset;
			break;
		case SliderState.LargeUp:
		{
			Point point2 = base.PointToClient(new Point(Engine.m_xMouse, Engine.m_yMouse));
			int position3 = this.GetPosition(point2.X, point2.Y);
			if (position > position3)
			{
				base.Value -= base.LargeOffset;
			}
			else
			{
				this._state = SliderState.Idle;
			}
			break;
		}
		case SliderState.LargeDown:
		{
			Point point = base.PointToClient(new Point(Engine.m_xMouse, Engine.m_yMouse));
			int position2 = this.GetPosition(point.X, point.Y);
			if (position + thumbSize < position2)
			{
				base.Value += base.LargeOffset;
			}
			else
			{
				this._state = SliderState.Idle;
			}
			break;
		}
		}
		this.DrawThumb(X, Y, position);
	}

	protected abstract void DrawThumb(int x, int y, int p);

	protected internal override bool HitTest(int X, int Y)
	{
		return true;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (this._state == SliderState.Seek && Gumps.Capture == this)
		{
			int trackSize = this.GetTrackSize();
			int thumbSize = this.GetThumbSize();
			base.Value = base.GetValue(this.GetPosition(X, Y) - this._seekOffset, trackSize - thumbSize);
		}
		this._state = SliderState.Idle;
		Gumps.Capture = null;
	}

	protected internal override void OnMouseMove(int X, int Y, MouseButtons mb)
	{
		if (this._state == SliderState.Seek && Gumps.Capture == this)
		{
			int trackSize = this.GetTrackSize();
			int thumbSize = this.GetThumbSize();
			base.Value = base.GetValue(this.GetPosition(X, Y) - this._seekOffset, trackSize - thumbSize);
		}
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		int trackSize = this.GetTrackSize();
		int thumbSize = this.GetThumbSize();
		int position = base.GetPosition(trackSize - thumbSize);
		int num = this.GetPosition(X, Y) - position;
		SliderState state;
		if (num < 0)
		{
			state = SliderState.LargeUp;
		}
		else if (num >= thumbSize)
		{
			state = SliderState.LargeDown;
		}
		else
		{
			state = SliderState.Seek;
			this._seekOffset = num;
		}
		this.State = state;
		Gumps.Capture = this;
	}
}
