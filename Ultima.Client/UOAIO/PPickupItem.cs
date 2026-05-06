using System;

namespace UOAIO;

internal class PPickupItem : Packet
{
	public static Item m_Item;

	public PPickupItem(Item item, int amount)
		: base(7, 7)
	{
		PPickupItem.m_Item = item;
		Engine.m_LastAction = DateTime.Now;
		base.m_Stream.Write(item.Serial);
		base.m_Stream.Write(checked((ushort)amount));
	}
}
