using System.Reflection;
using UOAIO.Profiles;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GRenderSettingEditorForm : GWindowsForm
{
	private static GRenderSettingEditorForm m_Instance;

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

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
	}

	public GRenderSettingEditorForm()
		: base(0, 0, 478, 536)
	{
		base.Text = "Display Settings";
		this.Center();
		base.Client.Children.Add(new RenderSettingsVisualizer(0, 0));
	}

	protected internal override void OnDispose()
	{
		GRenderSettingEditorForm.m_Instance = null;
		UOAIO.Profiles.Config.Current.Save();
		base.OnDispose();
	}
}
