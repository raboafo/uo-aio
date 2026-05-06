using System;
using System.Reflection;
using System.Windows.Forms;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GWindowsTabPanel : Gump
{
	private int m_Width;

	private int m_Height;

	private string m_Category;

	private GLabel m_CaptionLabel;

	private Gump m_Client;

	private GWindowsTabButton m_TabButton;

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

	public string Category
	{
		get
		{
			return this.m_Category;
		}
		set
		{
			this.m_Category = value;
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

	public GWindowsTabButton CloseButton
	{
		get
		{
			return this.m_TabButton;
		}
		set
		{
			this.m_TabButton = value;
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
		this.m_TabButton.X = this.Width - 6 - this.m_TabButton.Width;
		this.m_TabButton.Y = 6;
	}

	public GWindowsTabPanel()
		: this(0, 0, 200, 200, null)
	{
	}

	public GWindowsTabPanel(int width, int height, string category)
		: this(0, 0, width, height, category)
	{
	}

	public GWindowsTabPanel(int x, int y, int width, int height, string category)
		: base(x, y)
	{
		base.m_CanDrag = false;
		base.m_QuickDrag = false;
		this.m_Width = width;
		this.m_Height = height;
		this.m_Client = new GEmpty(0, 0, 0, 0);
		base.m_Children.Add(this.m_Client);
		this.m_Category = category;
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
	}

	public virtual void Close()
	{
		Gumps.Destroy(this);
	}

	public void CloseButton_Clicked(object sender, EventArgs e)
	{
	}

	public void TabPullup_Clicked(object sender, EventArgs e, string category)
	{
		Engine.AddTextMessage(category);
		base.BringToTop();
	}
}
