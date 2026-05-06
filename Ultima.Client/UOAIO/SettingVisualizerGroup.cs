namespace UOAIO;

public class SettingVisualizerGroup : Gump
{
	private int _settingCount;

	private Texture _title;

	public override int Width
	{
		get
		{
			int num = 85;
			return num * this._settingCount;
		}
		set
		{
		}
	}

	public override int Height
	{
		get
		{
			return 136;
		}
		set
		{
		}
	}

	public SettingVisualizerGroup(int x, int y, int settingCount, string titleKey)
		: base(x, y)
	{
		this._settingCount = settingCount;
		this._title = Engine.LoadArchivedTexture($"visualizer/titles/{titleKey}.png");
	}

	protected internal override void OnDispose()
	{
		base.OnDispose();
		this._title.Dispose();
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return false;
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		Renderer.SetTexture(null);
		Renderer.SolidRect(13421772, X, Y, this.Width, this.Height);
		Renderer.SolidRect(15658734, X + 1, Y + 5, this.Width - 2, this.Height - 2);
		this._title.Draw(X + 5, Y + 5, 3342387);
	}
}
