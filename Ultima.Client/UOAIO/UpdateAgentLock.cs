namespace UOAIO;

public class UpdateAgentLock : ILocked
{
	private PhysicalAgent _agent;

	public UpdateAgentLock(PhysicalAgent agent)
	{
		this._agent = agent;
	}

	public void Invoke()
	{
		Map.Update(this._agent);
	}
}
