using UOAIO.Profiles;

namespace UOAIO;

public class AnimatedCharactersSetting : IVisualizableSetting
{
	public bool Enabled
	{
		get
		{
			return Preferences.Current.RenderSettings.AnimatedCharacters;
		}
		set
		{
			Preferences.Current.RenderSettings.AnimatedCharacters = value;
		}
	}

	public string LabelKey => this.Enabled ? "animated" : "lockstep";

	public void Draw(int x, int y)
	{
		int xCenter = x + 35;
		int yCenter = y + 63;
		int TextureX = 0;
		int TextureY = 0;
		Frame frame = Engine.m_Animations.GetFrame(null, 400, this.Enabled ? 2 : 0, 1, 0, xCenter, yCenter, Hues.Load(1004), ref TextureX, ref TextureY, preserveHue: false);
		frame.Image.Draw(TextureX, TextureY);
	}
}
