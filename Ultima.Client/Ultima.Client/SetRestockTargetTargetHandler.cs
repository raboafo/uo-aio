using Ultima.Data;
using UOAIO;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace Ultima.Client;

internal class SetRestockTargetTargetHandler : ClientTargetHandler
{
	private readonly bool invoking;

	public SetRestockTargetTargetHandler(bool invoking)
	{
		this.invoking = invoking;
	}

	protected override bool OnTarget(Item item)
	{
		if (item != null && Map.m_ItemFlags[item.ID][TileFlag.Container])
		{
			if (item.IsChildOf(World.Player))
			{
				RestockAgent restockAgent = Player.Current.RestockAgent;
				restockAgent.TargetContainer = new ItemRef(item);
				if (this.invoking)
				{
					restockAgent.Invoke();
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
