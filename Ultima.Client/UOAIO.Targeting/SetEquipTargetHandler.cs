using UOAIO.Profiles;

namespace UOAIO.Targeting;

internal class SetEquipTargetHandler : ClientTargetHandler
{
	private int m_Index;

	public SetEquipTargetHandler(int index)
	{
		this.m_Index = index;
	}

	protected override bool OnTarget(Item item)
	{
		if (item.IsWearable)
		{
			int quality = Map.GetQuality(item.ID);
			if (quality == 1 || quality == 2)
			{
				Player.Current.EquipAgent.Arms.Assign(this.m_Index, item);
				return true;
			}
		}
		return false;
	}
}
