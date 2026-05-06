using System.Windows.Forms;

namespace UOAIO;

public class GVolumeControl : Gump
{
	private Texture m_Background;

	private Texture m_Slider;

	public override int Width
	{
		get
		{
			return this.m_Background.Width;
		}
		set
		{
		}
	}

	public override int Height
	{
		get
		{
			return this.m_Background.Height;
		}
		set
		{
		}
	}

	protected internal override void OnDispose()
	{
		base.OnDispose();
		this.m_Background.Dispose();
		this.m_Slider.Dispose();
	}

	public GVolumeControl()
		: base(0, 0)
	{
		this.m_Background = Engine.LoadArchivedTexture("volume.png");
		this.m_Slider = Engine.LoadArchivedTexture("volume_slider.png");
		base.m_Children.Add(new GVolumeSlider(sound: true, this.m_Slider, 13, 48));
		base.m_Children.Add(new GVolumeSlider(sound: false, this.m_Slider, 13, 94));
		base.GUID = "Volume";
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return true;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		if (mb == MouseButtons.Right)
		{
			Gumps.Destroy(this);
		}
	}

	protected internal override void Render(int X, int Y)
	{
		this.X = Engine.GameX + Engine.GameWidth - this.Width;
		this.Y = Engine.GameY;
		base.Render(X, Y);
	}

	protected internal override void Draw(int X, int Y)
	{
		this.m_Background.Draw(X, Y);
	}
}
