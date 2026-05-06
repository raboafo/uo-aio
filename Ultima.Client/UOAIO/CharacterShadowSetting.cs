using UOAIO.Profiles;

namespace UOAIO;

public class CharacterShadowSetting : IVisualizableSetting
{
	public bool Enabled
	{
		get
		{
			return Preferences.Current.RenderSettings.CharacterShadows;
		}
		set
		{
			Preferences.Current.RenderSettings.CharacterShadows = value;
		}
	}

	public string LabelKey => this.Enabled ? "shadows" : "shadowless";

	public void Draw(int x, int y)
	{
		int num = x + 35;
		int num2 = y + 63;
		int TextureX = 0;
		int TextureY = 0;
		Frame frame;
		if (this.Enabled)
		{
			Renderer.PushAlpha(0.5f);
			frame = Engine.m_Animations.GetFrame(null, 400, 4, 1, 0, num, num2, Hues.Shadow, ref TextureX, ref TextureY, preserveHue: false);
			frame.Image.DrawShadow(TextureX, TextureY, num - TextureX, num2 - TextureY);
			Renderer.PopAlpha();
		}
		frame = Engine.m_Animations.GetFrame(null, 400, 4, 1, 0, num, num2, Hues.Load(1004), ref TextureX, ref TextureY, preserveHue: false);
		frame.Image.Draw(TextureX, TextureY);
	}
}
