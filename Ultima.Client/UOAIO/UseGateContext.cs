namespace UOAIO;

internal class UseGateContext : UseContext
{
	public UseGateContext(Item gate)
		: base(gate, isManual: true)
	{
	}
}
