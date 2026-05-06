using UOAIO;

namespace UOAIOPlugins.Display;

public class GPlayerInfo : GLabel
{
	private IHue[] m_Hues;

	public Mobile player = World.Player;

	public int maxWeight;

	public GPlayerInfo()
		: base("", Engine.DefaultFont, Hues.Load(23), Engine.GameX, Engine.GameY - 24)
	{
	}

	protected internal override void Render(int X, int Y)
	{
		this.Text = $"Health: {this.player.CurrentHitPoints} Mana: {this.player.CurrentMana} Stamina: {this.player.CurrentStamina} Armor: {this.player.Armor} Weight: {this.player.Weight}/{this.maxWeight = 40 + (int)(3.5 * (double)World.Player.Strength)} Gold: {this.player.Gold}";
		base.Render(X, Y);
	}
}
