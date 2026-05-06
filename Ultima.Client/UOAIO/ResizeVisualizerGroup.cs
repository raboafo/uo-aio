namespace UOAIO;

public class ResizeVisualizerGroup : SettingVisualizerGroup
{
	public ResizeVisualizerGroup()
		: base(16, 298, 3, "resize-game-screen")
	{
		SettingVisualizerPanel settingVisualizerPanel = new SettingVisualizerPanel();
		settingVisualizerPanel.AddSetting(new ResizeGameScreenSetting(0));
		settingVisualizerPanel.AddSetting(new ResizeGameScreenSetting(1));
		settingVisualizerPanel.AddSetting(new ResizeGameScreenSetting(2));
		base.Children.Add(settingVisualizerPanel);
	}
}
