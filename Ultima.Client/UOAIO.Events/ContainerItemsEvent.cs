using System.Collections.Generic;

namespace UOAIO.Events;

public class ContainerItemsEvent : IEvent
{
	public readonly List<Item> Items;

	public readonly Item Container;

	public ContainerItemsEvent(Item container, List<Item> items)
	{
		this.Container = container;
		this.Items = items;
	}
}
