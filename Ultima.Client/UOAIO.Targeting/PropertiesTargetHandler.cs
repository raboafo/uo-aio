namespace UOAIO.Targeting;

internal class PropertiesTargetHandler : ClientTargetHandler
{
	public static bool OnMacro(string args)
	{
		TargetManager.Client = new PropertiesTargetHandler();
		return true;
	}

	protected override bool OnTarget(Item item)
	{
		item.Look();
		Engine.AddTextMessage("ID: " + item.ID);
		Engine.AddTextMessage("Hue: " + item.Hue);
		Engine.AddTextMessage("Location: " + PropertiesTargetHandler.GetLocationFormat(item.X, item.Y, item.Z));
		Engine.AddTextMessage("Layer: " + item.Layer);
		Engine.AddTextMessage("AnimationId: " + item.AnimationId);
		Engine.AddTextMessage("PaperdollItem: " + item.PaperdollItem);
		Engine.AddTextMessage(item.m_LastText + "-------------");
		return true;
	}

	protected override bool OnTarget(GroundTarget groundTarget)
	{
		Engine.AddTextMessage("GroundTarget: " + PropertiesTargetHandler.GetLocationFormat(groundTarget.X, groundTarget.Y, groundTarget.Z));
		return true;
	}

	protected override bool OnTarget(Mobile mob)
	{
		Engine.AddTextMessage("Name: " + mob.Name);
		Engine.AddTextMessage("Body: " + mob.Body);
		Engine.AddTextMessage("Identifier: " + mob.Identifier);
		Engine.AddTextMessage("Location: " + PropertiesTargetHandler.GetLocationFormat(mob.X, mob.Y, mob.Z));
		Engine.AddTextMessage("Hue: " + mob.Hue);
		Engine.AddTextMessage("AnimationId: " + mob.AnimationId);
		Engine.AddTextMessage("Animation: " + mob.Animation);
		Engine.AddTextMessage("-----------------------------------");
		return true;
	}

	protected override bool OnTarget(StaticTarget staticTarget)
	{
		Engine.AddTextMessage("RealID: " + staticTarget.RealID);
		Engine.AddTextMessage("StaticTarget: " + PropertiesTargetHandler.GetLocationFormat(staticTarget.X, staticTarget.Y, staticTarget.Z));
		return true;
	}

	public static string GetLocationFormat(int x, int y, int z)
	{
		return $"({x}, {y}, {z})";
	}
}
