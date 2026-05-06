namespace UOAIO;

public class EnvironmentalEffectsVisualizerGroup : SettingVisualizerGroup
{
	public EnvironmentalEffectsVisualizerGroup()
		: base(275, 157, 2, "environmental-effects")
	{
		SettingVisualizerPanel settingVisualizerPanel = new SettingVisualizerPanel();
		settingVisualizerPanel.AddSetting(new CharacterShadowSetting());
		settingVisualizerPanel.AddSetting(new ItemShadowSetting());
		base.Children.Add(settingVisualizerPanel);
	}
}
