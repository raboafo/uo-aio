using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Veritas;

namespace UOAIO.Profiles;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class ScreenLayout : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private Rectangle m_GameBounds;

	private Rectangle m_ScreenBounds;

	private bool m_Maximized;

	private Size m_FullSize;

	private bool m_Fullscreen;

	private GumpLayoutCollection m_Gumps;

	private bool m_Applying;

	public override PersistableType TypeID => ScreenLayout.TypeCode;

	public Rectangle ScreenBounds
	{
		get
		{
			return this.m_ScreenBounds;
		}
		set
		{
			this.m_ScreenBounds = value;
		}
	}

	public Rectangle GameBounds
	{
		get
		{
			return this.m_GameBounds;
		}
		set
		{
			this.m_GameBounds = value;
		}
	}

	public bool Maximized
	{
		get
		{
			return this.m_Maximized;
		}
		set
		{
			this.m_Maximized = value;
		}
	}

	public Size FullSize
	{
		get
		{
			return this.m_FullSize;
		}
		set
		{
			this.m_FullSize = value;
		}
	}

	public bool Fullscreen
	{
		get
		{
			return this.m_Fullscreen;
		}
		set
		{
			this.m_Fullscreen = value;
		}
	}

	public GumpLayoutCollection Gumps
	{
		get
		{
			return this.m_Gumps;
		}
		set
		{
			this.m_Gumps = value;
		}
	}

	private static PersistableObject Construct()
	{
		return new ScreenLayout(isLoading: true);
	}

	public ScreenLayout()
		: this(isLoading: false)
	{
	}

	private ScreenLayout(bool isLoading)
	{
		this.m_Gumps = new GumpLayoutCollection();
		if (!isLoading)
		{
			Screen screen = Screen.FromControl(Engine.m_Display);
			Rectangle workingArea = screen.WorkingArea;
			Size size = new Size(800, 600);
			Size fullSize = new Size(1024, 768);
			this.m_GameBounds = new Rectangle((fullSize.Width - size.Width) / 2, (fullSize.Height - size.Height) / 2, size.Width, size.Height);
			this.m_ScreenBounds = new Rectangle(workingArea.X + (workingArea.Width - fullSize.Width) / 2, workingArea.Y + (workingArea.Height - fullSize.Height) / 2, fullSize.Width, fullSize.Height);
			this.m_Maximized = false;
			this.m_FullSize = fullSize;
			this.m_Fullscreen = false;
		}
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetRectangle("game", this.m_GameBounds);
		op.SetRectangle("screen", this.m_ScreenBounds);
		op.SetBoolean("maximized", this.m_Maximized);
		op.SetSize("full", this.m_FullSize);
		op.SetBoolean("fullscreen", this.m_Fullscreen);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_GameBounds = ip.GetRectangle("game");
		this.m_ScreenBounds = ip.GetRectangle("screen");
		this.m_Maximized = ip.GetBoolean("maximized");
		this.m_FullSize = ip.GetSize("full");
		this.m_Fullscreen = ip.GetBoolean("fullscreen");
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.m_Gumps.Count; i++)
		{
			this.m_Gumps[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Gumps.Add(ip.GetChild() as GumpLayout);
		}
	}

	public void Update()
	{
		if (!this.m_Applying)
		{
			Display display = Engine.m_Display;
			this.m_GameBounds = new Rectangle(Engine.GameX, Engine.GameY, Engine.GameWidth, Engine.GameHeight);
			if (!this.m_Fullscreen)
			{
				this.m_Maximized = display.WindowState == FormWindowState.Maximized;
			}
			if (!this.m_Maximized && !this.m_Fullscreen && display.WindowState == FormWindowState.Normal)
			{
				this.m_ScreenBounds = new Rectangle(display.PointToScreen(new Point(0, 0)), display.ClientSize);
			}
			Engine.ScreenWidth = display.ClientSize.Width;
			Engine.ScreenHeight = display.ClientSize.Height;
		}
	}

	public void Resize(int gameHeight, int gameWidth)
	{
		if (!this.m_Applying)
		{
			Display display = Engine.m_Display;
			this.m_GameBounds = new Rectangle(Engine.GameX, Engine.GameY, Engine.GameWidth, Engine.GameHeight);
			if (!this.m_Fullscreen)
			{
				this.m_Maximized = display.WindowState == FormWindowState.Maximized;
			}
			if (!this.m_Maximized && !this.m_Fullscreen && display.WindowState == FormWindowState.Normal)
			{
				this.m_ScreenBounds = new Rectangle(display.PointToScreen(new Point(0, 0)), display.ClientSize);
			}
			Engine.ScreenWidth = gameHeight;
			Engine.ScreenHeight = gameHeight;
		}
	}

	public void Apply(bool applyGumps)
	{
		if (this.m_Applying)
		{
			return;
		}
		this.m_Applying = true;
		try
		{
			Display display = Engine.m_Display;
			Engine.GameX = this.m_GameBounds.X;
			Engine.GameY = this.m_GameBounds.Y;
			Engine.GameWidth = this.m_GameBounds.Width;
			Engine.GameHeight = this.m_GameBounds.Height;
			if (this.m_Fullscreen)
			{
				display.FormBorderStyle = FormBorderStyle.None;
				display.WindowState = FormWindowState.Maximized;
			}
			else if (this.m_Maximized)
			{
				display.FormBorderStyle = FormBorderStyle.Sizable;
				display.WindowState = FormWindowState.Maximized;
			}
			else
			{
				Size frameBorderSize = SystemInformation.FrameBorderSize;
				int captionHeight = SystemInformation.CaptionHeight;
				display.FormBorderStyle = FormBorderStyle.Sizable;
				display.WindowState = FormWindowState.Normal;
				display.Location = new Point(this.m_ScreenBounds.X - frameBorderSize.Width, this.m_ScreenBounds.Y - frameBorderSize.Height - captionHeight);
				display.ClientSize = this.m_ScreenBounds.Size;
			}
			int num = this.m_GameBounds.Width * this.m_GameBounds.Height;
			int num2 = (Renderer.blockHeight = (Renderer.blockWidth = ((num >= 1920000) ? 11 : ((num >= 1310720) ? 9 : ((num >= 480000) ? 7 : ((num < 307200) ? 3 : 5))))));
			Renderer.cellWidth = num2 << 3;
			Renderer.cellHeight = num2 << 3;
			if (!applyGumps)
			{
				return;
			}
			foreach (GumpLayout gump2 in this.m_Gumps)
			{
				Gump gump = gump2.CreateGump();
				gump2.Setup(gump);
				gump.SetLayout(gump2);
				UOAIO.Gumps.Desktop.Children.Add(gump);
			}
		}
		finally
		{
			this.m_Applying = false;
		}
	}

	static ScreenLayout()
	{
		ScreenLayout.TypeCode = new PersistableType("screenLayout", Construct, SpellIconLayout.TypeCode, SkillIconLayout.TypeCode);
	}
}
