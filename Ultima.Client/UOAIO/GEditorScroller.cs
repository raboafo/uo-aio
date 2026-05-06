using System.Reflection;
using System.Windows.Forms;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GEditorScroller : GSliderBase
{
	private enum State
	{
		Normal,
		SmallScrollUp,
		SmallScrollDown,
		LargeScrollUp,
		LargeScrollDown,
		Inactive
	}

	private GEditorPanel m_Panel;

	private Texture m_ScrollTexture;

	private State m_State = State.Inactive;

	private int m_Offset;

	public GEditorScroller(GEditorPanel panel)
		: base(0, 0)
	{
		this.m_Panel = panel;
		base.LargeOffset = 21;
		base.WheelOffset = 21;
		base.SmallOffset = 7;
	}

	protected internal override void OnDispose()
	{
		if (this.m_ScrollTexture != null)
		{
			this.m_ScrollTexture.Dispose();
		}
		this.m_ScrollTexture = null;
		base.OnDispose();
	}

	private int GetBarHeight()
	{
		int num = this.Height - 32;
		int num2 = num * base.LargeOffset / (base.Maximum - base.Minimum + 1);
		if (num2 > num)
		{
			num2 = num;
		}
		if (num2 < 8)
		{
			num2 = 8;
		}
		return num2;
	}

	protected internal unsafe override void Draw(int X, int Y)
	{
		if (this.m_ScrollTexture == null)
		{
			this.m_ScrollTexture = new Texture(16, 16, TextureTransparency.None);
			LockData lockData = this.m_ScrollTexture.Lock(LockFlags.WriteOnly);
			ushort num = Engine.C32216(GumpColors.ControlLightLight);
			ushort num2 = Engine.C32216(GumpColors.ScrollBar);
			for (int i = 0; i < 16; i++)
			{
				ushort* ptr = (ushort*)((byte*)lockData.pvSrc + i * lockData.Pitch);
				for (int j = 0; j < 16; j++)
				{
					if ((((i & 1) + j) & 1) == 0)
					{
						*(ptr++) = num;
					}
					else
					{
						*(ptr++) = num2;
					}
				}
			}
			this.m_ScrollTexture.Unlock();
		}
		this.m_ScrollTexture.Draw(X, Y, this.Width, this.Height);
		int barHeight = this.GetBarHeight();
		int num3 = Y + 16;
		int num4 = this.Height - 32;
		int position = base.GetPosition(num4 - barHeight);
		Renderer.SetTexture(null);
		if (this.m_State == State.LargeScrollUp)
		{
			if (position > 0)
			{
				Renderer.PushAlpha(0.9f);
				Renderer.SolidRect(GumpColors.ControlDarkDark, X, Y + this.Width, this.Width, position);
				Renderer.PopAlpha();
				int num5 = base.PointToClient(new Point(Engine.m_xMouse, Engine.m_yMouse)).Y - 16;
				if (position > num5)
				{
					base.Value -= base.LargeOffset;
				}
				else
				{
					this.m_State = State.Inactive;
				}
			}
		}
		else if (this.m_State == State.LargeScrollDown && num4 - position - barHeight > 0)
		{
			Renderer.PushAlpha(0.9f);
			Renderer.SolidRect(GumpColors.ControlDarkDark, X, num3 + position + barHeight, this.Width, num4 - position - barHeight);
			Renderer.PopAlpha();
			int num6 = base.PointToClient(new Point(Engine.m_xMouse, Engine.m_yMouse)).Y - 16;
			if (position + barHeight < num6)
			{
				base.Value += base.LargeOffset;
			}
			else
			{
				this.m_State = State.Inactive;
			}
		}
		GumpPaint.DrawRaised3D(X, num3 + position, 16, barHeight);
		if (this.m_State == State.SmallScrollUp)
		{
			GumpPaint.DrawFlat(X, Y, this.Width, this.Width);
			Engine.m_WinScrolls[0].Draw(X + 5, Y + 7, GumpColors.ControlText);
			base.Value -= base.SmallOffset;
		}
		else
		{
			GumpPaint.DrawRaised3D(X, Y, this.Width, this.Width);
			Engine.m_WinScrolls[0].Draw(X + 4, Y + 6, GumpColors.ControlText);
		}
		Renderer.SetTexture(null);
		if (this.m_State == State.SmallScrollDown)
		{
			GumpPaint.DrawFlat(X, Y + this.Height - this.Width, this.Width, this.Width);
			Engine.m_WinScrolls[1].Draw(X + 5, Y + this.Height - this.Width + 7, GumpColors.ControlText);
			base.Value += base.SmallOffset;
		}
		else
		{
			GumpPaint.DrawRaised3D(X, Y + this.Height - this.Width, this.Width, this.Width);
			Engine.m_WinScrolls[1].Draw(X + 4, Y + this.Height - this.Width + 6, GumpColors.ControlText);
		}
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return true;
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		int barHeight = this.GetBarHeight();
		int num = 16;
		int num2 = this.Height - 32;
		if (Y < num)
		{
			this.m_State = State.SmallScrollUp;
			Gumps.Capture = this;
			return;
		}
		if (Y >= num + num2)
		{
			this.m_State = State.SmallScrollDown;
			Gumps.Capture = this;
			return;
		}
		int position = base.GetPosition(num2 - barHeight);
		int num3 = Y - num - position;
		if (num3 < 0)
		{
			this.m_State = State.LargeScrollUp;
			Gumps.Capture = this;
			return;
		}
		if (num3 >= barHeight)
		{
			this.m_State = State.LargeScrollDown;
			Gumps.Capture = this;
			return;
		}
		this.m_State = State.Normal;
		this.m_Offset = num3;
		base.Value = base.GetValue(num3 - this.m_Offset + position, this.Height - 32 - barHeight);
		Gumps.Capture = this;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (Gumps.Capture == this && this.m_State == State.Normal)
		{
			int barHeight = this.GetBarHeight();
			base.Value = base.GetValue(Y - 16 - this.m_Offset, this.Height - 32 - barHeight);
		}
		this.m_State = State.Inactive;
		Gumps.Capture = null;
	}

	protected internal override void OnMouseMove(int X, int Y, MouseButtons mb)
	{
		if (Gumps.Capture == this && this.m_State == State.Normal)
		{
			int barHeight = this.GetBarHeight();
			base.Value = base.GetValue(Y - 16 - this.m_Offset, this.Height - 32 - barHeight);
		}
	}

	protected override void OnChanged(int oldValue)
	{
		this.m_Panel.Layout();
	}
}
