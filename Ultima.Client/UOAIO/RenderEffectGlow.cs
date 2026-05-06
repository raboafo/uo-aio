namespace UOAIO;

public class RenderEffectGlow
{
	private float _alpha;

	private int? _color;

	public float Alpha => this._alpha;

	public int? Color => this._color;

	public RenderEffectGlow(float alpha, int? color)
	{
		this._alpha = alpha;
		this._color = color;
	}
}
