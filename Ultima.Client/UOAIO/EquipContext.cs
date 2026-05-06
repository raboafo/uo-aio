namespace UOAIO;

internal class EquipContext : MoveContext
{
	public EquipContext(Item pickUp, int amount, Mobile dropTo, bool clickFirst)
		: base(pickUp, amount, dropTo, clickFirst)
	{
	}

	protected override void SendDropPacket()
	{
		Network.Send(new PEquipItem(base.pickUp, base.dropTo as Mobile));
	}
}
