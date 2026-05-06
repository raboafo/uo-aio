using UOAIO.Prompts;

namespace UOAIO.Targeting;

internal class RenameTargetHandler : ClientTargetHandler
{
	protected override bool OnTarget(Mobile mob)
	{
		if (!mob.IsPet)
		{
			Engine.AddTextMessage("You cannot rename this creature.");
			return true;
		}
		Engine.Prompt = new RenameRequestPrompt(mob);
		return true;
	}
}
