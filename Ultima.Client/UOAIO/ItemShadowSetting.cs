using UOAIO.Profiles;

namespace UOAIO;

public class ItemShadowSetting : IVisualizableSetting
{
	public bool Enabled
	{
		get
		{
			return Preferences.Current.RenderSettings.ItemShadows;
		}
		set
		{
			Preferences.Current.RenderSettings.ItemShadows = value;
		}
	}

	public string LabelKey => this.Enabled ? "shadows" : "shadowless";

	public void Draw(int x, int y)
	{
		int num = x + 34;
		int num2 = y + 76;
		if (this.Enabled)
		{
			Texture item = Hues.Shadow.GetItem(4967);
			Renderer.PushAlpha(0.5f);
			item.DrawShadow(num - item.Width / 2, num2 - item.Height + 8, (item.xMax - item.xMin) / 2, item.yMax);
			Renderer.PopAlpha();
		}
		Texture item2 = Hues.Default.GetItem(4967);
		item2.Draw(num - item2.Width / 2, num2 - item2.Height);
	}
}
