namespace UOAIO;

public class CharacterQualityVisualizerGroup : SettingVisualizerGroup
{
	public CharacterQualityVisualizerGroup()
		: base(275, 16, 2, "character-quality")
	{
		SettingVisualizerPanel settingVisualizerPanel = new SettingVisualizerPanel();
		settingVisualizerPanel.AddSetting(new SmoothCharactersSetting());
		settingVisualizerPanel.AddSetting(new AnimatedCharactersSetting());
		base.Children.Add(settingVisualizerPanel);
	}
}
