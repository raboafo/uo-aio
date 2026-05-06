using Ultima.Data;
using UOAIO;
using UOAIO.Profiles;
using UOAIO.Targeting;

namespace Ultima.Client;

internal class SetRestockSourceTargetHandler : ClientTargetHandler
{
	private readonly bool invoking;

	public SetRestockSourceTargetHandler(bool invoking)
	{
		this.invoking = invoking;
	}

	protected override bool OnTarget(Item item)
	{
		if (item != null && Map.m_ItemFlags[item.ID][TileFlag.Container])
		{
			Item agent = null;
			if (World.Player != null)
			{
				agent = World.Player.FindEquip(Layer.Bank);
			}
			if (!item.IsChildOf(World.Player) || item.IsChildOf(agent))
			{
				RestockAgent restockAgent = Player.Current.RestockAgent;
				restockAgent.SourceContainer = new ItemRef(item);
				if (this.invoking)
				{
					restockAgent.Invoke();
				}
				return true;
			}
			Engine.AddTextMessage("Container must not be on your person.");
			return false;
		}
		Engine.AddTextMessage("Target a container.");
		return false;
	}
}
