namespace UOAIO;

public class RenderEffect
{
	private float _alpha;

	private DrawBlendType _blendType;

	private RenderEffectGlow _glow;

	public float Alpha => this._alpha;

	public DrawBlendType BlendType => this._blendType;

	public RenderEffectGlow Glow => this._glow;

	public RenderEffect(float alpha, DrawBlendType blendType, RenderEffectGlow glow)
	{
		this._alpha = alpha;
		this._blendType = blendType;
		this._glow = glow;
	}
}
