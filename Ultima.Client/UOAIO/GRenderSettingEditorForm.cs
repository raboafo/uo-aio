using System.Drawing;
using System.Reflection;
using UOAIO.Profiles;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GRenderSettingEditorForm : GWindowsForm
{
	private static readonly string[] TerrainQualityOptions = new string[3] { "Low", "Medium", "High" };

	private static readonly string[] CharacterQualityOptions = new string[2] { "Smooth", "Animated" };

	private static readonly string[] TerrainAntiAliasingOptions = new string[3] { "None", "2x", "4x" };

	private static GRenderSettingEditorForm m_Instance;

	private readonly GComboBox m_TerrainQualityCombo;

	private readonly GComboBox m_CharacterQualityCombo;

	private readonly GComboBox m_TerrainAntiAliasingCombo;

	private readonly GWindowsButton m_ShadowsToggleButton;

	private readonly GWindowsTextBox m_GameXTextBox;

	private readonly GWindowsTextBox m_GameYTextBox;

	private readonly GWindowsTextBox m_GameWidthTextBox;

	private readonly GWindowsTextBox m_GameHeightTextBox;

	private Rectangle m_LastObservedLiveBounds;

	public static bool IsOpen => GRenderSettingEditorForm.m_Instance != null;

	public static void Open()
	{
		if (GRenderSettingEditorForm.m_Instance == null)
		{
			GRenderSettingEditorForm.m_Instance = new GRenderSettingEditorForm();
			Gumps.Desktop.Children.Add(GRenderSettingEditorForm.m_Instance);
			Gumps.Focus = GRenderSettingEditorForm.m_Instance;
		}
	}

	public GRenderSettingEditorForm()
		: base(0, 0, 478, 360)
	{
		base.Text = "Display Settings";
		this.Center();
		this.BuildBackground();
		int num = 16;
		int num2 = 14;
		int num3 = 152;
		int num4 = 214;
		int num5 = 24;
		int num6 = 34;
		base.Client.Children.Add(this.CreateSectionLabel("Quality", num, num2));
		num2 += 28;
		this.m_TerrainQualityCombo = this.AddComboRow("Terrain Quality", TerrainQualityOptions, this.GetTerrainQualityIndex(), num, num2, num3, num4, num5);
		num2 += num6;
		this.m_CharacterQualityCombo = this.AddComboRow("Character Quality", CharacterQualityOptions, this.GetCharacterQualityIndex(), num, num2, num3, num4, num5);
		num2 += num6;
		this.m_TerrainAntiAliasingCombo = this.AddComboRow("Terrain Anti-Aliasing", TerrainAntiAliasingOptions, this.GetTerrainAntiAliasingIndex(), num, num2, num3, num4, num5);
		num2 += num6;
		base.Client.Children.Add(this.CreateLabel("Shadows", num, num2 + 4));
		this.m_ShadowsToggleButton = new GWindowsButton(this.GetShadowsButtonText(this.AreShadowsEnabled()), num + num3, num2, 70, num5);
		this.m_ShadowsToggleButton.Style = WindowsButtonStyle.Flat;
		this.m_ShadowsToggleButton.OnClick = ToggleShadows_OnClick;
		base.Client.Children.Add(this.m_ShadowsToggleButton);
		num2 += 48;
		base.Client.Children.Add(this.CreateSectionLabel("Game Window", num, num2));
		num2 += 28;
		base.Client.Children.Add(this.CreateLabel("Position", num, num2 + 4));
		base.Client.Children.Add(this.CreateLabel("X", num + num3, num2 + 4));
		this.m_GameXTextBox = this.CreateNumberTextBox(num + num3 + 20, num2, 64, num5);
		base.Client.Children.Add(this.m_GameXTextBox);
		base.Client.Children.Add(this.CreateLabel("Y", num + num3 + 102, num2 + 4));
		this.m_GameYTextBox = this.CreateNumberTextBox(num + num3 + 122, num2, 64, num5);
		base.Client.Children.Add(this.m_GameYTextBox);
		num2 += num6;
		base.Client.Children.Add(this.CreateLabel("Size", num, num2 + 4));
		base.Client.Children.Add(this.CreateLabel("W", num + num3, num2 + 4));
		this.m_GameWidthTextBox = this.CreateNumberTextBox(num + num3 + 20, num2, 64, num5);
		base.Client.Children.Add(this.m_GameWidthTextBox);
		base.Client.Children.Add(this.CreateLabel("H", num + num3 + 102, num2 + 4));
		this.m_GameHeightTextBox = this.CreateNumberTextBox(num + num3 + 122, num2, 64, num5);
		base.Client.Children.Add(this.m_GameHeightTextBox);
		// num2 += num6;
		// base.Client.Children.Add(this.CreateHintLabel(string.Format("Minimum size: {0} x {1}", Engine.MinimumGameWidth, Engine.MinimumGameHeight), num, num2 + 2));
		GWindowsButton gWindowsButton = new GWindowsButton("Load Defaults", 170, 286, 110, 24);
		gWindowsButton.Style = WindowsButtonStyle.Flat;
		gWindowsButton.OnClick = LoadDefaults_OnClick;
		base.Client.Children.Add(gWindowsButton);
		GWindowsButton gWindowsButton2 = new GWindowsButton("Cancel", 290, 286, 78, 24);
		gWindowsButton2.Style = WindowsButtonStyle.Flat;
		gWindowsButton2.OnClick = Cancel_OnClick;
		base.Client.Children.Add(gWindowsButton2);
		GWindowsButton gWindowsButton3 = new GWindowsButton("Apply", 376, 286, 78, 24);
		gWindowsButton3.Style = WindowsButtonStyle.Flat;
		gWindowsButton3.CanEnter = true;
		gWindowsButton3.OnClick = Apply_OnClick;
		base.Client.Children.Add(gWindowsButton3);
		this.m_LastObservedLiveBounds = this.GetLiveGameBounds();
		this.SetWindowDraftFields(this.m_LastObservedLiveBounds);
	}

	protected internal override void Draw(int x, int y)
	{
		this.SyncWindowFieldsToLiveBounds();
		base.Draw(x, y);
	}

	protected internal override void OnDispose()
	{
		GRenderSettingEditorForm.m_Instance = null;
		base.OnDispose();
	}

	private void BuildBackground()
	{
		GAlphaBackground gAlphaBackground = new GAlphaBackground(0, 0, base.Client.Width, base.Client.Height);
		gAlphaBackground.FillColor = GumpColors.Control;
		gAlphaBackground.RightColor = GumpColors.Control;
		gAlphaBackground.BorderColor = GumpColors.ControlDarkDark;
		gAlphaBackground.FillAlpha = 1f;
		gAlphaBackground.ShouldHitTest = false;
		base.Client.Children.Add(gAlphaBackground);
	}

	private GComboBox AddComboRow(string label, string[] options, int index, int x, int y, int labelWidth, int comboWidth, int comboHeight)
	{
		base.Client.Children.Add(this.CreateLabel(label, x, y + 4));
		GComboBox gComboBox = new GComboBox(options, index, x + labelWidth, y, comboWidth, comboHeight, Engine.GetUniFont(2), GumpHues.ControlText, GumpHues.Highlight);
		base.Client.Children.Add(gComboBox);
		return gComboBox;
	}

	private GLabel CreateSectionLabel(string text, int x, int y)
	{
		return new GLabel(text, Engine.GetUniFont(2), GumpHues.WindowText, x, y);
	}

	private GLabel CreateLabel(string text, int x, int y)
	{
		return new GLabel(text, Engine.GetUniFont(2), GumpHues.ControlText, x, y);
	}

	private GLabel CreateHintLabel(string text, int x, int y)
	{
		return new GLabel(text, Engine.GetUniFont(1), GumpHues.GrayText, x, y);
	}

	private GWindowsTextBox CreateNumberTextBox(int x, int y, int width, int height)
	{
		GWindowsTextBox gWindowsTextBox = new GWindowsTextBox(x, y, width, height, "", Engine.GetUniFont(2), GumpHues.ControlText, GumpHues.ControlText, GumpHues.ControlText, '\0');
		gWindowsTextBox.Style = WindowsTextBoxStyle.Flat;
		return gWindowsTextBox;
	}

	private Rectangle GetLiveGameBounds()
	{
		return new Rectangle(Engine.GameX, Engine.GameY, Engine.GameWidth, Engine.GameHeight);
	}

	private void SyncWindowFieldsToLiveBounds()
	{
		Rectangle liveGameBounds = this.GetLiveGameBounds();
		if (liveGameBounds != this.m_LastObservedLiveBounds)
		{
			this.m_LastObservedLiveBounds = liveGameBounds;
			this.SetWindowDraftFields(liveGameBounds);
		}
	}

	private void SetWindowDraftFields(Rectangle bounds)
	{
		this.m_GameXTextBox.TextBox.String = bounds.X.ToString();
		this.m_GameYTextBox.TextBox.String = bounds.Y.ToString();
		this.m_GameWidthTextBox.TextBox.String = bounds.Width.ToString();
		this.m_GameHeightTextBox.TextBox.String = bounds.Height.ToString();
	}

	private int GetTerrainQualityIndex()
	{
		int terrainQuality = Preferences.Current.RenderSettings.TerrainQuality;
		if (terrainQuality < 0)
		{
			return 0;
		}
		if (terrainQuality > 2)
		{
			return 2;
		}
		return terrainQuality;
	}

	private int GetCharacterQualityIndex()
	{
		RenderSettings renderSettings = Preferences.Current.RenderSettings;
		if (renderSettings.SmoothCharacters && !renderSettings.AnimatedCharacters)
		{
			return 0;
		}
		return 1;
	}

	private int GetTerrainAntiAliasingIndex()
	{
		int smoothingMode = Preferences.Current.RenderSettings.SmoothingMode;
		if (smoothingMode < 0)
		{
			return 0;
		}
		if (smoothingMode > 2)
		{
			return 2;
		}
		return smoothingMode;
	}

	private bool AreShadowsEnabled()
	{
		RenderSettings renderSettings = Preferences.Current.RenderSettings;
		return renderSettings.CharacterShadows || renderSettings.ItemShadows;
	}

	private string GetShadowsButtonText(bool enabled)
	{
		return enabled ? "On" : "Off";
	}

	private void ToggleShadows_OnClick(Gump g)
	{
		this.m_ShadowsToggleButton.Text = this.GetShadowsButtonText(this.m_ShadowsToggleButton.Text != "On");
	}

	private void LoadDefaults_OnClick(Gump g)
	{
		this.m_TerrainQualityCombo.Index = 1;
		this.m_CharacterQualityCombo.Index = 1;
		this.m_TerrainAntiAliasingCombo.Index = 1;
		this.m_ShadowsToggleButton.Text = "On";
		ScreenLayout screenLayout = new ScreenLayout();
		this.SetWindowDraftFields(screenLayout.GameBounds);
	}

	private void Cancel_OnClick(Gump g)
	{
		Gumps.Destroy(this);
	}

	private void Apply_OnClick(Gump g)
	{
		int result;
		int result2;
		int result3;
		int result4;
		if (!int.TryParse(this.m_GameXTextBox.TextBox.String, out result) || !int.TryParse(this.m_GameYTextBox.TextBox.String, out result2) || !int.TryParse(this.m_GameWidthTextBox.TextBox.String, out result3) || !int.TryParse(this.m_GameHeightTextBox.TextBox.String, out result4))
		{
			Gumps.MessageBoxOk("Display settings require numeric X, Y, Width, and Height values.", Modal: false, null);
			return;
		}
		RenderSettings renderSettings = Preferences.Current.RenderSettings;
		bool flag = renderSettings.TerrainQuality != this.m_TerrainQualityCombo.Index;
		renderSettings.TerrainQuality = this.m_TerrainQualityCombo.Index;
		renderSettings.SmoothingMode = this.m_TerrainAntiAliasingCombo.Index;
		if (this.m_CharacterQualityCombo.Index == 0)
		{
			renderSettings.SmoothCharacters = true;
			renderSettings.AnimatedCharacters = false;
		}
		else
		{
			renderSettings.SmoothCharacters = false;
			renderSettings.AnimatedCharacters = true;
		}
		bool flag2 = this.m_ShadowsToggleButton.Text == "On";
		renderSettings.CharacterShadows = flag2;
		renderSettings.ItemShadows = flag2;
		Engine.ApplyGameViewportBounds(result, result2, result3, result4, rebuildChrome: true);
		if (flag)
		{
			TerrainMeshProviders.Reset();
			Renderer.SetupTerrainFormats();
		}
		UOAIO.Profiles.Config.Current.Save();
		this.m_LastObservedLiveBounds = this.GetLiveGameBounds();
		this.SetWindowDraftFields(this.m_LastObservedLiveBounds);
	}
}
