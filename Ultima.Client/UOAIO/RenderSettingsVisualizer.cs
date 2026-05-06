using System.Windows.Forms;

namespace UOAIO;

public class RenderSettingsVisualizer : Gump
{
	public override int Width
	{
		get
		{
			return 470;
		}
		set
		{
		}
	}

	public override int Height
	{
		get
		{
			return base.Parent.Height;
		}
		set
		{
		}
	}

	public RenderSettingsVisualizer(int x, int y)
		: base(x, y)
	{
		base.Children.Add(new TerrainQualityVisualizerGroup());
		base.Children.Add(new MultiSampleVisualizerGroup());
		base.Children.Add(new CharacterQualityVisualizerGroup());
		base.Children.Add(new EnvironmentalEffectsVisualizerGroup());
		base.Children.Add(new ResizeVisualizerGroup());
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		base.OnMouseDown(X, Y, mb);
		base.BringToTop();
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		Renderer.SetTexture(null);
		Renderer.SolidRect(15658734, X, Y, this.Width, this.Height);
	}
}
