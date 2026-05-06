using System;
using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class GAlphaVSlider : Gump
{
	protected int m_HalfHeight;

	protected int m_xOffset;

	protected int m_Width;

	protected int m_Height;

	protected int m_Position;

	protected double m_Start;

	protected double m_End;

	protected double m_Increase;

	protected double m_ScrollOffset = 5.0;

	protected OnValueChange m_OnValueChange;

	public OnValueChange OnValueChange
	{
		get
		{
			return this.m_OnValueChange;
		}
		set
		{
			this.m_OnValueChange = value;
		}
	}

	public int HalfHeight => this.m_HalfHeight;

	public override int Width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
			this.m_xOffset = (this.m_Width - 16) / 2;
		}
	}

	public override int Height
	{
		get
		{
			return this.m_Height;
		}
		set
		{
			this.m_Height = value;
		}
	}

	public double Start
	{
		get
		{
			return this.m_Start;
		}
		set
		{
			this.m_Start = value;
		}
	}

	public double End
	{
		get
		{
			return this.m_End;
		}
		set
		{
			this.m_End = value;
		}
	}

	public double Increase
	{
		get
		{
			return this.m_Increase;
		}
		set
		{
			this.m_Increase = value;
		}
	}

	public double ScrollOffset
	{
		get
		{
			return this.m_ScrollOffset;
		}
		set
		{
			this.m_ScrollOffset = value;
		}
	}

	public int Position
	{
		get
		{
			return this.m_Position;
		}
		set
		{
			this.m_Position = value;
		}
	}

	public GAlphaVSlider(int X, int Y, int Width, int Height, double Value, double Start, double End, double Increase)
		: base(X, Y)
	{
		this.m_Width = Width;
		this.m_Height = Height;
		this.m_Start = Start;
		this.m_End = End;
		this.m_Increase = Increase;
		this.m_HalfHeight = 6;
		this.SetValue(Value, CallOnChange: false);
	}

	public void Slide(int Y)
	{
		Gumps.Capture = this;
		int position = this.m_Position;
		double value = this.GetValue();
		this.m_Position = Y;
		if (this.m_Position < 0)
		{
			this.m_Position = 0;
		}
		else if (this.m_Position >= this.m_Height)
		{
			this.m_Position = this.m_Height - 1;
		}
		double num = this.m_End - this.m_Start + 1.0;
		num -= 1E-13;
		double num2 = (double)this.m_Position / (double)(this.m_Height - 1);
		double num3 = num2 * num;
		num3 += this.m_Start;
		num3 = (int)num3;
		if (position != this.m_Position)
		{
			if (this.m_OnValueChange != null)
			{
				this.m_OnValueChange(num3, value, this);
			}
			Engine.Redraw();
		}
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		if (mb != MouseButtons.None)
		{
			this.Slide(Y);
		}
	}

	protected internal override void OnMouseMove(int X, int Y, MouseButtons mb)
	{
		if (mb != MouseButtons.None)
		{
			this.Slide(Y);
		}
	}

	protected internal override void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
		if (mb != MouseButtons.None)
		{
			this.Slide(Y);
		}
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (mb != MouseButtons.None)
		{
			this.Slide(Y);
		}
		Gumps.Capture = null;
	}

	protected internal override void OnMouseWheel(int Delta)
	{
		this.SetValue(this.GetValue() + (double)(-Math.Sign(Delta)) * this.m_ScrollOffset * this.m_Increase, CallOnChange: true);
	}

	protected internal override void Draw(int X, int Y)
	{
		Renderer.SetTexture(null);
		Renderer.PushAlpha(0.5f);
		Renderer.SolidRect(8421504, X, Y - this.m_HalfHeight + 1, this.m_Width - 2, this.m_Height + this.m_HalfHeight * 2 - 3);
		X += this.m_xOffset;
		Y += this.m_Position - this.m_HalfHeight;
		X--;
		Renderer.SetAlpha(0.8f);
		Renderer.TransparentRect(0, X, Y, 16, 12);
		Renderer.SetAlpha(0.5f);
		if (Gumps.Capture == this)
		{
			Renderer.GradientRect(16448250, 9868950, X + 1, Y + 1, 14, 10);
		}
		else
		{
			Renderer.GradientRect(13158600, 6579300, X + 1, Y + 1, 14, 10);
		}
		Renderer.PopAlpha();
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return !Engine.amMoving && !TargetManager.IsActive;
	}

	public void SetValue(double Value, bool CallOnChange)
	{
		if (Value > this.m_End)
		{
			Value = this.m_End;
		}
		else if (Value < this.m_Start)
		{
			Value = this.m_Start;
		}
		double value = this.GetValue();
		double num = Value - this.m_Start;
		num /= this.m_End - this.m_Start;
		if (num < 0.0)
		{
			num = 0.0;
		}
		else if (num > 1.0)
		{
			num = 1.0;
		}
		this.m_Position = (int)(num * (double)(this.m_Height - 1));
		if (CallOnChange && this.m_OnValueChange != null)
		{
			this.m_OnValueChange(Value, value, this);
		}
	}

	public double GetValue()
	{
		return this.GetValue(this.m_Position);
	}

	public double GetValue(int Position)
	{
		if (Position < 0)
		{
			Position = 0;
		}
		else if (Position >= this.m_Height)
		{
			Position = this.m_Height - 1;
		}
		double num = this.m_End - this.m_Start + 1.0;
		num -= 1E-13;
		double num2 = (double)Position / (double)(this.m_Height - 1);
		double num3 = num2 * num;
		num3 += this.m_Start;
		return (int)num3;
	}
}
