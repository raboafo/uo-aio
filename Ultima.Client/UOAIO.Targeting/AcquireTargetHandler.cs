namespace UOAIO.Targeting;

internal class AcquireTargetHandler : ClientTargetHandler
{
	public override AggressionType Aggression => AggressionType.Offensive;

	protected override bool OnTarget(Mobile mob)
	{
		mob.AddTextMessage("", "- last target set -", Engine.DefaultFont, Hues.Load(89), unremovable: false);
		if (Party.State == PartyState.Joined)
		{
			string identifier = mob.Identifier;
			if (identifier != null)
			{
				Party.SendAutomatedMessage("Changing last target to {0}", identifier);
			}
		}
		return true;
	}

	protected override bool OnTarget(Item item)
	{
		item.AddTextMessage("", "- last target set -", Engine.DefaultFont, Hues.Load(89), unremovable: false);
		return true;
	}

	protected override bool OnTarget(GroundTarget groundTarget)
	{
		Engine.AddTextMessage("Last target set.", Engine.DefaultFont, Hues.Load(89));
		return true;
	}

	protected override bool OnTarget(StaticTarget staticTarget)
	{
		Engine.AddTextMessage("Last target set.", Engine.DefaultFont, Hues.Load(89));
		return true;
	}
}
