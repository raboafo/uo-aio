using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Ultima.Data;

public sealed class TileData : IDisposable
{
	private const int LandBlockSize = 964;

	private const int ItemBlockSize = 1316;

	private const int LandTableSize = 493568;

	private const int ItemTableSize = 2695168;

	private readonly MemoryMappedFile file;

	private readonly MemoryMappedViewAccessor view;

	private unsafe readonly byte* landTable;

	private unsafe readonly byte* itemTable;

	public unsafe TileData(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		this.file = MemoryMappedFile.CreateFromFile(File.OpenRead(path), null, 0L, MemoryMappedFileAccess.CopyOnWrite, null, HandleInheritability.None, leaveOpen: false);
		this.view = this.file.CreateViewAccessor(0L, 3188736L, MemoryMappedFileAccess.CopyOnWrite);
		this.view.SafeMemoryMappedViewHandle.AcquirePointer(ref this.landTable);
		this.itemTable = this.landTable + 493568;
	}

	public unsafe LandData* GetLandDataPointer(LandId landId)
	{
		byte* source = this.landTable;
		ushort value = (ushort)landId;
		int block = value >> 5;
		int entry = value & 0x1F;
		if (value != 0)
		{
			source += block * 964 + 4 + entry * 30;
		}
		return (LandData*)source;
	}

	public unsafe ItemData* GetItemDataPointer(ItemId itemId)
	{
		byte* source = this.itemTable;
		ushort value = (ushort)itemId;
		int block = value >> 5;
		int entry = value & 0x1F;
		return (ItemData*)(source + (block * 1316 + 4 + entry * 41));
	}

	public void Dispose()
	{
		this.view.SafeMemoryMappedViewHandle.ReleasePointer();
		this.view.Dispose();
		this.file.Dispose();
	}
}
