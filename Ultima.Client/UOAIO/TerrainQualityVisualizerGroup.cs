namespace UOAIO;

public class TerrainQualityVisualizerGroup : SettingVisualizerGroup
{
	public TerrainQualityVisualizerGroup()
		: base(16, 16, 3, "terrain-quality")
	{
		SettingVisualizerPanel settingVisualizerPanel = new SettingVisualizerPanel();
		settingVisualizerPanel.AddSetting(new TerrainQualitySetting(0));
		settingVisualizerPanel.AddSetting(new TerrainQualitySetting(1));
		settingVisualizerPanel.AddSetting(new TerrainQualitySetting(2));
		base.Children.Add(settingVisualizerPanel);
	}
}
