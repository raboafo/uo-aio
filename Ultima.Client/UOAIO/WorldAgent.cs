namespace UOAIO;

public class WorldAgent : Agent
{
	public WorldAgent()
		: base(-1)
	{
	}

	protected override void OnChildAdded(Agent child)
	{
		base.OnChildAdded(child);
		if (child is PhysicalAgent)
		{
			Map.Update((PhysicalAgent)child);
		}
	}

	protected override void OnChildRemoved(Agent child)
	{
		base.OnChildRemoved(child);
		if (child is PhysicalAgent)
		{
			Map.Update((PhysicalAgent)child);
		}
	}
}
