using System.Runtime.InteropServices;
using Ultima.Data;

namespace UOAIO;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
public struct Tile
{
	public LandId landId;

	public sbyte z;

	public bool Ignored => this.landId == LandId.NoDraw || this.landId == (LandId)475 || ((int)this.landId >= 430 && (int)this.landId <= 437);

	public bool Visible => this.landId != LandId.NoDraw && this.landId != (LandId)475 && ((int)this.landId < 430 || (int)this.landId > 437);
}
