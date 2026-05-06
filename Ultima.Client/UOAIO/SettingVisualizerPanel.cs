using System;

namespace UOAIO;

public class SettingVisualizerPanel : Gump
{
	public override int Width
	{
		get
		{
			return 2 + base.Children.Count * 68 + Math.Max(base.Children.Count - 1, 0) * 7;
		}
		set
		{
		}
	}

	public override int Height
	{
		get
		{
			return 90;
		}
		set
		{
		}
	}

	public void AddSetting(IVisualizableSetting setting)
	{
		base.Children.Add(new SettingVisualizerOption(base.Children.Count, setting));
	}

	public SettingVisualizerPanel()
		: base(13, 30)
	{
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return true;
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		Renderer.SetTexture(null);
		Renderer.SolidRect(13421772, X, Y + 1, this.Width, this.Height);
		Renderer.SolidRect(16777215, X + 1, Y + 1, this.Width - 2, this.Height - 22);
		Renderer.GradientRect(16777215, 14540253, X + 1, Y + this.Height - 21, this.Width - 2, 20);
	}
}
