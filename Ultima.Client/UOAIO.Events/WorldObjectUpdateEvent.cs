namespace UOAIO.Events;

public class WorldObjectUpdateEvent : IEvent
{
	public readonly Item Item;

	public WorldObjectUpdateEvent(Item item)
	{
		this.Item = item;
	}
}
