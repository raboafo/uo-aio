using System;
using Ultima.Data;

namespace UOAIO;

public interface IItem : ITile, ICell, IDisposable
{
	ItemId ItemId { get; }

	IHue Hue { get; }

	Texture GetItem(IHue hue, ushort itemID);
}
