using System.Runtime.InteropServices;
using Ultima.Data;

namespace UOAIO;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StaticTile
{
	public ItemId itemId;

	public byte x;

	public byte y;

	public sbyte z;

	public ushort hueId;
}
