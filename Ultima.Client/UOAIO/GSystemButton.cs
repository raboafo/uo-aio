using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GSystemButton : GAlphaBackground
{
	private enum ButtonState
	{
		Inactive,
		Active,
		Pressed
	}

	private Color m_InactiveColor;

	private Color m_ActiveColor;

	private Color m_PressedColor;

	private Color m_ForeColor;

	private ButtonState m_State;

	private OnClick m_OnClick;

	public string Text
	{
		get
		{
			GLabel gLabel = null;
			if (base.m_Children.Count > 0)
			{
				gLabel = base.m_Children[0] as GLabel;
			}
			return (gLabel == null) ? "" : gLabel.Text;
		}
		set
		{
			GLabel gLabel = null;
			if (base.m_Children.Count > 0)
			{
				gLabel = base.m_Children[0] as GLabel;
			}
			if (gLabel != null)
			{
				gLabel.Text = value;
			}
		}
	}

	public OnClick OnClick
	{
		get
		{
			return this.m_OnClick;
		}
		set
		{
			this.m_OnClick = value;
		}
	}

	public Color InactiveColor
	{
		get
		{
			return this.m_InactiveColor;
		}
		set
		{
			this.m_InactiveColor = value;
			this.UpdateColors();
		}
	}

	public Color ActiveColor
	{
		get
		{
			return this.m_ActiveColor;
		}
		set
		{
			this.m_ActiveColor = value;
			this.UpdateColors();
		}
	}

	public Color PressedColor
	{
		get
		{
			return this.m_PressedColor;
		}
		set
		{
			this.m_PressedColor = value;
			this.UpdateColors();
		}
	}

	public Color ForeColor
	{
		get
		{
			return this.m_ForeColor;
		}
		set
		{
			this.m_ForeColor = value;
			this.UpdateColors();
		}
	}

	public virtual float Darkness => 0.5f;

	public void UpdateColors()
	{
		switch (this.m_State)
		{
		case ButtonState.Inactive:
			base.FillColor = this.m_InactiveColor.ToArgb() & 0xFFFFFF;
			break;
		case ButtonState.Active:
			base.FillColor = this.m_ActiveColor.ToArgb() & 0xFFFFFF;
			break;
		case ButtonState.Pressed:
			base.FillColor = this.m_PressedColor.ToArgb() & 0xFFFFFF;
			break;
		}
		GLabel gLabel = null;
		if (base.m_Children.Count > 0)
		{
			gLabel = base.m_Children[0] as GLabel;
		}
		if (gLabel != null)
		{
			gLabel.Hue = new Hues.ColorFillHue(this.m_ForeColor.ToArgb() & 0xFFFFFF);
		}
	}

	protected internal override void OnMouseEnter(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Left)
		{
			this.m_State = ButtonState.Pressed;
		}
		else
		{
			this.m_State = ButtonState.Active;
		}
		this.UpdateColors();
	}

	protected internal override void OnMouseLeave()
	{
		this.m_State = ButtonState.Inactive;
		this.UpdateColors();
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Left)
		{
			this.m_State = ButtonState.Pressed;
		}
		else
		{
			this.m_State = ButtonState.Active;
		}
		this.UpdateColors();
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Left && this.m_OnClick != null)
		{
			this.m_OnClick(this);
		}
		this.m_State = ButtonState.Active;
		this.UpdateColors();
	}

	public void SetBackColor(Color backColor)
	{
		this.m_InactiveColor = backColor;
		this.m_ActiveColor = ControlPaint.Dark(backColor, 0f - this.Darkness);
		this.m_PressedColor = ControlPaint.Light(backColor);
		this.UpdateColors();
	}

	public GSystemButton(int x, int y, int width, int height, Color backColor, Color foreColor, string text, IFont font)
		: base(x, y, width, height)
	{
		this.m_InactiveColor = backColor;
		this.m_ActiveColor = ControlPaint.Dark(backColor, 0f - this.Darkness);
		this.m_PressedColor = ControlPaint.Light(backColor);
		this.m_ForeColor = foreColor;
		base.m_CanDrag = false;
		GLabel gLabel = new GLabel(text, font, Hues.Default, 0, 0);
		base.m_Children.Add(gLabel);
		gLabel.Center();
		base.FillAlpha = 1f;
		this.UpdateColors();
	}
}
