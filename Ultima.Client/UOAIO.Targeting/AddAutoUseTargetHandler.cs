using UOAIO.Profiles;

namespace UOAIO.Targeting;

internal class AddAutoUseTargetHandler : ClientTargetHandler
{
	protected override bool OnTarget(Item item)
	{
		UseOnceAgent useOnceAgent = Player.Current.UseOnceAgent;
		ItemRef itemRef = useOnceAgent[item];
		if (itemRef != null)
		{
			item.OverrideHue(-1);
			useOnceAgent.Items.Remove(itemRef);
		}
		else
		{
			item.OverrideHue(34);
			useOnceAgent.Items.Add(new ItemRef(item));
		}
		return true;
	}
}
