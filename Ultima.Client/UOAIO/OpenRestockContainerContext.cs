namespace UOAIO;

internal class OpenRestockContainerContext : UseContext
{
	public OpenRestockContainerContext(Item container)
		: base(container, isManual: false)
	{
	}
}
