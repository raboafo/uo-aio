using System.Runtime.InteropServices;

namespace Ultima.Data;

[StructLayout(LayoutKind.Sequential, Size = 41)]
public struct LandData
{
	public const int Size = 30;

	private TileFlag flags;

	private ushort textureId;

	private unsafe fixed byte name[20];

	public TileFlag Flags => this.flags;

	public ushort TextureId => this.textureId;

	public unsafe string Name
	{
		get
		{
			fixed (byte* name = this.name)
			{
				return MarshalEx.PtrToStringAnsiFixed(name, 20);
			}
		}
	}
}
