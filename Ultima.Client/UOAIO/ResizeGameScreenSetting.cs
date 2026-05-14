using System.Drawing;
using System.Windows.Forms;
using UOAIO.Profiles;

namespace UOAIO;

public class ResizeGameScreenSetting : IVisualizableSetting
{
	private int _mode;

	private static Texture[] _textures;

	public bool Enabled
	{
		get
		{
			return this._mode switch
			{
				0 => true, 
				1 => true, 
				2 => true, 
				_ => false, 
			};
		}
		set
		{
			switch (this._mode)
			{
			case 0:
				this.Resize(800, 600);
				break;
			case 1:
				this.Resize(1024, 768);
				break;
			case 2:
				this.Resize(1280, 720);
				break;
			}
		}
	}

	public string LabelKey => this._mode switch
	{
		1 => "1024-768", 
		2 => "1280-720", 
		_ => "800-600", 
	};

	private void Resize(int width, int height)
	{
		if (Preferences.Current.Layout.GameBounds.Width == width && Preferences.Current.Layout.GameBounds.Height == height)
		{
			return;
		}
		if (Preferences.Current.Options.AlwaysLight)
		{
			Preferences.Current.Options.AlwaysLight = false;
		}
		Preferences.Current.Layout.Maximized = true;
		Screen screen = Screen.FromControl(Engine.m_Display);
		Rectangle workingArea = screen.WorkingArea;
		Size size = new Size(width, height);
		Size fullSize = new Size(Preferences.Current.Layout.ScreenBounds.Width, Preferences.Current.Layout.ScreenBounds.Height);
		if (width == 1280 && height == 720)
		{
			Preferences.Current.Layout.GameBounds = new Rectangle(fullSize.Width - size.Width / 2 + 75, fullSize.Height - size.Height / 2, size.Width, size.Height);
		}
		else
		{
			Preferences.Current.Layout.GameBounds = new Rectangle(fullSize.Width - size.Width / 2, fullSize.Height - size.Height / 2, size.Width, size.Height);
		}
		Preferences.Current.Layout.ScreenBounds = new Rectangle(workingArea.X + (workingArea.Width - fullSize.Width) / 2, workingArea.Y + (workingArea.Height - fullSize.Height) / 2, fullSize.Width, fullSize.Height);
		Preferences.Current.Layout.Maximized = true;
		Preferences.Current.Layout.FullSize = fullSize;
		Preferences.Current.Layout.Fullscreen = false;
		GumpLayoutCollection gumps = Preferences.Current.Layout.Gumps;
		UOAIO.Profiles.Config.Current.Save();
		Preferences.Current.Layout.Apply(applyGumps: false);
		Preferences.Current.Layout.Update();
		Preferences.Current.Options.AlwaysLight = true;
		Engine.RebuildViewportChrome();
		Preferences.Current.Layout.Gumps = gumps;
		UOAIO.Profiles.Config.Current.Save();
		UOAIO.Profiles.Config.Current.Load();
		Preferences.Current.Layout.Apply(applyGumps: false);
		Preferences.Current.Layout.Update();
		Renderer.Draw();
	}

	public ResizeGameScreenSetting(int mode)
	{
		this._mode = mode;
	}

	public void Draw(int x, int y)
	{
		if (ResizeGameScreenSetting._textures == null)
		{
			ResizeGameScreenSetting._textures = new Texture[3];
			for (int i = 0; i < ResizeGameScreenSetting._textures.Length; i++)
			{
				ResizeGameScreenSetting._textures[i] = Engine.LoadArchivedTexture($"visualizer/rs-gp-{i}.png");
			}
		}
		ResizeGameScreenSetting._textures[this._mode].Draw(x + 2, y + 2);
	}
}
