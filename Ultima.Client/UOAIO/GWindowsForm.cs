using System;
using System.Reflection;
using System.Windows.Forms;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GWindowsForm : Gump
{
	private int m_Width;

	private int m_Height;

	private GLabel m_CaptionLabel;

	private Gump m_Client;

	private GWindowsButton m_CloseButton;

	public override int Width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
			this.Resize();
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
			this.Resize();
		}
	}

	public string Text
	{
		get
		{
			return this.m_CaptionLabel.Text;
		}
		set
		{
			this.m_CaptionLabel.Text = value;
		}
	}

	public GWindowsButton CloseButton
	{
		get
		{
			return this.m_CloseButton;
		}
		set
		{
			this.m_CloseButton = value;
		}
	}

	public Gump Client => this.m_Client;

	public virtual void Resize()
	{
		this.ResizeClient();
	}

	public virtual void ResizeClient()
	{
		this.m_Client.X = 4;
		this.m_Client.Y = 23;
		this.m_Client.Width = this.Width - 8;
		this.m_Client.Height = this.Height - 27;
		this.m_CloseButton.X = this.Width - 6 - this.m_CloseButton.Width;
		this.m_CloseButton.Y = 6;
	}

	public GWindowsForm()
		: this(0, 0, 200, 200)
	{
	}

	public GWindowsForm(int width, int height)
		: this(0, 0, width, height)
	{
	}

	public GWindowsForm(int x, int y, int width, int height)
		: base(x, y)
	{
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		this.m_Width = width;
		this.m_Height = height;
		this.m_CaptionLabel = new GLabel("Form", Engine.GetUniFont(1), Hues.Default, 7, 3);
		base.m_Children.Add(this.m_CaptionLabel);
		this.m_Client = new GEmpty(0, 0, 0, 0);
		base.m_Children.Add(this.m_Client);
		this.m_CloseButton = new GWindowsButton("", 0, 0, 16, 14);
		this.m_CloseButton.ImageColor = 0;
		this.m_CloseButton.Image = Engine.m_FormX;
		this.m_CloseButton.Clicked += CloseButton_Clicked;
		base.m_Children.Add(this.m_CloseButton);
		this.ResizeClient();
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return true;
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		base.BringToTop();
	}

	protected internal override void OnDragStart()
	{
		if (base.m_OffsetY > 21)
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
		}
		base.BringToTop();
	}

	protected internal override void Draw(int X, int Y)
	{
		Renderer.SetTexture(null);
		GumpPaint.DrawRaised3D(X, Y, this.m_Width, this.m_Height);
		Gump focus = Gumps.Focus;
		if (focus == this || (focus != null && focus.IsChildOf(this)))
		{
			Renderer.GradientRectLR(GumpColors.ActiveCaption, GumpColors.ActiveCaptionGradient, X + 4, Y + 4, this.Width - 8, 18);
			this.m_CaptionLabel.Hue = GumpHues.ActiveCaptionText;
		}
		else
		{
			Renderer.GradientRectLR(GumpColors.InactiveCaption, GumpColors.InactiveCaptionGradient, X + 4, Y + 4, this.Width - 8, 18);
			this.m_CaptionLabel.Hue = GumpHues.InactiveCaptionText;
		}
	}

	public virtual void Close()
	{
		Gumps.Destroy(this);
	}

	private void CloseButton_Clicked(object sender, EventArgs e)
	{
		this.Close();
	}
}
