using System.Runtime.InteropServices;

namespace Ultima.Data;

[StructLayout(LayoutKind.Explicit, Size = 41)]
public struct ItemData
{
	public const int Size = 41;

	[FieldOffset(0)]
	public TileFlag Flags;

	[FieldOffset(8)]
	public byte Weight;

	[FieldOffset(9)]
	public byte quality_layer_light;

	[FieldOffset(10)]
	public uint unk_10;

	[FieldOffset(14)]
	public ushort AnimationId;

	[FieldOffset(16)]
	public ushort unk_16;

	[FieldOffset(18)]
	public ushort value;

	[FieldOffset(20)]
	public byte Height;

	[FieldOffset(21)]
	private unsafe fixed byte name[20];

	public byte SurfaceHeight
	{
		get
		{
			if ((this.Flags & TileFlag.Bridge) != 0)
			{
				return (byte)(this.Height / 2);
			}
			return this.Height;
		}
	}

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

	public bool Background => (this.Flags & TileFlag.Background) != 0;
}
