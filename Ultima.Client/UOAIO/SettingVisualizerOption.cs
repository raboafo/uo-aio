using System.Collections.Generic;
using System.Windows.Forms;

namespace UOAIO;

public class SettingVisualizerOption : Gump
{
	private IVisualizableSetting _setting;

	private Dictionary<string, Texture> _labels;

	private Texture label;

	public override int Width
	{
		get
		{
			return 68;
		}
		set
		{
		}
	}

	public override int Height
	{
		get
		{
			return 88;
		}
		set
		{
		}
	}

	public SettingVisualizerOption(int index, IVisualizableSetting setting)
		: base(1 + index * 75, 1)
	{
		this._labels = new Dictionary<string, Texture>();
		this._setting = setting;
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return true;
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		base.OnMouseUp(X, Y, mb);
		if (mb == MouseButtons.Left)
		{
			this._setting.Enabled = !this._setting.Enabled;
		}
	}

	protected internal override void OnDispose()
	{
		base.OnDispose();
		this.label.Dispose();
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		this._setting.Draw(X, Y);
		string labelKey = this._setting.LabelKey;
		if (!this._labels.TryGetValue(labelKey, out this.label))
		{
			this._labels.Add(labelKey, this.label = Engine.LoadArchivedTexture($"visualizer/labels/{labelKey}.png"));
		}
		Renderer.PushAlpha(this._setting.Enabled ? 1f : 0.5f);
		this.label.Draw(X + 2, Y + this.Height - 20, 2241133);
		Renderer.PopAlpha();
		if (Gumps.LastOver == this)
		{
			Renderer.PushAlpha(0.1f);
			Renderer.SetTexture(null);
			Renderer.SolidRect(41471, X, Y, this.Width, this.Height);
			Renderer.PopAlpha();
		}
	}
}
