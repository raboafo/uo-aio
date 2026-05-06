namespace UOAIO;

public class MultiSampleVisualizerGroup : SettingVisualizerGroup
{
	public MultiSampleVisualizerGroup()
		: base(16, 157, 3, "terrain-anti-aliasing")
	{
		SettingVisualizerPanel settingVisualizerPanel = new SettingVisualizerPanel();
		settingVisualizerPanel.AddSetting(new MultiSampleSetting(0));
		settingVisualizerPanel.AddSetting(new MultiSampleSetting(1));
		settingVisualizerPanel.AddSetting(new MultiSampleSetting(2));
		base.Children.Add(settingVisualizerPanel);
	}
}
