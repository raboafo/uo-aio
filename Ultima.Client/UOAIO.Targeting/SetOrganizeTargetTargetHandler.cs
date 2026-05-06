using Ultima.Data;
using UOAIO.Profiles;

namespace UOAIO.Targeting;

internal class SetOrganizeTargetTargetHandler : ClientTargetHandler
{
	private readonly bool m_Invoking;

	public SetOrganizeTargetTargetHandler(bool invoking)
	{
		this.m_Invoking = invoking;
	}

	protected override bool OnTarget(Item item)
	{
		if (item != null && Map.m_ItemFlags[item.ID][TileFlag.Container])
		{
			if (item.IsChildOf(World.Player) || item.InRange(World.Player, 2))
			{
				Player.Current.OrganizeAgent.TargetContainer = new ItemRef(item);
				if (this.m_Invoking)
				{
					Player.Current.OrganizeAgent.Invoke();
				}
				return true;
			}
			Engine.AddTextMessage("Container must be on your person.");
			return false;
		}
		Engine.AddTextMessage("Target a container.");
		return false;
	}
}
