using System.Runtime.InteropServices;
using Ultima.Data;

namespace UOAIO;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HuedTile
{
	public ItemId itemId;

	public ushort hueId;

	public sbyte z;

	public HuedTile(StaticTile source)
	{
		this.itemId = source.itemId;
		this.hueId = source.hueId;
		this.z = source.z;
	}
}
