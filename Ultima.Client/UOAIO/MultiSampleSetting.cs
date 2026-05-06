using UOAIO.Profiles;

namespace UOAIO;

public class MultiSampleSetting : IVisualizableSetting
{
	private static Texture[] _textures;

	private int _mode;

	public bool Enabled
	{
		get
		{
			return Preferences.Current.RenderSettings.SmoothingMode == this._mode;
		}
		set
		{
			if (value)
			{
				Preferences.Current.RenderSettings.SmoothingMode = this._mode;
			}
		}
	}

	public string LabelKey => this._mode switch
	{
		1 => "2x-medium", 
		2 => "4x-high", 
		_ => "none", 
	};

	public MultiSampleSetting(int mode)
	{
		this._mode = mode;
	}

	public void Draw(int x, int y)
	{
		if (MultiSampleSetting._textures == null)
		{
			MultiSampleSetting._textures = new Texture[3];
			for (int i = 0; i < MultiSampleSetting._textures.Length; i++)
			{
				MultiSampleSetting._textures[i] = Engine.LoadArchivedTexture($"visualizer/rs-ta-{i}.png");
			}
		}
		MultiSampleSetting._textures[this._mode].Draw(x + 2, y + 2);
	}
}
