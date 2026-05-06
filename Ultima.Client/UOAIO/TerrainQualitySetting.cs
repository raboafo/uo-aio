using UOAIO.Profiles;

namespace UOAIO;

public class TerrainQualitySetting : IVisualizableSetting
{
	private static Texture[] _textures;

	private int _mode;

	public bool Enabled
	{
		get
		{
			return Preferences.Current.RenderSettings.TerrainQuality == this._mode;
		}
		set
		{
			if (value && Preferences.Current.RenderSettings.TerrainQuality != this._mode)
			{
				Preferences.Current.RenderSettings.TerrainQuality = this._mode;
				TerrainMeshProviders.Reset();
				Renderer.SetupTerrainFormats();
			}
		}
	}

	public string LabelKey => this._mode switch
	{
		1 => "medium", 
		2 => "high", 
		_ => "low", 
	};

	public TerrainQualitySetting(int mode)
	{
		this._mode = mode;
	}

	public void Draw(int x, int y)
	{
		if (TerrainQualitySetting._textures == null)
		{
			TerrainQualitySetting._textures = new Texture[3];
			for (int i = 0; i < TerrainQualitySetting._textures.Length; i++)
			{
				TerrainQualitySetting._textures[i] = Engine.LoadArchivedTexture($"visualizer/rs-tq-{i}.png");
			}
		}
		TerrainQualitySetting._textures[this._mode].Draw(x + 2, y + 2);
	}
}
