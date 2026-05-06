namespace UOAIO;

public static class RenderEffects
{
	public static readonly RenderEffect Default;

	public static readonly RenderEffect Additive;

	public static readonly RenderEffect AdditiveGlow;

	public static readonly RenderEffect HalfAdditive;

	static RenderEffects()
	{
		RenderEffects.Default = new RenderEffect(1f, DrawBlendType.Normal, null);
		RenderEffects.Additive = new RenderEffect(1f, DrawBlendType.Additive, null);
		RenderEffects.AdditiveGlow = new RenderEffect(0.5f, DrawBlendType.Additive, new RenderEffectGlow(1f, null));
		RenderEffects.HalfAdditive = new RenderEffect(0.5f, DrawBlendType.Additive, null);
	}
}
