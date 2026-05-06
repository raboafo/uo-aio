namespace UOAIO;

public class ResurrectEffect : Fade
{
	public ResurrectEffect()
		: base(16777215, 0f, 1f, 0.5f)
	{
		Renderer.m_DeathOverride = true;
	}

	protected internal override void OnFadeComplete()
	{
		Mobile player = World.Player;
		if (player != null)
		{
			player.Animation = null;
		}
		Renderer.m_DeathOverride = false;
		Engine.Effects.Add(new Fade(16777215, 1f, 0f, 1f));
	}
}
